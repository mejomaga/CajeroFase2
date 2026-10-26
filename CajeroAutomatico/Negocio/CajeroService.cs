using System;
using System.Collections.Generic;
using System.Linq;
using CajeroAutomatico.Datos;
using CajeroAutomatico.Entidades;
using CajeroAutomatico.Presentacion;

namespace CajeroAutomatico.Negocio
{
    // Capa de Negocio: toda la lógica y reglas del cajero. Los Forms
    // (capa de Presentación) solo llaman a estos métodos; nunca acceden
    // a RepositorioArchivos directamente.
    public class CajeroService
    {
        private readonly IArduinoService _arduino;
        private List<Usuario> _usuarios;
        private CajeroConfig _config;

        public static decimal MontoMaximoInicializacion = 10000m;
        public static decimal MontoMaximoAgregarEfectivo = 30000m;
        public const int IntentosMaximos = 3;

        public CajeroService(IArduinoService arduino)
        {
            _arduino = arduino;
            RepositorioArchivos.AsegurarArchivos();
            _usuarios = RepositorioArchivos.CargarUsuarios();
            _config = RepositorioArchivos.CargarConfig();
            SembrarAdministradorSiNoExiste();
        }

        // ---------- Lector de tarjetas (Arduino) ----------
        public event Action<string> TarjetaDetectada
        {
            add { _arduino.TarjetaDetectada += value; }
            remove { _arduino.TarjetaDetectada -= value; }
        }

        public void IniciarLector()
        {
            _arduino.Iniciar();
        }

        public void DetenerLector()
        {
            _arduino.Detener();
        }

        // ---------- Eventos de huella ----------
        public event Action<int> HuellaRegistrada
        {
            add { _arduino.HuellaRegistrada += value; }
            remove { _arduino.HuellaRegistrada -= value; }
        }

        public event Action<int> HuellaDetectada
        {
            add { _arduino.HuellaDetectada += value; }
            remove { _arduino.HuellaDetectada -= value; }
        }

        public void RegistrarHuella()
        {
            _arduino.RegistrarHuella();
        }

        public void BorrarHuella(int slot)
        {
            _arduino.BorrarHuella(slot);
        }

        public void ModoEsperarHuella(bool esperar)
        {
            _arduino.ModoEsperarHuella(esperar);
        }

        // ---------- Búsqueda por UID ----------
        public Usuario BuscarPorUID(string uid) =>
            _usuarios.FirstOrDefault(u =>
                !string.IsNullOrEmpty(u.UID) &&
                u.UID.Equals(uid, StringComparison.OrdinalIgnoreCase));

        public (Usuario usuario, bool permitido, string mensaje) ValidarTarjetaPorUID(string uid)
        {
            var usuario = BuscarPorUID(uid);
            if (usuario == null)
                return (null, false, "Tarjeta no reconocida.");

            if (usuario.Bloqueado)
                return (usuario, false, "Tarjeta bloqueada. Contacte al administrador.");

            if (_config.Estado == EstadoCajero.Inactivo && usuario.Tipo != TipoUsuario.Administrador)
                return (usuario, false, "Cajero inactivo. Solo el administrador puede ingresar.");

            return (usuario, true, "");
        }

        // ---------- Búsqueda por huella ----------
        public Usuario BuscarPorHuella(int slot) =>
            _usuarios.FirstOrDefault(u => u.HuellaSlot == slot);

        public (Usuario usuario, bool permitido, string mensaje) ValidarHuella(int slot)
        {
            var usuario = BuscarPorHuella(slot);
            if (usuario == null)
                return (null, false, "Huella no reconocida.");

            if (usuario.Bloqueado)
                return (usuario, false, "Tarjeta bloqueada. Contacte al administrador.");

            if (_config.Estado == EstadoCajero.Inactivo && usuario.Tipo != TipoUsuario.Administrador)
                return (usuario, false, "Cajero inactivo. Solo el administrador puede ingresar.");

            return (usuario, true, "");
        }


