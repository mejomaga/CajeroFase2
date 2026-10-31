using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace CajeroAutomatico.Comunicacion
{
    public class ServidorClient : IDisposable
    {
        private readonly string _host;
        private readonly int _puerto;
        private readonly int _timeoutMs;
        private readonly object _lockRed = new object();

        private TcpClient _cliente;
        private NetworkStream _stream;
        private StreamReader _lector;
        private StreamWriter _escritor;
        private Timer _timerHeartbeat;

        public string Token { get; private set; }
        public string TokenOperacion { get; private set; }
        public string CodigoCajero { get; set; } = "ATM-02";

        public ServidorClient()
        {
            _host = ConfigurationManager.AppSettings["ServidorIP"] ?? "192.168.1.43";
            _puerto = int.TryParse(ConfigurationManager.AppSettings["ServidorPuerto"], out var p) ? p : 5000;
            _timeoutMs = int.TryParse(ConfigurationManager.AppSettings["ServidorTimeoutMs"], out var t) ? t : 10000;

            // Timer de Heartbeat cada 10 segundos para mantener el cajero en línea en el servidor
            _timerHeartbeat = new Timer(EjecutarHeartbeat, null, 10000, 10000);
        }

        private void EjecutarHeartbeat(object state)
        {
            try
            {
                if (EstaConectado())
                {
                    EnviarHeartbeat();
                }
            }
            catch { }
        }

        public bool EstaConectado()
        {
            lock (_lockRed)
            {
                return _cliente != null && _cliente.Connected;
            }
        }

        public void FijarToken(string token) => Token = token;
        public void LimpiarToken() => Token = null;

        // --- 1. LOGIN (Tarjeta + PIN) ---
        public RespuestaRed Login(string numeroTarjeta, string pin)
        {
            var datos = new { tarjeta = numeroTarjeta, pin = pin };
            var respuesta = Enviar(Operaciones.Login, datos);

            if (respuesta.Exito)
            {
                if (respuesta.Datos.ContainsKey("token"))
                    Token = respuesta.Datos["token"]?.ToString();
                else if (respuesta.Datos.ContainsKey("Token"))
                    Token = respuesta.Datos["Token"]?.ToString();
            }

            return respuesta;
        }

        // --- 2. RETIRAR ---
        public RespuestaRed Retirar(decimal monto, Dictionary<int, int> desglose)
        {
            var datos = new
            {
                token = Token,
                monto = monto,
                desglose = desglose
            };
            return Enviar(Operaciones.Retirar, datos);
        }

        // --- 3. DEPOSITAR ---
        public RespuestaRed Depositar(Dictionary<int, int> desglose)
        {
            var datos = new { token = Token, desglose = desglose };
            return Enviar(Operaciones.Depositar, datos);
        }

        // --- 4. CONSULTAR SALDO ---
        public RespuestaRed ConsultarSaldo() => Enviar(Operaciones.ConsultarSaldo, new { token = Token });

        // --- 5. CAMBIAR PIN ---
        public RespuestaRed CambiarPin(string pinActual, string pinNuevo)
        {
            var datos = new { token = Token, pinActual = pinActual, pinNuevo = pinNuevo };
            return Enviar(Operaciones.CambiarPin, datos);
        }

        // --- 6. ULTIMAS TRANSACCIONES ---
        public RespuestaRed UltimasTransacciones(int cantidad = 5)
        {
            var datos = new { token = Token, cantidad = cantidad };
            return Enviar(Operaciones.UltimasTransacciones, datos);
        }

        // --- 7. LOGOUT ---
        public RespuestaRed CerrarSesion()
        {
            if (string.IsNullOrEmpty(Token)) return new RespuestaRed { Exito = true };
            var respuesta = Enviar(Operaciones.CerrarSesion, new { token = Token });
            LimpiarToken();
            return respuesta;
        }

        // --- 8. ESTADO CAJERO ---
        public RespuestaRed ObtenerEstadoCajero() => Enviar(Operaciones.EstadoCajero, new { });

        // --- 9. HEARTBEAT (Para mantener estado 'En línea' en el servidor) ---
        public RespuestaRed EnviarHeartbeat() => Enviar(Operaciones.Heartbeat, new { cajero = CodigoCajero });

        // --- 10. VERIFICAR O ASEGURAR CONEXIÓN ---
        public bool VerificarConexion()
        {
            return AsegurarConexion();
        }

        private bool AsegurarConexion()
        {
            lock (_lockRed)
            {
                if (_cliente != null && _cliente.Connected)
                    return true;

                CerrarConexionInterna();

                try
                {
                    _cliente = new TcpClient();
                    var resultado = _cliente.BeginConnect(_host, _puerto, null, null);
                    if (!resultado.AsyncWaitHandle.WaitOne(_timeoutMs))
                    {
                        CerrarConexionInterna();
                        return false;
                    }

                    _cliente.EndConnect(resultado);
                    _cliente.ReceiveTimeout = _timeoutMs;
                    _cliente.SendTimeout = _timeoutMs;

                    _stream = _cliente.GetStream();
                    _lector = new StreamReader(_stream, Encoding.UTF8);
                    _escritor = new StreamWriter(_stream, new UTF8Encoding(false)) { AutoFlush = true, NewLine = "\n" };

                    return true;
                }
                catch
                {
                    CerrarConexionInterna();
                    return false;
                }
            }
        }

        private void CerrarConexionInterna()
        {
            try { _lector?.Dispose(); } catch { }
            try { _escritor?.Dispose(); } catch { }
            try { _stream?.Dispose(); } catch { }
            try { _cliente?.Close(); } catch { }
            _lector = null;
            _escritor = null;
            _stream = null;
            _cliente = null;
        }

        // --- 11. GENERAR TOKEN (para autorizar una transacción) ---
        public RespuestaRed GenerarTokenOperacion()
        {
            var respuesta = Enviar(Operaciones.GenerarToken, new { });
            return respuesta;
        }

        // --- 12. VALIDAR TOKEN DE OPERACIÓN ---
        public RespuestaRed ValidarTokenOperacion(string tokenIngresado)
        {
            var datos = new { tokenOperacion = tokenIngresado };
            return Enviar(Operaciones.ValidarToken, datos);
        }


        // =========================================================
        // EL CORAZÓN: Comunicación JSON thread-safe sobre socket persistente
        // =========================================================
        private RespuestaRed Enviar(string tipoMensaje, object datos)
        {
            lock (_lockRed)
            {
                try
                {
                    if (!AsegurarConexion())
                        return RespuestaRed.ErrorConexion("No se pudo conectar con el servidor central.");

                    var solicitud = new Dictionary<string, object>
                    {
                        { "tipo", tipoMensaje },
                        { "idSolicitud", Guid.NewGuid().ToString().Substring(0, 4) },
                        { "cajero", CodigoCajero }
                    };

                    if (datos != null)
                    {
                        var props = datos.GetType().GetProperties();
                        foreach (var prop in props)
                        {
                            solicitud[prop.Name] = prop.GetValue(datos);
                        }
                    }

                    string json = JsonSerializer.Serialize(solicitud);
                    _escritor.WriteLine(json);

                    string linea = _lector.ReadLine();
                    if (string.IsNullOrEmpty(linea))
                    {
                        CerrarConexionInterna();
                        return RespuestaRed.ErrorConexion("El servidor cerró la conexión.");
                    }

                    var respuestaRaw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(linea);
                    var res = new RespuestaRed();

                    // Soportar 'Exito' o 'exito'
                    if (respuestaRaw.ContainsKey("Exito"))
                        res.Exito = respuestaRaw["Exito"].GetBoolean();
                    else if (respuestaRaw.ContainsKey("exito"))
                        res.Exito = respuestaRaw["exito"].GetBoolean();

                    // Soportar 'Mensaje' o 'mensaje'
                    if (respuestaRaw.ContainsKey("Mensaje"))
                        res.Mensaje = respuestaRaw["Mensaje"].GetString();
                    else if (respuestaRaw.ContainsKey("mensaje"))
                        res.Mensaje = respuestaRaw["mensaje"].GetString();

                    // Soportar 'Datos' o 'datos'
                    string claveDatos = respuestaRaw.ContainsKey("Datos") ? "Datos"
                                      : (respuestaRaw.ContainsKey("datos") ? "datos" : null);

                    if (claveDatos != null && respuestaRaw[claveDatos].ValueKind == JsonValueKind.Object)
                    {
                        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(respuestaRaw[claveDatos].GetRawText());
                        if (dict != null)
                        {
                            res.Datos = new Dictionary<string, object>(dict, StringComparer.OrdinalIgnoreCase);
                        }
                    }

                    return res;
                }
                catch (Exception ex)
                {
                    CerrarConexionInterna();
                    return RespuestaRed.ErrorConexion($"Error de comunicación: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            try { _timerHeartbeat?.Dispose(); } catch { }
            CerrarConexionInterna();
        }
    }
}
