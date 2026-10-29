using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace ServidorCajero
{
    // -----------------------------------------------------------------
    // SERVIDOR DE REFERENCIA
    //
    // Implementa el mismo protocolo (Comunicacion/Protocolo.cs) que usa
    // el cliente CajeroAutomatico, para que puedas probar el flujo
    // completo end-to-end. Toda la lógica de negocio que antes vivía en
    // CajeroService (validar PIN, mover saldo, límites diarios,
    // denominaciones, bloqueo de tarjetas, administración de usuarios)
    // ahora vive AQUÍ, del lado del servidor.
    //
    // Es intencionalmente simple (datos en memoria + un archivo de
    // respaldo) para que sea fácil de leer y de extender hacia una base
    // de datos real (SQL Server, SQLite, etc.) sin tocar el protocolo.
    // -----------------------------------------------------------------
    internal class Usuario
    {
        public string NumeroTarjeta;
        public string Nombre;
        public string Pin;
        public string Huella;
        public decimal Saldo;
        public decimal LimiteDiario;
        public decimal RetiradoHoy;
        public DateTime FechaRetiroHoy;
        public int IntentosFallidos;
        public bool Bloqueado;
    }

    internal class Transaccion
    {
        public string NumeroTarjeta;
        public DateTime Fecha;
        public string Tipo; // "Retiro" | "Deposito"
        public decimal Monto;
    }

    internal static class Program
    {
        private const int Puerto = 9000;
        private const int IntentosMaximos = 3;

        private static readonly List<Usuario> _usuarios = new List<Usuario>
        {
            new Usuario
            {
                NumeroTarjeta = "1234567890123456",
                Nombre = "Cliente de Prueba",
                Pin = "1234",
                Huella = "",
                Saldo = 5000m,
                LimiteDiario = 2000m
            }
        };

        private static readonly List<Transaccion> _transacciones = new List<Transaccion>();

        // denominación -> billetes disponibles en el cajero
        private static readonly Dictionary<int, int> _denominaciones = new Dictionary<int, int>
        {
            { 200, 20 }, { 100, 30 }, { 50, 40 }, { 20, 50 }, { 10, 50 }, { 5, 50 }, { 1, 50 }
        };

        private static bool _cajeroActivo = true;

        // token -> número de tarjeta de la sesión autenticada
        private static readonly ConcurrentDictionary<string, string> _sesiones = new ConcurrentDictionary<string, string>();

        private static readonly object _bloqueo = new object();
        private static readonly JavaScriptSerializer _json = new JavaScriptSerializer();

        private static void Main()
        {
            var listener = new TcpListener(IPAddress.Any, Puerto);
            listener.Start();
            Console.WriteLine($"Servidor de cajero escuchando en el puerto {Puerto}...");

            while (true)
            {
                var cliente = listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(_ => AtenderCliente(cliente));
            }
        }

        private static void AtenderCliente(TcpClient cliente)
        {
            try
            {
                using (cliente)
                using (var stream = cliente.GetStream())
                using (var lector = new StreamReader(stream, Encoding.UTF8))
                using (var escritor = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true, NewLine = "\n" })
                {
                    string linea = lector.ReadLine();
                    if (string.IsNullOrEmpty(linea)) return;

                    var solicitud = _json.Deserialize<Dictionary<string, object>>(linea);
                    var respuesta = Procesar(solicitud);

                    escritor.WriteLine(_json.Serialize(respuesta));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error atendiendo cliente: {ex.Message}");
            }
        }

        private static Dictionary<string, object> Procesar(Dictionary<string, object> solicitud)
        {
            string operacion = solicitud != null && solicitud.ContainsKey("Operacion") ? solicitud["Operacion"]?.ToString() : null;
            var datos = solicitud != null && solicitud.ContainsKey("Datos") && solicitud["Datos"] is Dictionary<string, object> d
                ? d
                : new Dictionary<string, object>();

            lock (_bloqueo)
            {
                switch (operacion)
                {
                    case "VALIDAR_TARJETA": return ValidarTarjeta(datos);
                    case "VALIDAR_PIN": return ValidarPin(datos);
                    case "VALIDAR_HUELLA": return ValidarHuella(datos);
                    case "CONSULTAR_SALDO": return ConsultarSaldo(datos);
                    case "RETIRAR": return Retirar(datos);
                    case "DEPOSITAR": return Depositar(datos);
                    case "CAMBIAR_PIN": return CambiarPin(datos);
                    case "ULTIMAS_TRANSACCIONES": return UltimasTransacciones(datos);
                    case "ESTADO_CAJERO": return EstadoCajero();
                    case "CERRAR_SESION": return CerrarSesion(datos);
                    default: return Error("Operación no reconocida.");
                }
            }
        }

        // ---------- Operaciones ----------
        private static Dictionary<string, object> ValidarTarjeta(Dictionary<string, object> datos)
        {
            string numeroTarjeta = Texto(datos, "NumeroTarjeta");
            var usuario = _usuarios.FirstOrDefault(u => u.NumeroTarjeta == numeroTarjeta);

            if (usuario == null) return Error("Tarjeta no reconocida.");
            if (usuario.Bloqueado) return Error("Tarjeta bloqueada. Contacte al administrador.");
            if (!_cajeroActivo) return Error("Cajero inactivo en este momento.");

            return Ok("", new Dictionary<string, object>
            {
                { "Nombre", usuario.Nombre },
                { "Bloqueado", usuario.Bloqueado },
                { "RequiereHuella", !string.IsNullOrEmpty(usuario.Huella) }
            });
        }

        private static Dictionary<string, object> ValidarPin(Dictionary<string, object> datos)
        {
            string numeroTarjeta = Texto(datos, "NumeroTarjeta");
            string pin = Texto(datos, "Pin");
            var usuario = _usuarios.FirstOrDefault(u => u.NumeroTarjeta == numeroTarjeta);

            if (usuario == null) return Error("Tarjeta no reconocida.");
            if (usuario.Bloqueado) return ErrorBloqueado();

            if (usuario.Pin == pin)
            {
                usuario.IntentosFallidos = 0;
                string token = CrearSesion(usuario.NumeroTarjeta);
                return Ok("Ingreso correcto.", new Dictionary<string, object> { { "Token", token } });
            }

            return RegistrarIntentoFallido(usuario);
        }

        private static Dictionary<string, object> ValidarHuella(Dictionary<string, object> datos)
        {
            string numeroTarjeta = Texto(datos, "NumeroTarjeta");
            string huella = Texto(datos, "Huella");
            var usuario = _usuarios.FirstOrDefault(u => u.NumeroTarjeta == numeroTarjeta);

            if (usuario == null) return Error("Tarjeta no reconocida.");
            if (usuario.Bloqueado) return ErrorBloqueado();

            if (!string.IsNullOrEmpty(usuario.Huella) && usuario.Huella == huella)
            {
                usuario.IntentosFallidos = 0;
                string token = CrearSesion(usuario.NumeroTarjeta);
                return Ok("Ingreso correcto.", new Dictionary<string, object> { { "Token", token } });
            }

            return RegistrarIntentoFallido(usuario);
        }

        private static Dictionary<string, object> ConsultarSaldo(Dictionary<string, object> datos)
        {
            var usuario = UsuarioDeSesion(datos, out var error);
            if (usuario == null) return error;

            decimal disponibleHoy = usuario.FechaRetiroHoy.Date == DateTime.Today
                ? usuario.LimiteDiario - usuario.RetiradoHoy
                : usuario.LimiteDiario;

            return Ok("", new Dictionary<string, object>
            {
                { "SaldoActual", usuario.Saldo },
                { "LimiteMaximo", usuario.LimiteDiario },
                { "DisponibleHoy", disponibleHoy }
            });
        }

        private static Dictionary<string, object> Retirar(Dictionary<string, object> datos)
        {
            var usuario = UsuarioDeSesion(datos, out var error);
            if (usuario == null) return error;

            decimal monto = Decimal(datos, "Monto");
            var desglose = Desglose(datos, "Desglose");
            var totalDesglose = desglose.Sum(kv => (decimal)kv.Key * kv.Value);

            if (totalDesglose != monto)
                return Error("El desglose de billetes no coincide con el monto solicitado.");
            if (monto > usuario.Saldo)
                return Error("Saldo insuficiente.");

            if (usuario.FechaRetiroHoy.Date != DateTime.Today)
            {
                usuario.RetiradoHoy = 0;
                usuario.FechaRetiroHoy = DateTime.Today;
            }
            if (usuario.RetiradoHoy + monto > usuario.LimiteDiario)
                return Error("Ese retiro supera su límite diario.");

            foreach (var kv in desglose)
            {
                int disponible = _denominaciones.ContainsKey(kv.Key) ? _denominaciones[kv.Key] : 0;
                if (disponible < kv.Value)
                    return Error($"El cajero no tiene suficientes billetes de Q.{kv.Key}.");
            }

            foreach (var kv in desglose) _denominaciones[kv.Key] -= kv.Value;
            usuario.Saldo -= monto;
            usuario.RetiradoHoy += monto;

            _transacciones.Add(new Transaccion { NumeroTarjeta = usuario.NumeroTarjeta, Fecha = DateTime.Now, Tipo = "Retiro", Monto = monto });

            return Ok($"Retiro de Q.{monto:N2} realizado. Saldo actual: Q.{usuario.Saldo:N2}",
                new Dictionary<string, object> { { "SaldoActual", usuario.Saldo } });
        }

        private static Dictionary<string, object> Depositar(Dictionary<string, object> datos)
        {
            var usuario = UsuarioDeSesion(datos, out var error);
            if (usuario == null) return error;

            var desglose = Desglose(datos, "Desglose");
            var monto = desglose.Sum(kv => (decimal)kv.Key * kv.Value);
            if (monto <= 0) return Error("Debe ingresar al menos un billete para depositar.");

            foreach (var kv in desglose)
                _denominaciones[kv.Key] = (_denominaciones.ContainsKey(kv.Key) ? _denominaciones[kv.Key] : 0) + kv.Value;

            usuario.Saldo += monto;
            _transacciones.Add(new Transaccion { NumeroTarjeta = usuario.NumeroTarjeta, Fecha = DateTime.Now, Tipo = "Deposito", Monto = monto });

            return Ok($"Depósito de Q.{monto:N2} realizado. Saldo actual: Q.{usuario.Saldo:N2}",
                new Dictionary<string, object> { { "SaldoActual", usuario.Saldo } });
        }

        private static Dictionary<string, object> CambiarPin(Dictionary<string, object> datos)
        {
            var usuario = UsuarioDeSesion(datos, out var error);
            if (usuario == null) return error;

            string pinActual = Texto(datos, "PinActual");
            string pinNuevo = Texto(datos, "PinNuevo");
            string confirmacion = Texto(datos, "ConfirmacionPinNuevo");

            if (usuario.Pin != pinActual) return Error("El PIN actual es incorrecto.");
            if (pinNuevo.Length != 4 || !pinNuevo.All(char.IsDigit)) return Error("El nuevo PIN debe tener 4 dígitos.");
            if (pinNuevo != confirmacion) return Error("La confirmación no coincide con el nuevo PIN.");

            usuario.Pin = pinNuevo;
            return Ok("PIN actualizado correctamente.");
        }

        private static Dictionary<string, object> UltimasTransacciones(Dictionary<string, object> datos)
        {
            var usuario = UsuarioDeSesion(datos, out var error);
            if (usuario == null) return error;

            int cantidad = Entero(datos, "Cantidad", 5);
            var lista = _transacciones
                .Where(t => t.NumeroTarjeta == usuario.NumeroTarjeta)
                .OrderByDescending(t => t.Fecha)
                .Take(cantidad)
                .Select(t => new Dictionary<string, object>
                {
                    { "Fecha", t.Fecha.ToString("d") },
                    { "Hora", t.Fecha.ToString("HH:mm") },
                    { "Tipo", t.Tipo },
                    { "Monto", t.Monto }
                })
                .ToList();

            return Ok("", new Dictionary<string, object> { { "Transacciones", lista } });
        }

        private static Dictionary<string, object> EstadoCajero() =>
            Ok("", new Dictionary<string, object> { { "Estado", _cajeroActivo ? "Activo" : "Inactivo" } });

        private static Dictionary<string, object> CerrarSesion(Dictionary<string, object> datos)
        {
            string token = Texto(datos, "Token");
            if (!string.IsNullOrEmpty(token)) _sesiones.TryRemove(token, out _);
            return Ok();
        }

        // ---------- Sesiones ----------
        private static string CrearSesion(string numeroTarjeta)
        {
            string token = Guid.NewGuid().ToString("N");
            _sesiones[token] = numeroTarjeta;
            return token;
        }

        private static Usuario UsuarioDeSesion(Dictionary<string, object> datos, out Dictionary<string, object> error)
        {
            string token = Texto(datos, "Token");
            if (string.IsNullOrEmpty(token) || !_sesiones.TryGetValue(token, out var numeroTarjeta))
            {
                error = Error("Sesión inválida o expirada. Vuelva a ingresar su tarjeta.");
                return null;
            }

            var usuario = _usuarios.FirstOrDefault(u => u.NumeroTarjeta == numeroTarjeta);
            if (usuario == null)
            {
                error = Error("Sesión inválida.");
                return null;
            }

            error = null;
            return usuario;
        }

        private static Dictionary<string, object> RegistrarIntentoFallido(Usuario usuario)
        {
            usuario.IntentosFallidos++;
            if (usuario.IntentosFallidos >= IntentosMaximos) usuario.Bloqueado = true;

            if (usuario.Bloqueado) return ErrorBloqueado();

            int restantes = IntentosMaximos - usuario.IntentosFallidos;
            return Error2($"Credencial incorrecta. Intentos restantes: {restantes}",
                new Dictionary<string, object> { { "IntentosRestantes", restantes }, { "Bloqueado", false } });
        }

        private static Dictionary<string, object> ErrorBloqueado() =>
            Error2("Tarjeta bloqueada por demasiados intentos fallidos.", new Dictionary<string, object> { { "Bloqueado", true } });

        // ---------- Helpers de bajo nivel ----------
        private static string Texto(Dictionary<string, object> d, string clave) =>
            d != null && d.ContainsKey(clave) && d[clave] != null ? d[clave].ToString() : "";

        private static decimal Decimal(Dictionary<string, object> d, string clave) =>
            d != null && d.ContainsKey(clave) && d[clave] != null ? Convert.ToDecimal(d[clave]) : 0m;

        private static int Entero(Dictionary<string, object> d, string clave, int porDefecto) =>
            d != null && d.ContainsKey(clave) && d[clave] != null ? Convert.ToInt32(d[clave]) : porDefecto;

        private static Dictionary<int, int> Desglose(Dictionary<string, object> d, string clave)
        {
            var resultado = new Dictionary<int, int>();
            if (d == null || !d.ContainsKey(clave) || !(d[clave] is Dictionary<string, object> mapa)) return resultado;

            foreach (var kv in mapa)
                resultado[int.Parse(kv.Key)] = Convert.ToInt32(kv.Value);

            return resultado;
        }

        private static Dictionary<string, object> Ok(string mensaje = "", Dictionary<string, object> datos = null) =>
            new Dictionary<string, object> { { "Exito", true }, { "Mensaje", mensaje }, { "Datos", datos ?? new Dictionary<string, object>() } };

        private static Dictionary<string, object> Error(string mensaje) =>
            new Dictionary<string, object> { { "Exito", false }, { "Mensaje", mensaje }, { "Datos", new Dictionary<string, object>() } };

        private static Dictionary<string, object> Error2(string mensaje, Dictionary<string, object> datos) =>
            new Dictionary<string, object> { { "Exito", false }, { "Mensaje", mensaje }, { "Datos", datos } };
    }
}