        // ---------- Modificar tarjeta completa (número + UID) ----------
        public ResultadoOperacion ModificarTarjetaCompleta(Usuario usuario, string nuevoNumero, string nuevoUID)
        {
            if (nuevoNumero.Length != 16 || !nuevoNumero.All(char.IsDigit))
                return ResultadoOperacion.Error("El nuevo número de tarjeta debe tener 16 dígitos.");

            if (_usuarios.Any(u => u != usuario && u.NumeroTarjeta == nuevoNumero))
                return ResultadoOperacion.Error("El nuevo número de tarjeta ya está en uso.");

            if (string.IsNullOrWhiteSpace(nuevoUID))
                return ResultadoOperacion.Error("Debe vincular la tarjeta física nueva.");

            if (_usuarios.Any(u => u != usuario &&
                !string.IsNullOrEmpty(u.UID) &&
                u.UID.Equals(nuevoUID, StringComparison.OrdinalIgnoreCase)))
                return ResultadoOperacion.Error("Esa tarjeta física ya está vinculada a otro usuario.");

            usuario.HistorialTarjetas.Add(usuario.NumeroTarjeta);
            usuario.NumeroTarjeta = nuevoNumero;
            usuario.UID = nuevoUID;

            GuardarUsuarios();
            return ResultadoOperacion.Ok("Tarjeta y número actualizados correctamente.");
        }

        // ---------- Asignar huella a un usuario ----------
        public ResultadoOperacion AsignarHuella(Usuario usuario, int slot)
        {
            if (slot <= 0)
                return ResultadoOperacion.Error("Slot de huella inválido.");

            // Verificar que ningún otro usuario tenga ese slot
            if (_usuarios.Any(u => u != usuario && u.HuellaSlot == slot))
                return ResultadoOperacion.Error("Ese slot de huella ya está asignado a otro usuario.");

            usuario.HuellaSlot = slot;
            GuardarUsuarios();
            return ResultadoOperacion.Ok("Huella asignada correctamente.");
        }

        // La primera vez que corre el programa no hay ningún usuario en el
        // CSV, así que nadie podría entrar nunca. Se crea un administrador
        // por defecto para poder probar el sistema (tarjeta 0000000000000001,
        // PIN 0000). Bórralo o cámbialo desde "Modificar tarjeta"/"Crear usuario"
        // una vez tengan sus propios datos de prueba.
        private void SembrarAdministradorSiNoExiste()
        {
            if (_usuarios.Any(u => u.Tipo == TipoUsuario.Administrador)) return;

            _usuarios.Add(new Usuario
            {
                Nombre = "Administrador",
                NumeroTarjeta = "0000000000000001",
                Pin = "0000",
                Tipo = TipoUsuario.Administrador,
                Saldo = 0,
                LimiteDiario = 0,
                UltimoAcceso = DateTime.MinValue,
                FechaRetiroHoy = DateTime.MinValue
            });
            GuardarUsuarios();
        }

        // ---------- Arranque ----------
        public bool VerificarConexionArchivos() => RepositorioArchivos.VerificarConexion();
        public bool VerificarConexionArduino() => _arduino.VerificarConexion();
        public EstadoCajero ObtenerEstado() => _config.Estado;

        // ---------- Autenticación ----------
        public Usuario BuscarPorTarjeta(string numeroTarjeta) =>
            _usuarios.FirstOrDefault(u => u.NumeroTarjeta == numeroTarjeta || u.HistorialTarjetas.Contains(numeroTarjeta));

        public (Usuario usuario, bool permitido, string mensaje) ValidarTarjeta(string numeroTarjeta)
        {
            var usuario = BuscarPorTarjeta(numeroTarjeta);
            if (usuario == null)
                return (null, false, "Tarjeta no reconocida.");

            if (usuario.Bloqueado)
                return (usuario, false, "Tarjeta bloqueada. Contacte al administrador.");

            if (_config.Estado == EstadoCajero.Inactivo && usuario.Tipo != TipoUsuario.Administrador)
                return (usuario, false, "Cajero inactivo. Solo el administrador puede ingresar.");

            return (usuario, true, "");
        }

