using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using ProyectoServidor.Datos;
using ProyectoServidor.Negocio;

namespace ProyectoServidor.Comunicaciones
{
    public class ServidorCentral
    {
        private TcpListener _listener;
        private CancellationTokenSource _cancellationTokenSource;
        private ConcurrentDictionary<string, TcpClient> _cajerosConectados;

        // Variables necesarias para el control de sesiones y heartbeats
        private ConcurrentDictionary<string, (int idUsuario, int idTarjeta)> _sesionesActivas;
        private readonly ConcurrentDictionary<string, DateTime> _tokensOperacionValidados;
        private ConcurrentDictionary<string, DateTime> _ultimoHeartbeat;
        private Timer _timerHeartbeat;

        private readonly ConexionServidor _bd;
        private readonly GestorToken _gestorToken;

        public event Action<string> OnMensajeConsola;

        public ServidorCentral()
        {
            _tokensOperacionValidados = new ConcurrentDictionary<string, DateTime>();
            _cajerosConectados = new ConcurrentDictionary<string, TcpClient>();
            _sesionesActivas = new ConcurrentDictionary<string, (int, int)>();
            _ultimoHeartbeat = new ConcurrentDictionary<string, DateTime>();
            _bd = new ConexionServidor();
            _gestorToken = new GestorToken();
        }

        public void Iniciar(int puerto = 5000)
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, puerto);
                _listener.Start();
                _cancellationTokenSource = new CancellationTokenSource();

                // Iniciar el vigilante de Heartbeats cada 10 segundos
                _timerHeartbeat = new Timer(RevisarCajerosInactivos, null, 10000, 10000);


                OnMensajeConsola?.Invoke($"[SERVIDOR] Iniciado correctamente en el puerto {puerto}.");
                OnMensajeConsola?.Invoke("[SERVIDOR] Esperando conexiones de cajeros...");

                Task.Run(() => EscucharClientesAsync(_cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                OnMensajeConsola?.Invoke($"[ERROR] No se pudo iniciar el servidor: {ex.Message}");
            }
        }

        public void Detener()
        {
            _cancellationTokenSource?.Cancel();
            _listener?.Stop();
            _timerHeartbeat?.Dispose();
            OnMensajeConsola?.Invoke("[SERVIDOR] Apagado.");
        }

