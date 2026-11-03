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

        public string CodigoCajero { get; set; } = "ATM-04";

        public ServidorClient()
        {
            _host = ConfigurationManager.AppSettings["ServidorIP"] ?? "10.125.197.14";
            _puerto = int.TryParse(ConfigurationManager.AppSettings["ServidorPuerto"], out var p) ? p : 5000;
            _timeoutMs = int.TryParse(ConfigurationManager.AppSettings["ServidorTimeoutMs"], out var t) ? t : 10000;

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

        public RespuestaRed Depositar(Dictionary<int, int> desglose)
        {
            var datos = new { token = Token, desglose = desglose };
            return Enviar(Operaciones.Depositar, datos);
        }

        public RespuestaRed ConsultarSaldo() => Enviar(Operaciones.ConsultarSaldo, new { token = Token });

        public RespuestaRed CambiarPin(string pinActual, string pinNuevo)
        {
            var datos = new { token = Token, pinActual = pinActual, pinNuevo = pinNuevo };
            return Enviar(Operaciones.CambiarPin, datos);
        }

        public RespuestaRed UltimasTransacciones(int cantidad = 5)
        {
            var datos = new { token = Token, cantidad = cantidad };
            return Enviar(Operaciones.UltimasTransacciones, datos);
        }

        public RespuestaRed CerrarSesion()
        {
            if (string.IsNullOrEmpty(Token)) return new RespuestaRed { Exito = true };
            var respuesta = Enviar(Operaciones.CerrarSesion, new { token = Token });
            LimpiarToken();
            return respuesta;
        }

        public RespuestaRed ObtenerEstadoCajero() => Enviar(Operaciones.EstadoCajero, new { });

        public RespuestaRed EnviarHeartbeat() => Enviar(Operaciones.Heartbeat, new { cajero = CodigoCajero });

        public bool VerificarConexion()
        {
            return AsegurarConexion();
        }

        public RespuestaRed ValidarTarjeta(string numeroTarjeta)
        {
            return Enviar("VALIDAR_TARJETA", new { tarjeta = numeroTarjeta });
        }

        public RespuestaRed GenerarTokenOperacion()
        {
            return Enviar(Operaciones.GenerarToken, new { token = Token });
        }

        public RespuestaRed ValidarTokenOperacion(string tokenIngresado)
        {
            return Enviar(Operaciones.ValidarToken, new { token = Token, tokenOperacion = tokenIngresado });
        }

        // --- OPERACIONES INTERBANCARIAS (PROTOCOLO NETGEAR 2.2) ---
        public List<BancoExternoDto> ObtenerBancosAutorizados()
        {
            var lista = new List<BancoExternoDto>();
            var res = Enviar(Operaciones.BancosAutorizados, new { token = Token });
            if (res.Exito && res.Datos != null && res.Datos.ContainsKey("Bancos"))
            {
                try
                {
                    string rawJson = res.Datos["Bancos"].ToString();
                    var doc = JsonDocument.Parse(rawJson);
                    foreach (var elem in doc.RootElement.EnumerateArray())
                    {
                        lista.Add(new BancoExternoDto
                        {
                            CodigoBanco = elem.TryGetProperty("codigoBanco", out var c) ? c.GetString() : "",
                            NombreBanco = elem.TryGetProperty("nombreBanco", out var n) ? n.GetString() : "",
                            IP = elem.TryGetProperty("ip", out var ip) ? ip.GetString() : "",
                            PuertoTcp = elem.TryGetProperty("puertoTcp", out var p) ? p.GetInt32() : 5000,
                            CupoDisponible = elem.TryGetProperty("cupoDisponible", out var cupo) ? cupo.GetDecimal() : 0m
                        });
                    }
                }
                catch { }
            }
            return lista;
        }

        public RespuestaRed VerificarCuentaInterbancaria(string bancoDestino, string numeroCuenta)
        {
            return Enviar(Operaciones.VerificarCuentaIb, new
            {
                token = Token,
                bancoDestino = bancoDestino,
                numeroCuenta = numeroCuenta
            });
        }

        public RespuestaRed TransferirInterbancario(
            string bancoDestino,
            string cuentaDestino,
            decimal monto,
            string concepto,
            string tokenTransferencia = "")
        {
            return Enviar(Operaciones.TransferirIb, new
            {
                token = Token,
                bancoDestino = bancoDestino,
                cuentaDestino = cuentaDestino,
                monto = monto,
                concepto = concepto,
                tokenTransferencia = tokenTransferencia
            });
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

                    if (respuestaRaw.ContainsKey("Exito"))
                        res.Exito = respuestaRaw["Exito"].GetBoolean();
                    else if (respuestaRaw.ContainsKey("exito"))
                        res.Exito = respuestaRaw["exito"].GetBoolean();

                    if (respuestaRaw.ContainsKey("Mensaje"))
                        res.Mensaje = respuestaRaw["Mensaje"].GetString();
                    else if (respuestaRaw.ContainsKey("mensaje"))
                        res.Mensaje = respuestaRaw["mensaje"].GetString();

                    string claveDatos = respuestaRaw.ContainsKey("Datos") ? "Datos"
                                      : (respuestaRaw.ContainsKey("datos") ? "datos" : null);

                    if (claveDatos != null && respuestaRaw[claveDatos].ValueKind == JsonValueKind.Object)
                    {
                        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(respuestaRaw[claveDatos].GetRawText());
                        if (dict != null)
                        {
                            var dictProcesado = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                            foreach (var kvp in dict)
                            {
                                if (kvp.Value is JsonElement elementoJson)
                                {
                                    switch (elementoJson.ValueKind)
                                    {
                                        case JsonValueKind.Number:
                                            dictProcesado[kvp.Key] = elementoJson.GetDecimal();
                                            break;
                                        case JsonValueKind.String:
                                            dictProcesado[kvp.Key] = elementoJson.GetString();
                                            break;
                                        case JsonValueKind.True:
                                        case JsonValueKind.False:
                                            dictProcesado[kvp.Key] = elementoJson.GetBoolean();
                                            break;
                                        default:
                                            dictProcesado[kvp.Key] = elementoJson.GetRawText();
                                            break;
                                    }
                                }
                                else
                                {
                                    dictProcesado[kvp.Key] = kvp.Value;
                                }
                            }

                            res.Datos = dictProcesado;
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