        public bool ValidarPin(Usuario usuario, string pin)
        {
            if (usuario.Pin == pin)
            {
                usuario.IntentosFallidos = 0;
                usuario.UltimoAcceso = DateTime.Now;
                GuardarUsuarios();
                return true;
            }

            usuario.IntentosFallidos++;
            if (usuario.IntentosFallidos >= IntentosMaximos)
                usuario.Bloqueado = true;
            GuardarUsuarios();
            return false;
        }

        //Autenticación por Huella Digital
        public bool VerificarHuellaDigital(Usuario usuario, string huella)
        {
            if (usuario.HuellaDigital == huella)
            {
                usuario.IntentosFallidos = 0;
                usuario.UltimoAcceso = DateTime.Now;
                GuardarUsuarios();
                return true;
            }

            usuario.IntentosFallidos++;
            if (usuario.IntentosFallidos >= IntentosMaximos)
                usuario.Bloqueado = true;
            GuardarUsuarios();
            return false;
        }
        public bool ValidarAcceso(string tarjeta, string pin, string huella)
        {
            var usuarios = RepositorioArchivos.CargarUsuarios();
            var u = usuarios.FirstOrDefault(x => x.NumeroTarjeta == tarjeta);
            if (u == null) return false;

            if (u.Bloqueado) return false; // Ya está bloqueado

            bool credencialOk = u.Pin == pin || (!string.IsNullOrEmpty(huella) && u.HuellaDigital == huella);

            if (credencialOk)
            {
                u.IntentosFallidos = 0;
                u.UltimoAcceso = DateTime.Now;
                RepositorioArchivos.GuardarUsuarios(usuarios);
                return true;
            }

            u.IntentosFallidos++;
            if (u.IntentosFallidos >= 3)
            {
                u.Bloqueado = true;
            }
            RepositorioArchivos.GuardarUsuarios(usuarios);
            return false;
        }

        public int IntentosRestantes(Usuario usuario) => Math.Max(0, IntentosMaximos - usuario.IntentosFallidos);


        // ---------- Administración ----------

        public Dictionary<int, int> ObtenerDesgloseActual()
        {
            if (_config?.Denominaciones == null)
            {
                return new Dictionary<int, int>
                {
                    { 200, 0 }, { 100, 0 }, { 50, 0 }, { 20, 0 }, { 10, 0 }, { 5, 0 }, { 1, 0 }
                };
            }

            return new Dictionary<int, int>(_config.Denominaciones);
        }


        public ResultadoOperacion CrearUsuario(string nombre, string numeroTarjeta, string pin, decimal saldoInicial, decimal limiteDiario, string huellaDigital, string uid, int huellaSlot)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return ResultadoOperacion.Error("El nombre es obligatorio.");
            if (numeroTarjeta.Length != 16 || !numeroTarjeta.All(char.IsDigit))
                return ResultadoOperacion.Error("El número de tarjeta debe tener 16 dígitos.");
            if (pin.Length != 4 || !pin.All(char.IsDigit))
                return ResultadoOperacion.Error("El PIN debe tener 4 dígitos.");
            if (string.IsNullOrWhiteSpace(uid))
                return ResultadoOperacion.Error("Debe vincular una tarjeta antes de guardar.");
            if (_usuarios.Any(u => u.NumeroTarjeta == numeroTarjeta))
                return ResultadoOperacion.Error("Ya existe un usuario con ese número de tarjeta.");
            if (_usuarios.Any(u => u.UID == uid && !string.IsNullOrEmpty(u.UID)))
                return ResultadoOperacion.Error("Esa tarjeta ya está vinculada a otro usuario.");
            if (_usuarios.Any(u => u.HuellaSlot == huellaSlot))
                return ResultadoOperacion.Error("Ese slot de huella ya está asignado a otro usuario.");