        private async Task EscucharClientesAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient cliente = await _listener.AcceptTcpClientAsync();
                    OnMensajeConsola?.Invoke($"[RED] Nueva conexión detectada desde: {cliente.Client.RemoteEndPoint}");
                    _ = Task.Run(() => AtenderCajeroAsync(cliente, token));
                }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex) { OnMensajeConsola?.Invoke($"[ERROR RED] {ex.Message}"); }
            }
        }

        private async Task AtenderCajeroAsync(TcpClient cliente, CancellationToken token)
        {
            string idCajero = "Desconocido";

            try
            {
                using NetworkStream stream = cliente.GetStream();
                using StreamReader reader = new StreamReader(stream);
                using StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

                while (cliente.Connected && !token.IsCancellationRequested)
                {
                    string jsonCompleto = await reader.ReadLineAsync();

                    // Si el cajero cierra la conexión abruptamente, salimos
                    if (jsonCompleto == null) break;

                    // Si viene vacío, ignoramos y seguimos escuchando
                    if (string.IsNullOrWhiteSpace(jsonCompleto)) continue;

                    OnMensajeConsola?.Invoke($"[MENSAJE RECIBIDO] {jsonCompleto}");

                    try
                    {
                        string respuestaJson = ProcesarMensaje(jsonCompleto, ref idCajero, cliente);
                        await writer.WriteLineAsync(respuestaJson);
                    }
                    catch (Exception ex)
                    {
                        // Si un mensaje falla, avisamos pero NO cerramos la conexión
                        OnMensajeConsola?.Invoke($"[ERROR MENSAJE] {ex.Message}");
                        await writer.WriteLineAsync("{\"Exito\":false,\"Mensaje\":\"Error interno del servidor\"}");
                    }
                }
            }
            catch (Exception ex)
            {
                OnMensajeConsola?.Invoke($"[DESCONEXIÓN] Error con cajero {idCajero}: {ex.Message}");
            }
            finally
            {
                cliente.Close();
                OnMensajeConsola?.Invoke($"[RED] El cajero {idCajero} se ha desconectado.");
                if (idCajero != "Desconocido")
                {
                    //_cajerosConectados.TryRemove(idCajero, out _);
                    //_sesionesActivas.TryRemove(idCajero, out _);
                }
            }
        }

        private string ProcesarMensaje(string jsonCompleto, ref string idCajero, TcpClient cliente = null)
        {
            var jsonNode = JsonNode.Parse(jsonCompleto);
            string tipo = jsonNode["Operacion"]?.ToString().ToUpper() ?? jsonNode["tipo"]?.ToString()?.ToUpper();
            string idSol = jsonNode["idSolicitud"]?.ToString() ?? "0";

            if (jsonNode["cajero"] != null && idCajero == "Desconocido")
                idCajero = jsonNode["cajero"].ToString();

            // --- 1. REGISTRAR CAJERO ---
            if (!string.IsNullOrEmpty(idCajero) && idCajero != "Desconocido")
            {
                if (cliente != null) _cajerosConectados[idCajero] = cliente;
                _ultimoHeartbeat[idCajero] = DateTime.Now;
            }

            // --- 1. REGISTRAR CAJERO ---
            if (tipo == "REGISTRAR_CAJERO")
            {
                if (cliente != null) _cajerosConectados[idCajero] = cliente;
                _bd.RegistrarCajero(idCajero, "Terminal", "127.0.0.1", out string cod, out _);
                return $"{{\"Exito\":true,\"Mensaje\":\"Registrado\",\"codigo\":\"{cod}\"}}";
            }

            // --- 2. HEARTBEAT / ESTADO ---
            if (tipo == "HEARTBEAT" || tipo == "ESTADO_CAJERO")
            {
                _ultimoHeartbeat[idCajero] = DateTime.Now;
                _bd.ActualizarHeartbeat(idCajero);
                return "{\"Exito\":true,\"Mensaje\":\"Vivo\",\"Datos\":{\"Estado\":\"Activo\"}}";
            }

            // --- 3. VALIDAR PIN (LOGIN) ---
            if (tipo == "LOGIN" || tipo == "VALIDAR_PIN")
            {
                var datos = jsonNode["Datos"];
                string tarjeta = datos?["NumeroTarjeta"]?.ToString() ?? jsonNode["tarjeta"]?.ToString();
                string pin = datos?["Pin"]?.ToString() ?? jsonNode["pin"]?.ToString();

                bool exito = _bd.ValidarCredenciales(tarjeta, pin, out int idUsuario, out int idTarjeta, out string cod, out string err);

                if (exito)
                {
                    _bd.GenerarToken(idUsuario, idCajero, out string token, out _);
                    _sesionesActivas[idCajero] = (idUsuario, idTarjeta);
                    return $"{{\"Exito\":true,\"Mensaje\":\"Ingreso correcto\",\"Datos\":{{\"Token\":\"{token}\"}}}}";
                }
                return $"{{\"Exito\":false,\"Mensaje\":\"{err}\"}}";
            }
            // --- 3.5 GENERAR TOKEN DE OPERACIÓN ---
            if (tipo == "GENERAR_TOKEN")
            {
                if (!_sesionesActivas.TryGetValue(idCajero, out var ids))
                    return "{\"Exito\":false,\"Mensaje\":\"Sesión no iniciada\"}";

                bool exito = _bd.GenerarTokenOperacion(ids.idUsuario, idCajero, out string token, out string err);

                if (exito)
                    return $"{{\"Exito\":true,\"Mensaje\":\"Token generado\",\"Datos\":{{\"TokenOperacion\":\"{token}\"}}}}";

                return $"{{\"Exito\":false,\"Mensaje\":\"{err}\"}}";
            }

            // --- 3.6 VALIDAR TOKEN DE OPERACIÓN ---
            if (tipo == "VALIDAR_TOKEN")
            {
                if (!_sesionesActivas.TryGetValue(idCajero, out var ids))
                    return "{\"Exito\":false,\"Mensaje\":\"Sesión no iniciada\"}";

                // El cliente puede enviarlo en Datos o en la raíz
                string tokenIng = jsonNode["Datos"]?["TokenOperacion"]?.ToString()
                               ?? jsonNode["tokenOperacion"]?.ToString()
                               ?? jsonNode["Datos"]?["tokenOperacion"]?.ToString();

                if (string.IsNullOrWhiteSpace(tokenIng))
                    return "{\"Exito\":false,\"Mensaje\":\"Token no recibido\"}";

                bool exito = _bd.ValidarTokenOperacion(ids.idUsuario, idCajero, tokenIng, out string err);

                if (exito)
                {
                    // 🔑 Guarda por 3 minutos (más que suficiente para ejecutar la transacción)
                    _tokensOperacionValidados[idCajero] = DateTime.Now;
                }

                return $"{{\"Exito\":{exito.ToString().ToLower()},\"Mensaje\":\"{(exito ? "Token válido" : err)}\"}}";
            }

            // --- 4. RETIRAR (requiere token ya validado) ---
            if (tipo == "RETIRO" || tipo == "RETIRAR")
            {
                if (!_sesionesActivas.TryGetValue(idCajero, out var ids))
                    return "{\"Exito\":false,\"Mensaje\":\"Sesión no iniciada\"}";

                // 🔒 El cliente debe haber validado el token ANTES de llegar aquí
                if (!_tokensOperacionValidados.TryRemove(idCajero, out _))
                    return "{\"Exito\":false,\"Mensaje\":\"Debe validar el token antes de ejecutar la transacción\"}";

                var datos = jsonNode["Datos"];
                decimal monto = datos?["Monto"] != null
                    ? Convert.ToDecimal(datos["Monto"].ToString())
                    : Convert.ToDecimal(jsonNode["monto"]?.ToString());
                var desgloseInt = ProcesarDesglose(datos?["Desglose"] ?? jsonNode["desglose"]);

                bool exito = _bd.ProcesarRetiro(ids.idUsuario, ids.idTarjeta, idCajero,
                    monto, desgloseInt, out int idTx, out string cod, out string err);

                return $"{{\"Exito\":{exito.ToString().ToLower()},\"Mensaje\":\"{(exito ? "Retiro aprobado" : err)}\"}}";
            }

            // --- 5. DEPOSITAR (requiere token ya validado) ---
            if (tipo == "DEPOSITO" || tipo == "DEPOSITAR")
            {
                if (!_sesionesActivas.TryGetValue(idCajero, out var ids))
                    return "{\"Exito\":false,\"Mensaje\":\"Sesión no iniciada\"}";

                if (!_tokensOperacionValidados.TryRemove(idCajero, out _))
                    return "{\"Exito\":false,\"Mensaje\":\"Debe validar el token antes de ejecutar la transacción\"}";

                var datos = jsonNode["Datos"];
                var desgloseInt = ProcesarDesglose(datos?["Desglose"] ?? jsonNode["desglose"]);
                decimal monto = desgloseInt.Sum(x => x.Key * x.Value);

                bool exito = _bd.ProcesarDeposito(ids.idUsuario, ids.idTarjeta, idCajero,
                    monto, desgloseInt, out int idTx, out string cod, out string err);

                return $"{{\"Exito\":{exito.ToString().ToLower()},\"Mensaje\":\"{(exito ? "Depósito aprobado" : err)}\"}}";
            }

            // --- 6. CONSULTAR SALDO ---
            if (tipo == "CONSULTA_SALDO" || tipo == "CONSULTAR_SALDO")
            {
                if (!_sesionesActivas.TryGetValue(idCajero, out var ids))
                    return "{\"Exito\":false,\"Mensaje\":\"Sesión no iniciada\"}";

                bool exito = _bd.ConsultarSaldo(ids.idUsuario, out decimal saldo, out decimal limite,
                    out decimal disponible, out string err);

                if (!exito)
                    return "{\"Exito\":false,\"Mensaje\":\"Error al consultar\"}";

                // Lee el límite máximo por operación desde Parametro
                decimal limiteSistema = _bd.ObtenerParametroDecimal("MAX_RETIRO_POR_OPERACION",
                                            _bd.ObtenerParametroDecimal("MAX_CAPACIDAD_CAJERO", 30000m));

                return $"{{\"Exito\":true,\"Datos\":{{" +
                       $"\"SaldoActual\":{saldo.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                       $"\"LimiteMaximo\":{limite.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                       $"\"DisponibleHoy\":{disponible.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                       $"\"LimiteSistema\":{limiteSistema.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                       $"}}}}";
            }

            // --- 7. CAMBIAR PIN ---
            if (tipo == "CAMBIO_PIN" || tipo == "CAMBIAR_PIN")
            {
                var datos = jsonNode["Datos"];
                string nuevoPin = datos?["PinNuevo"]?.ToString() ?? jsonNode["pinNuevo"]?.ToString();

                if (!_sesionesActivas.TryGetValue(idCajero, out var ids))
                    return "{\"Exito\":false,\"Mensaje\":\"Sesión no iniciada\"}";

                bool exito = _bd.CambiarPin(ids.idUsuario, idCajero, "Usuario", nuevoPin, out string cod, out string err);

                return $"{{\"Exito\":{exito.ToString().ToLower()},\"Mensaje\":\"{(exito ? "PIN actualizado" : err)}\"}}";
            }

            // --- 8. ULTIMAS TRANSACCIONES ---
            if (tipo == "ULTIMAS_TRANSACCIONES")
            {
                if (!_sesionesActivas.TryGetValue(idCajero, out var ids))
                    return "{\"Exito\":false,\"Mensaje\":\"Sesión no iniciada\"}";

                var lista = _bd.ObtenerUltimasTransacciones(ids.idUsuario);
                return $"{{\"Exito\":true,\"Datos\":{{\"Transacciones\":{JsonSerializer.Serialize(lista)}}}}}";
            }

            // --- 9. CERRAR SESION ---
            if (tipo == "LOGOUT" || tipo == "CERRAR_SESION")
            {
                _sesionesActivas.TryRemove(idCajero, out _);
                return "{\"Exito\":true,\"Mensaje\":\"Sesión cerrada\"}";
            }

            return "{\"Exito\":false,\"Mensaje\":\"Operación no soportada\"}";
        }

        private Dictionary<int, int> ProcesarDesglose(JsonNode nodoDesglose)
        {
            var dict = new Dictionary<int, int>();
            if (nodoDesglose != null)
            {
                foreach (var kv in nodoDesglose.AsObject())
                    dict[int.Parse(kv.Key)] = int.Parse(kv.Value.ToString());
            }
            return dict;
        }

        public async Task<bool> EnviarComandoACajeroAsync(string codigoCajero, string comando)
        {
            // Buscamos la conexión activa del cajero
            if (_cajerosConectados.TryGetValue(codigoCajero, out TcpClient cliente) && cliente != null)
            {
                try
                {
                    using NetworkStream stream = cliente.GetStream();
                    using StreamWriter writer = new StreamWriter(stream, new System.Text.UTF8Encoding(false)) { AutoFlush = true, NewLine = "\n" };

                    // Armamos el JSON con el comando
                    string json = $"{{\"tipo\":\"COMANDO\",\"cajero\":\"{codigoCajero}\",\"comando\":\"{comando}\"}}";
                    await writer.WriteLineAsync(json);

                    OnMensajeConsola?.Invoke($"[COMANDO ENVIADO] A {codigoCajero}: {comando}");
                    return true;
                }
                catch (Exception ex)
                {
                    OnMensajeConsola?.Invoke($"[ERROR COMANDO] {ex.Message}");
                }
            }
            return false;
        }
        private void RevisarCajerosInactivos(object state)
        {
            DateTime ahora = DateTime.Now;
            foreach (var kvp in _ultimoHeartbeat)
            {
                if ((ahora - kvp.Value).TotalSeconds > 30)
                {
                    OnMensajeConsola?.Invoke($"[ALERTA] El cajero {kvp.Key} se ha desconectado (sin heartbeat por 30s).");
                    _ultimoHeartbeat.TryRemove(kvp.Key, out _);
                    _cajerosConectados.TryRemove(kvp.Key, out _);
                    // En LOGOUT:
                    _tokensOperacionValidados.TryRemove(kvp.Key, out _);

                    _bd.MarcarCajeroDesconectado(kvp.Key);
                }
            }
        }
    }
}