            _usuarios.Add(new Usuario
            {
                Nombre = nombre,
                NumeroTarjeta = numeroTarjeta,
                Pin = pin,
                Saldo = saldoInicial,
                LimiteDiario = limiteDiario,
                Tipo = TipoUsuario.Usuario,
                UltimoAcceso = DateTime.MinValue,
                FechaRetiroHoy = DateTime.MinValue,
                HuellaDigital = huellaDigital ?? "",
                UID = uid,
                HuellaSlot = huellaSlot
            });

            GuardarUsuarios();
            return ResultadoOperacion.Ok("Usuario creado correctamente.");
        }


        public ResultadoOperacion InicializarCajero(Dictionary<int, int> denominaciones)
        {
            var total = denominaciones.Sum(kv => (decimal)kv.Key * kv.Value);
            if (total <= 0)
                return ResultadoOperacion.Error("Debe ingresar al menos un billete.");
            if (total > MontoMaximoInicializacion)
                return ResultadoOperacion.Error($"El monto inicial no puede superar Q.{MontoMaximoInicializacion:N2}");

            _config.Denominaciones = new Dictionary<int, int>(denominaciones);
            _config.Inicializado = true;
            _config.FechaInicializacion = DateTime.Now;
            _config.Estado = EstadoCajero.Activo;
            RepositorioArchivos.GuardarConfig(_config);
            return ResultadoOperacion.Ok($"Cajero inicializado con Q.{total:N2}.");
        }

        public ResultadoOperacion AgregarEfectivo(Dictionary<int, int> denominaciones)
        {
            if (!_config.Inicializado)
                return ResultadoOperacion.Error("El cajero debe ser inicializado primero.");

            var totalNuevo = denominaciones.Sum(kv => (decimal)kv.Key * kv.Value);
            if (totalNuevo <= 0)
                return ResultadoOperacion.Error("Debe ingresar al menos un billete.");
            if (totalNuevo > MontoMaximoAgregarEfectivo)
                return ResultadoOperacion.Error($"No se puede agregar más de Q.{MontoMaximoAgregarEfectivo:N2} de una vez.");

            foreach (var kv in denominaciones)
                _config.Denominaciones[kv.Key] = (_config.Denominaciones.ContainsKey(kv.Key) ? _config.Denominaciones[kv.Key] : 0) + kv.Value;

            RepositorioArchivos.GuardarConfig(_config);
            return ResultadoOperacion.Ok($"Se agregaron Q.{totalNuevo:N2} al cajero.");
        }

        public ResultadoOperacion ModificarNumeroTarjeta(Usuario usuario, string nuevoNumero)
        {
            if (nuevoNumero.Length != 16 || !nuevoNumero.All(char.IsDigit))
                return ResultadoOperacion.Error("El nuevo número de tarjeta debe tener 16 dígitos.");
            if (_usuarios.Any(u => u.NumeroTarjeta == nuevoNumero))
                return ResultadoOperacion.Error("El nuevo número de tarjeta ya está en uso.");

            usuario.HistorialTarjetas.Add(usuario.NumeroTarjeta);
            usuario.NumeroTarjeta = nuevoNumero;
            GuardarUsuarios();
            return ResultadoOperacion.Ok("Número de tarjeta actualizado.");
        }

        public ResultadoOperacion ModificarLimite(Usuario usuario, decimal nuevoLimite)
        {
            if (nuevoLimite <= 0)
                return ResultadoOperacion.Error("El límite debe ser mayor a cero.");

            usuario.LimiteDiario = nuevoLimite;
            GuardarUsuarios();
            return ResultadoOperacion.Ok("Límite diario actualizado.");
        }

        public ConsultaUsuarioDTO ConsultarUsuario(string numeroTarjeta)
        {
            var u = BuscarPorTarjeta(numeroTarjeta);
            if (u == null) return null;

            return new ConsultaUsuarioDTO
            {
                Nombre = u.Nombre,
                MontoMaximoDisponible = u.LimiteDiario,
                CantidadRetiradaHastaElMomento = u.FechaRetiroHoy.Date == DateTime.Today ? u.RetiradoHoy : 0,
                SaldoActual = u.Saldo,
                UltimoAcceso = u.UltimoAcceso,
                bloqueado = u.Bloqueado
            };
        }
           public bool CambiarEstadoBloqueo(string numeroTarjeta, bool bloquear)
        {
            var usuario = _usuarios.FirstOrDefault(u =>
                u.NumeroTarjeta == numeroTarjeta || u.HistorialTarjetas.Contains(numeroTarjeta));

            if (usuario == null) return false;

            usuario.Bloqueado = bloquear;
            if (!bloquear) usuario.IntentosFallidos = 0;

            GuardarUsuarios();
            return true;
        }
        


        public ControlUsuariosDTO ObtenerControlUsuarios()
        {
            var transaccionesHoy = RepositorioArchivos.CargarTransacciones().Where(t => t.Fecha.Date == DateTime.Today).ToList();
            var retiros = transaccionesHoy.Where(t => t.Tipo == TipoTransaccion.Retiro).ToList();
            var depositos = transaccionesHoy.Where(t => t.Tipo == TipoTransaccion.Deposito).ToList();

            var usuariosNoAdmin = _usuarios.Where(u => u.Tipo == TipoUsuario.Usuario).ToList();
            var ultimo = usuariosNoAdmin.OrderByDescending(u => u.UltimoAcceso).FirstOrDefault();

            return new ControlUsuariosDTO
            {
                TotalRetiradoTodos = retiros.Sum(t => t.Monto),
                PromedioDepositado = depositos.Count > 0 ? depositos.Average(t => t.Monto) : 0,
                UsuariosQueCambiaronPin = usuariosNoAdmin.Where(u => u.CambioPin).Select(u => u.Nombre).ToList(),
                UltimoUsuarioAcceso = ultimo?.Nombre,
                FechaUltimoAcceso = ultimo?.UltimoAcceso ?? DateTime.MinValue
            };
        }

        public void ApagarCajero()
        {
            _config.Estado = EstadoCajero.Inactivo;
            RepositorioArchivos.GuardarConfig(_config);
            Sesion.UsuarioActual = null;
        }

        public void GenerarReporteControlUsuarios(string rutaArchivo)
        {
            var dto = ObtenerControlUsuarios();
            var lineas = new List<string>
            {
                $"Total retirado (todos los usuarios, hoy): Q.{dto.TotalRetiradoTodos:N2}",
                $"Promedio depositado (hoy): Q.{dto.PromedioDepositado:N2}",
                $"Usuarios que cambiaron su PIN: {(dto.UsuariosQueCambiaronPin.Count > 0 ? string.Join(", ", dto.UsuariosQueCambiaronPin) : "Ninguno")}",
                $"Último usuario en acceder: {dto.UltimoUsuarioAcceso ?? "N/A"} ({dto.FechaUltimoAcceso})"
            };
            RepositorioArchivos.GenerarReporte(rutaArchivo, "Reporte de Control de Usuarios", lineas);
        }

        // ---------- Usuario ----------
        public ResultadoOperacion CambiarPin(Usuario usuario, string pinActual, string pinNuevo, string confirmacionPinNuevo)
        {
            if (usuario.Pin != pinActual)
                return ResultadoOperacion.Error("El PIN actual es incorrecto.");
            if (pinNuevo.Length != 4 || !pinNuevo.All(char.IsDigit))
                return ResultadoOperacion.Error("El nuevo PIN debe tener 4 dígitos.");
            if (pinNuevo != confirmacionPinNuevo)
                return ResultadoOperacion.Error("La confirmación no coincide con el nuevo PIN.");

            usuario.Pin = pinNuevo;
            usuario.CambioPin = true;
            GuardarUsuarios();
            return ResultadoOperacion.Ok("PIN actualizado correctamente.");
        }

        public ResultadoOperacion Retirar(Usuario usuario, decimal monto, Dictionary<int, int> desglose)
        {
            var totalDesglose = desglose.Sum(kv => (decimal)kv.Key * kv.Value);
            if (totalDesglose != monto)
                return ResultadoOperacion.Error("El desglose de billetes no coincide con el monto solicitado.");

            if (monto > usuario.Saldo)
                return ResultadoOperacion.Error("Saldo insuficiente.");

            if (usuario.FechaRetiroHoy.Date != DateTime.Today)
            {
                usuario.RetiradoHoy = 0;
                usuario.FechaRetiroHoy = DateTime.Today;
            }

            if (usuario.RetiradoHoy + monto > usuario.LimiteDiario)
                return ResultadoOperacion.Error("Ese retiro supera su límite diario.");

            foreach (var kv in desglose)
            {

                // Por esta:
                int disponible = _config.Denominaciones.ContainsKey(kv.Key) ? _config.Denominaciones[kv.Key] : 0;
                if (disponible < kv.Value)
                    return ResultadoOperacion.Error($"El cajero no tiene suficientes billetes de Q.{kv.Key}.");
            }

            foreach (var kv in desglose)
                _config.Denominaciones[kv.Key] -= kv.Value;

            usuario.Saldo -= monto;
            usuario.RetiradoHoy += monto;

            RegistrarTransaccion(usuario, TipoTransaccion.Retiro, monto, desglose);
            RepositorioArchivos.GuardarConfig(_config);
            GuardarUsuarios();
            return ResultadoOperacion.Ok($"Retiro de Q.{monto:N2} realizado. Saldo actual: Q.{usuario.Saldo:N2}");
        }

        public ResultadoOperacion Depositar(Usuario usuario, Dictionary<int, int> desglose)
        {
            var monto = desglose.Sum(kv => (decimal)kv.Key * kv.Value);
            if (monto <= 0)
                return ResultadoOperacion.Error("Debe ingresar al menos un billete para depositar.");

            foreach (var kv in desglose)
                _config.Denominaciones[kv.Key] = (_config.Denominaciones.ContainsKey(kv.Key) ? _config.Denominaciones[kv.Key] : 0) + kv.Value;

            usuario.Saldo += monto;

            RegistrarTransaccion(usuario, TipoTransaccion.Deposito, monto, desglose);
            RepositorioArchivos.GuardarConfig(_config);
            GuardarUsuarios();
            return ResultadoOperacion.Ok($"Depósito de Q.{monto:N2} realizado. Saldo actual: Q.{usuario.Saldo:N2}");
        }

        public List<Transaccion> ObtenerUltimasTransacciones(string numeroTarjeta, int cantidad = 5) =>
            RepositorioArchivos.CargarTransacciones()
                .Where(t => t.NumeroTarjeta == numeroTarjeta)
                .OrderByDescending(t => t.Fecha).ThenByDescending(t => t.Hora)
                .Take(cantidad)
                .ToList();

        public (decimal limiteMaximo, decimal disponibleHoy) VerSaldo(Usuario usuario) =>
            (usuario.LimiteDiario, usuario.DisponibleHoy());

        // ---------- Privados ----------
        private void RegistrarTransaccion(Usuario usuario, TipoTransaccion tipo, decimal monto, Dictionary<int, int> desglose)
        {
            RepositorioArchivos.AgregarTransaccion(new Transaccion
            {
                NumeroTarjeta = usuario.NumeroTarjeta,
                Fecha = DateTime.Today,
                Hora = DateTime.Now.TimeOfDay,
                Tipo = tipo,
                Monto = monto,
                Desglose = desglose
            });
        }

        private void GuardarUsuarios() => RepositorioArchivos.GuardarUsuarios(_usuarios);
    }
}
