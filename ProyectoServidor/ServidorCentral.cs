using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        private UdpClient _udpListener;
        private CancellationTokenSource _cancellationTokenSource;
        private ConcurrentDictionary<string, TcpClient> _cajerosConectados;

        private const int PUERTO_UDP_DESCUBRIMIENTO = 5005;
        private int _puertoTcpActual = 5000;

        // Variables necesarias para el control de sesiones y heartbeats
        private ConcurrentDictionary<string, (int idUsuario, int idTarjeta)> _sesionesActivas;
        private readonly ConcurrentDictionary<string, DateTime> _tokensOperacionValidados;
        private ConcurrentDictionary<string, DateTime> _ultimoHeartbeat;
        private Timer _timerHeartbeat;

        private readonly ConexionServidor _bd;
        private readonly GestorToken _gestorToken;

        public event Action<string> OnMensajeConsola;
        public ConexionServidor BaseDatos => _bd;

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
                DetenerSilencioso();

                // Asegurar tablas y SP interbancarios en BD_CajeroRed_Actualizada
                _bd.AsegurarEsquemaInterbancario();

                _puertoTcpActual = puerto;
                _listener = new TcpListener(IPAddress.Any, puerto);
                _listener.Start();
                _cancellationTokenSource = new CancellationTokenSource();

                // Iniciar listener UDP obligatorio en 0.0.0.0:5005 para descubrimiento NETGEAR
                try
                {
                    _udpListener = new UdpClient(new IPEndPoint(IPAddress.Any, PUERTO_UDP_DESCUBRIMIENTO));
                    Task.Run(() => EscucharDescubrimientoUdpAsync(_cancellationTokenSource.Token));
                    OnMensajeConsola?.Invoke($"[UDP 5005] Listener de descubrimiento activo en 0.0.0.0:{PUERTO_UDP_DESCUBRIMIENTO}.");
                }
                catch (Exception exUdp)
                {
                    OnMensajeConsola?.Invoke($"[ADVERTENCIA UDP 5005] No se pudo abrir el puerto UDP 5005: {exUdp.Message}");
                }

                // Iniciar el vigilante de Heartbeats cada 10 segundos
                _timerHeartbeat = new Timer(RevisarCajerosInactivos, null, 10000, 10000);

                var infoPropia = _bd.ObtenerEstadoInterbancarioPropio();
                string miIp = ClienteInterbancarioNetgear.ObtenerIpLocalHacia("8.8.8.8");

                OnMensajeConsola?.Invoke($"[SERVIDOR] TCP iniciado en 0.0.0.0:{puerto} (IP LAN detectada: {miIp}).");
                OnMensajeConsola?.Invoke($"[BANCO LOCAL] Código={infoPropia.codigoBanco} | Nombre={infoPropia.nombreBanco} | Cupo disp.=Q.{infoPropia.disponibleHoy:0.00}");
                OnMensajeConsola?.Invoke("[SERVIDOR] Esperando conexiones de cajeros y peticiones interbancarias IB_*...");

                Task.Run(() => EscucharClientesAsync(_cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                OnMensajeConsola?.Invoke($"[ERROR] No se pudo iniciar el servidor: {ex.Message}");
            }
        }

        private void DetenerSilencioso()
        {
            try { _cancellationTokenSource?.Cancel(); } catch { }
            try { _udpListener?.Close(); } catch { }
            try { _listener?.Stop(); } catch { }
            try { _timerHeartbeat?.Dispose(); } catch { }
            _udpListener = null;
            _listener = null;
            _timerHeartbeat = null;
        }

        public void Detener()
        {
            DetenerSilencioso();
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
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                        OnMensajeConsola?.Invoke($"[ERROR RED] {ex.Message}");
                }
            }
        }

        private async Task AtenderCajeroAsync(TcpClient cliente, CancellationToken token)
        {
            string idCajero = "Desconocido";
            bool esConexionInterbancaria = false;

            try
            {
                cliente.ReceiveTimeout = 10000;
                cliente.SendTimeout = 10000;

                using NetworkStream stream = cliente.GetStream();
                var utf8SinBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                using StreamReader reader = new StreamReader(stream, utf8SinBom);
                using StreamWriter writer = new StreamWriter(stream, utf8SinBom)
                {
                    AutoFlush = true,
                    NewLine = "\n" // LF real exigido por protocolo NETGEAR
                };

                while (cliente.Connected && !token.IsCancellationRequested)
                {
                    string jsonCompleto = await reader.ReadLineAsync();

                    if (jsonCompleto == null) break;
                    if (string.IsNullOrWhiteSpace(jsonCompleto)) continue;

                    // Máximo aceptado por NETGEAR: 32 KiB de petición
                    if ( utf8SinBom.GetByteCount(jsonCompleto) > 32768)
                    {
                        OnMensajeConsola?.Invoke("[WARN] Petición supera los 32 KiB; cerrando conexión.");
                        break;
                    }

                    OnMensajeConsola?.Invoke($"[MENSAJE RECIBIDO] {jsonCompleto}");

                    try
                    {
                        var preNode = JsonNode.Parse(jsonCompleto);
                        string preTipo = (preNode?["Operacion"]?.ToString() ?? preNode?["tipo"]?.ToString() ?? "").Trim().ToUpperInvariant();
                        if (preTipo.StartsWith("IB_"))
                        {
                            esConexionInterbancaria = true;
                            idCajero = "__IB__";
                        }
                        else if (preNode?["cajero"] != null && idCajero == "Desconocido")
                        {
                            idCajero = preNode["cajero"].ToString();
                        }

                        string respuestaJson = await ProcesarMensajeAsync(jsonCompleto, idCajero, cliente);

                        await writer.WriteLineAsync(respuestaJson);
                        await writer.FlushAsync();

                        // Regla Sección 4 NETGEAR: Para mensajes IB_*, responder una línea JSON con LF y cerrar esa conexión
                        if (esConexionInterbancaria)
                            break;
                    }
                    catch (Exception ex)
                    {
                        OnMensajeConsola?.Invoke($"[ERROR MENSAJE] {ex.Message}");
                        await writer.WriteLineAsync("{\"Exito\":false,\"Mensaje\":\"Error interno del servidor\"}");
                        if (esConexionInterbancaria) break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!esConexionInterbancaria)
                    OnMensajeConsola?.Invoke($"[DESCONEXIÓN] Error con cajero {idCajero}: {ex.Message}");
            }
            finally
            {
                cliente.Close();
                if (!esConexionInterbancaria)
                    OnMensajeConsola?.Invoke($"[RED] El cajero {idCajero} se ha desconectado.");
            }
        }

        private async Task<string> ProcesarMensajeAsync(string jsonCompleto, string idCajero, TcpClient cliente = null)
        {
            var jsonNode = JsonNode.Parse(jsonCompleto);
            string tipo = (jsonNode?["Operacion"]?.ToString() ?? jsonNode?["tipo"]?.ToString() ?? "").Trim().ToUpperInvariant();
            string idSol = jsonNode?["idSolicitud"]?.ToString() ?? "0";

            // =========================================================================
            // 0. PROTOCOLO INTERBANCARIO ENTRANTRE NETGEAR (IB_*)
            // =========================================================================
            if (tipo.StartsWith("IB_"))
            {
                idCajero = "__IB__";
                string ipRemota = (cliente?.Client?.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "";
                return ProcesarMensajeInterbancario(tipo, jsonNode, ipRemota);
            }

            if (jsonNode?["cajero"] != null && idCajero == "Desconocido")
                idCajero = jsonNode["cajero"].ToString();

            if (!string.IsNullOrEmpty(idCajero) && idCajero != "Desconocido" && idCajero != "SERVER")
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

                string estadoReal = _bd.ObtenerEstadoCajero(idCajero);
                return $"{{\"Exito\":true,\"Mensaje\":\"Vivo\",\"Datos\":{{\"Estado\":\"{estadoReal}\"}}}}";
            }

            // --- 3. VALIDAR PIN (LOGIN) ---
            if (tipo == "LOGIN" || tipo == "VALIDAR_PIN")
            {
                var datos = jsonNode["Datos"];
                string tarjeta = datos?["NumeroTarjeta"]?.ToString() ?? jsonNode["tarjeta"]?.ToString();
                string pin = datos?["Pin"]?.ToString() ?? jsonNode["pin"]?.ToString();

                bool exito = _bd.ValidarCredenciales(tarjeta, pin, out int idUsuario, out int idTarjeta, out string nombreUsuario, out string cod, out string err);

                if (exito)
                {
                    _bd.GenerarToken(idUsuario, idCajero, out string token, out _);
                    _sesionesActivas[idCajero] = (idUsuario, idTarjeta);

                    return $"{{\"Exito\":true,\"Mensaje\":\"Ingreso correcto\",\"Datos\":{{\"Token\":\"{token}\", \"Nombre\":\"{nombreUsuario}\"}}}}";
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

                string tokenIng = jsonNode["Datos"]?["TokenOperacion"]?.ToString()
                               ?? jsonNode["tokenOperacion"]?.ToString()
                               ?? jsonNode["Datos"]?["tokenOperacion"]?.ToString();

                if (string.IsNullOrWhiteSpace(tokenIng))
                    return "{\"Exito\":false,\"Mensaje\":\"Token no recibido\"}";

                bool exito = _bd.ValidarTokenOperacion(ids.idUsuario, idCajero, tokenIng, out string err);

                if (exito)
                {
                    _tokensOperacionValidados[idCajero] = DateTime.Now;
                }

                return $"{{\"Exito\":{exito.ToString().ToLower()},\"Mensaje\":\"{(exito ? "Token válido" : err)}\"}}";
            }

            // --- 4. RETIRAR (requiere token ya validado) ---
            if (tipo == "RETIRO" || tipo == "RETIRAR")
            {
                if (!_sesionesActivas.TryGetValue(idCajero, out var ids))
                    return "{\"Exito\":false,\"Mensaje\":\"Sesión no iniciada\"}";

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

                decimal limiteSistema = _bd.ObtenerParametroDecimal("MAX_RETIRO_POR_OPERACION",
                                            _bd.ObtenerParametroDecimal("MAX_CAPACIDAD_CAJERO", 30000m));

                return $"{{\"Exito\":true,\"Datos\":{{" +
                       $"\"SaldoActual\":{saldo.ToString(CultureInfo.InvariantCulture)}," +
                       $"\"LimiteMaximo\":{limite.ToString(CultureInfo.InvariantCulture)}," +
                       $"\"DisponibleHoy\":{disponible.ToString(CultureInfo.InvariantCulture)}," +
                       $"\"LimiteSistema\":{limiteSistema.ToString(CultureInfo.InvariantCulture)}" +
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

            // --- 10. VALIDAR TARJETA ANTES DEL PIN ---
            if (tipo == "VALIDAR_TARJETA")
            {
                var datos = jsonNode["Datos"];
                string tarjeta = datos?["NumeroTarjeta"]?.ToString() ?? jsonNode["tarjeta"]?.ToString();

                bool existe = _bd.ExisteTarjeta(tarjeta);
                if (existe)
                    return "{\"Exito\":true,\"Mensaje\":\"Tarjeta válida\"}";
                else
                    return "{\"Exito\":false,\"Mensaje\":\"La tarjeta no existe, está inactiva o bloqueada.\"}";
            }

            // =========================================================================
            // 11. OPERACIONES INTERBANCARIAS SOLICITADAS DESDE EL CAJERO (ATM -> SERVIDOR -> NETGEAR)
            // =========================================================================
            if (tipo == "BANCOS_AUTORIZADOS")
            {
                var bancos = _bd.ObtenerBancosRed(soloAutorizados: true)
                    .Select(b => new
                    {
                        codigoBanco = b.CodigoBanco,
                        nombreBanco = b.NombreBanco,
                        ip = b.IP,
                        puertoTcp = b.PuertoTcp,
                        cupoDisponible = b.CupoDisponible
                    })
                    .ToList();

                return JsonSerializer.Serialize(new
                {
                    Exito = true,
                    Mensaje = "Bancos autorizados obtenidos",
                    Datos = new { Bancos = bancos }
                });
            }

            if (tipo == "VERIFICAR_CUENTA_IB")
            {
                string bancoDestino = jsonNode?["bancoDestino"]?.ToString() ?? jsonNode?["Datos"]?["bancoDestino"]?.ToString() ?? "";
                string numeroCuenta = jsonNode?["numeroCuenta"]?.ToString() ?? jsonNode?["Datos"]?["numeroCuenta"]?.ToString() ?? "";

                var resVerif = await VerificarCuentaExternaDesdeServidorAsync(bancoDestino, numeroCuenta);
                return JsonSerializer.Serialize(new
                {
                    Exito = resVerif.exito,
                    Mensaje = resVerif.mensaje,
                    Datos = new
                    {
                        Existe = resVerif.exito,
                        NombreTitular = resVerif.nombreTitular,
                        CupoDisponible = resVerif.cupoDisponible,
                        Codigo = resVerif.codigo
                    }
                });
            }

            if (tipo == "TRANSFERIR_IB")
            {
                if (!_sesionesActivas.TryGetValue(idCajero, out var ids))
                    return "{\"Exito\":false,\"Mensaje\":\"Sesión no iniciada en el cajero\"}";

                string bancoDestino = jsonNode?["bancoDestino"]?.ToString() ?? jsonNode?["Datos"]?["bancoDestino"]?.ToString() ?? "";
                string cuentaDestino = jsonNode?["cuentaDestino"]?.ToString() ?? jsonNode?["Datos"]?["cuentaDestino"]?.ToString() ?? "";
                string concepto = jsonNode?["concepto"]?.ToString() ?? jsonNode?["Datos"]?["concepto"]?.ToString() ?? "Transferencia desde cajero";
                string tokenReintento = jsonNode?["tokenTransferencia"]?.ToString() ?? jsonNode?["Datos"]?["tokenTransferencia"]?.ToString() ?? "";

                decimal monto = 0m;
                var nodoMonto = jsonNode?["monto"] ?? jsonNode?["Datos"]?["monto"];
                if (nodoMonto != null)
                    decimal.TryParse(nodoMonto.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out monto);

                var resTransf = await EjecutarTransferenciaSalienteAsync(
                    ids.idUsuario, bancoDestino, cuentaDestino, monto, concepto, tokenReintento);

                return JsonSerializer.Serialize(new
                {
                    Exito = resTransf.exito,
                    Mensaje = resTransf.mensaje,
                    Datos = new
                    {
                        Codigo = resTransf.codigo,
                        TokenTransferencia = resTransf.tokenTransferencia,
                        NombreDestino = resTransf.nombreDestino,
                        Monto = monto
                    }
                });
            }

            return "{\"Exito\":false,\"Mensaje\":\"Operación no soportada\"}";
        }

        // =========================================================================
        // SECCIÓN 3 DE LA GUÍA: DESCUBRIMIENTO UDP OBLIGATORIO EN 0.0.0.0:5005
        // =========================================================================
        private async Task EscucharDescubrimientoUdpAsync(CancellationToken token)
        {
            var utf8SinBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

            while (!token.IsCancellationRequested && _udpListener != null)
            {
                try
                {
                    UdpReceiveResult recibido = await _udpListener.ReceiveAsync();
                    string texto = utf8SinBom.GetString(recibido.Buffer);
                    string[] campos = texto.Split('|');

                    // DISCOVER_BANCO_ATM|BANC02|NETGEAR|192.168.0.34|5000 (Exactamente 5 campos)
                    if (campos.Length != 5 || campos[0] != "DISCOVER_BANCO_ATM")
                        continue;

                    var infoPropia = _bd.ObtenerEstadoInterbancarioPropio();

                    // Si usan el mismo código que nuestro banco, omitimos la respuesta
                    if (string.Equals(campos[1].Trim(), infoPropia.codigoBanco, StringComparison.OrdinalIgnoreCase))
                        continue;

                    string ipLocalHaciaRemitente = ClienteInterbancarioNetgear.ObtenerIpLocalHacia(recibido.RemoteEndPoint.Address);

                    // HERE_BANCO_ATM|BANC03|CAJERORED|192.168.0.40|5000|25000.00|0.00 (Exactamente 7 campos)
                    string respuesta = string.Join("|", new[]
                    {
                        "HERE_BANCO_ATM",
                        infoPropia.codigoBanco,
                        infoPropia.nombreBanco,
                        ipLocalHaciaRemitente,
                        _puertoTcpActual.ToString(CultureInfo.InvariantCulture),
                        infoPropia.limiteDiario.ToString("0.00", CultureInfo.InvariantCulture),
                        infoPropia.usadoHoy.ToString("0.00", CultureInfo.InvariantCulture)
                    });

                    byte[] bytesRespuesta = utf8SinBom.GetBytes(respuesta);

                    // Contestar inmediatamente al IP y PUERTO de origen (recibido.RemoteEndPoint)
                    await _udpListener.SendAsync(bytesRespuesta, bytesRespuesta.Length, recibido.RemoteEndPoint);
                    OnMensajeConsola?.Invoke($"[UDP 5005] DISCOVER de {campos[1]} ({campos[2]}) @ {recibido.RemoteEndPoint} -> Respondido HERE_BANCO_ATM");
                }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                        OnMensajeConsola?.Invoke($"[UDP 5005 ERROR] {ex.Message}");
                }
            }
        }

        // =========================================================================
        // SECCIÓN 4 DE LA GUÍA: PROCESADOR TCP INTERBANCARIO (IB_*)
        // =========================================================================
        private string ProcesarMensajeInterbancario(string tipo, JsonNode req, string ipRemota)
        {
            string idMensaje = req?["idMensaje"]?.ToString() ?? "";
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var infoPropia = _bd.ObtenerEstadoInterbancarioPropio();

            // 1. IB_PING
            if (tipo == "IB_PING")
            {
                string bancoOrig = req?["bancoOrigen"]?.ToString() ?? "";
                OnMensajeConsola?.Invoke($"[IB_PING] Recibido desde {bancoOrig} ({ipRemota}) idMensaje={idMensaje}");

                var respuestaPing = new
                {
                    tipo = "RESP_OK",
                    idMensaje = idMensaje,
                    codigo = "00",
                    mensaje = "Banco disponible",
                    timestamp = timestamp,
                    datos = new
                    {
                        codigoBanco = infoPropia.codigoBanco,
                        nombreBanco = infoPropia.nombreBanco,
                        cupoDisponibleHoy = Math.Round(infoPropia.disponibleHoy, 2),
                        estado = "EN_LINEA"
                    }
                };
                return JsonSerializer.Serialize(respuestaPing);
            }

            // 2. IB_VERIFICAR_CUENTA
            if (tipo == "IB_VERIFICAR_CUENTA")
            {
                // Aceptar numeroCuenta o cuentaDestino como alias; conservar cuentas como texto
                string numeroCuenta = req?["numeroCuenta"]?.ToString()
                                   ?? req?["cuentaDestino"]?.ToString()
                                   ?? "";

                if (string.IsNullOrWhiteSpace(numeroCuenta))
                {
                    return JsonSerializer.Serialize(new
                    {
                        tipo = "RESP_OK",
                        idMensaje = idMensaje,
                        codigo = "30",
                        mensaje = "Formato inválido: falta numeroCuenta",
                        datos = new { existe = false }
                    });
                }

                bool existe = _bd.VerificarCuentaInterbancaria(numeroCuenta, out string nombreTitular);
                OnMensajeConsola?.Invoke($"[IB_VERIFICAR_CUENTA] Cuenta={numeroCuenta} -> Existe={existe} ({nombreTitular})");

                var respuestaCuenta = new
                {
                    tipo = "RESP_OK",
                    idMensaje = idMensaje,
                    codigo = existe ? "00" : "14",
                    mensaje = existe ? "Cuenta verificada" : "Cuenta inexistente",
                    datos = new
                    {
                        existe = existe,
                        codigoBanco = infoPropia.codigoBanco,
                        nombreBanco = infoPropia.nombreBanco,
                        numeroCuenta = numeroCuenta,
                        nombreTitular = existe ? nombreTitular : "",
                        cupoDisponibleBanco = Math.Round(infoPropia.disponibleHoy, 2)
                    }
                };
                return JsonSerializer.Serialize(respuestaCuenta);
            }

            // 3. IB_TRANSFERIR
            if (tipo == "IB_TRANSFERIR")
            {
                string tokenTransferencia = req?["tokenTransferencia"]?.ToString() ?? "";
                string bancoOrigen = req?["bancoOrigen"]?.ToString() ?? "";
                string cuentaOrigen = req?["cuentaOrigen"]?.ToString() ?? "";
                string nombreOrigen = req?["nombreOrigen"]?.ToString() ?? "";
                string bancoDestino = req?["bancoDestino"]?.ToString() ?? "";
                string cuentaDestino = req?["cuentaDestino"]?.ToString() ?? req?["numeroCuenta"]?.ToString() ?? "";
                string concepto = req?["concepto"]?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(tokenTransferencia) ||
                    string.IsNullOrWhiteSpace(bancoOrigen) ||
                    string.IsNullOrWhiteSpace(cuentaOrigen) ||
                    string.IsNullOrWhiteSpace(bancoDestino) ||
                    string.IsNullOrWhiteSpace(cuentaDestino) ||
                    req?["monto"] == null ||
                    !decimal.TryParse(req["monto"].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto))
                {
                    return JsonSerializer.Serialize(new
                    {
                        tipo = "RESP_OK",
                        idMensaje = idMensaje,
                        codigo = "30",
                        mensaje = "Formato inválido en IB_TRANSFERIR",
                        datos = new { }
                    });
                }

                // Confirmación solo DESPUÉS de COMMIT en sp_RecibirTransferenciaIB
                string codigo = _bd.ProcesarTransferenciaEntranteIB(
                    idMensaje, tokenTransferencia, bancoOrigen, cuentaOrigen, nombreOrigen,
                    bancoDestino, cuentaDestino, monto, concepto, ipRemota,
                    out string mensajeBd, out string nombreDestinoReal, out decimal cupoRestante);

                OnMensajeConsola?.Invoke($"[IB_TRANSFERIR] Token={tokenTransferencia} | De={bancoOrigen} ({ipRemota}) | Cuenta={cuentaDestino} | Monto={monto:0.00} => Código={codigo} ({mensajeBd})");

                var respuestaTransferencia = new
                {
                    tipo = "RESP_OK",
                    idMensaje = idMensaje,
                    codigo = codigo,
                    mensaje = mensajeBd,
                    datos = codigo == "00" ? (object)new
                    {
                        tokenTransferencia = tokenTransferencia,
                        bancoDestino = infoPropia.codigoBanco,
                        cuentaDestino = cuentaDestino,
                        nombreDestino = nombreDestinoReal,
                        monto = Math.Round(monto, 2),
                        cupoRestanteBancoDestino = Math.Round(cupoRestante, 2)
                    } : new { tokenTransferencia = tokenTransferencia }
                };

                return JsonSerializer.Serialize(respuestaTransferencia);
            }

            // 4. IB_ESTADO_TRANSFERENCIA (Conciliación)
            if (tipo == "IB_ESTADO_TRANSFERENCIA")
            {
                string bancoOrigen = req?["bancoOrigen"]?.ToString() ?? "";
                string tokenTransferencia = req?["tokenTransferencia"]?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(bancoOrigen) || string.IsNullOrWhiteSpace(tokenTransferencia))
                {
                    return JsonSerializer.Serialize(new
                    {
                        tipo = "RESP_OK",
                        idMensaje = idMensaje,
                        codigo = "30",
                        mensaje = "Formato inválido en IB_ESTADO_TRANSFERENCIA",
                        datos = new { }
                    });
                }

                bool encontrada = _bd.ConsultarEstadoTransferenciaIB(
                    bancoOrigen, tokenTransferencia,
                    out string bancoDest, out string cuentaDest, out string nombreDest, out decimal monto, out decimal cupoRestante);

                OnMensajeConsola?.Invoke($"[IB_ESTADO_TRANSFERENCIA] Token={tokenTransferencia} ({bancoOrigen}) -> Encontrada={encontrada}");

                return JsonSerializer.Serialize(new
                {
                    tipo = "RESP_OK",
                    idMensaje = idMensaje,
                    codigo = encontrada ? "00" : "25",
                    mensaje = encontrada ? "Transferencia acreditada" : "Estado no encontrado",
                    datos = encontrada ? (object)new
                    {
                        tokenTransferencia = tokenTransferencia,
                        bancoDestino = bancoDest,
                        cuentaDestino = cuentaDest,
                        nombreDestino = nombreDest,
                        monto = Math.Round(monto, 2),
                        cupoRestanteBancoDestino = Math.Round(cupoRestante, 2)
                    } : new { tokenTransferencia = tokenTransferencia }
                });
            }

            return JsonSerializer.Serialize(new
            {
                tipo = "RESP_OK",
                idMensaje = idMensaje,
                codigo = "96",
                mensaje = "Operación interbancaria no soportada",
                datos = new { }
            });
        }

        // =========================================================================
        // MÉTODOS PARA ESCANEAR, VERIFICAR Y ENVIAR DESDE NUESTRO SERVIDOR A NETGEAR
        // =========================================================================
        public async Task<List<BancoRedInfo>> EscanearBancosEnRedAsync()
        {
            var propio = _bd.ObtenerEstadoInterbancarioPropio();
            OnMensajeConsola?.Invoke($"[ESCANEO UDP 5005] Enviando DISCOVER_BANCO_ATM como {propio.codigoBanco} ({propio.nombreBanco})...");

            var descubiertos = await ClienteInterbancarioNetgear.EscanearBancosUdpAsync(
                propio.codigoBanco, propio.nombreBanco, _puertoTcpActual);

            foreach (var b in descubiertos)
            {
                _bd.RegistrarBancoDescubierto(b.CodigoBanco, b.NombreBanco, b.IP, b.PuertoTcp, b.LimiteDiario, b.TransferidoHoy);
                OnMensajeConsola?.Invoke($"[DESCUBIERTO] {b.CodigoBanco} | {b.NombreBanco} | {b.IP}:{b.PuertoTcp} | Límite={b.LimiteDiario:0.00} | Usado={b.TransferidoHoy:0.00}");
            }

            if (descubiertos.Count == 0)
                OnMensajeConsola?.Invoke("[ESCANEO UDP 5005] Ningún banco externo respondió en 3s (verificar firewall o registrar IP manual).");

            return _bd.ObtenerBancosRed(soloAutorizados: false);
        }

        public async Task<(bool exito, string mensaje)> ProbarPingBancoAsync(string codigoBancoDestino)
        {
            var banco = _bd.ObtenerBancoRedPorCodigo(codigoBancoDestino);
            if (banco == null)
                return (false, $"El banco {codigoBancoDestino} no está registrado.");

            var propio = _bd.ObtenerEstadoInterbancarioPropio();
            try
            {
                JsonNode resp = await ClienteInterbancarioNetgear.EnviarPingAsync(
                    banco.IP, banco.PuertoTcp, propio.codigoBanco, propio.nombreBanco);

                string codigo = resp?["codigo"]?.ToString() ?? "96";
                string msg = resp?["mensaje"]?.ToString() ?? "";
                string estado = resp?["datos"]?["estado"]?.ToString() ?? "";
                string cupo = resp?["datos"]?["cupoDisponibleHoy"]?.ToString() ?? "";

                OnMensajeConsola?.Invoke($"[PING -> {banco.CodigoBanco}] Código={codigo} | {msg} | Estado={estado} | Cupo={cupo}");
                return (codigo == "00", $"Código={codigo} | {msg} | Estado={estado} | Cupo disp.={cupo}");
            }
            catch (Exception ex)
            {
                OnMensajeConsola?.Invoke($"[ERROR PING -> {banco.CodigoBanco}] {ex.Message}");
                return (false, ex.Message);
            }
        }

        public async Task<(bool exito, string codigo, string nombreTitular, decimal cupoDisponible, string mensaje)> VerificarCuentaExternaDesdeServidorAsync(
            string codigoBancoDestino, string numeroCuenta)
        {
            var banco = _bd.ObtenerBancoRedPorCodigo(codigoBancoDestino);
            if (banco == null || !banco.Activo)
                return (false, "57", "", 0m, "Banco destino no registrado o inactivo.");

            var propio = _bd.ObtenerEstadoInterbancarioPropio();
            try
            {
                JsonNode resp = await ClienteInterbancarioNetgear.VerificarCuentaRemotaAsync(
                    banco.IP, banco.PuertoTcp, propio.codigoBanco, banco.CodigoBanco, numeroCuenta);

                string codigo = resp?["codigo"]?.ToString() ?? "96";
                string mensaje = resp?["mensaje"]?.ToString() ?? "";
                var datos = resp?["datos"];
                bool existe = datos?["existe"] != null && bool.TryParse(datos["existe"].ToString(), out bool b) && b;
                string titular = datos?["nombreTitular"]?.ToString() ?? "";
                decimal cupo = 0m;
                if (datos?["cupoDisponibleBanco"] != null)
                    decimal.TryParse(datos["cupoDisponibleBanco"].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out cupo);

                bool ok = codigo == "00" && existe && !string.IsNullOrWhiteSpace(titular);
                OnMensajeConsola?.Invoke($"[VERIFICAR CUENTA -> {banco.CodigoBanco}] Cuenta={numeroCuenta} => Código={codigo}, Existe={existe}, Titular={titular}");
                return (ok, codigo, titular, cupo, ok ? $"Cuenta verificada: {titular}" : (string.IsNullOrWhiteSpace(mensaje) ? "Cuenta inexistente" : mensaje));
            }
            catch (Exception ex)
            {
                OnMensajeConsola?.Invoke($"[ERROR VERIFICAR CUENTA -> {banco.CodigoBanco}] {ex.Message}");
                return (false, "96", "", 0m, $"Error de conexión con {banco.NombreBanco}: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejecuta el flujo completo para ENVIAR dinero a NETGEAR u otro banco:
        /// 1. Vuelve a verificar cuenta destino por IB_VERIFICAR_CUENTA.
        /// 2. Persiste referencia y reserva saldo/cupo en transacción local corta.
        /// 3. Envía IB_TRANSFERIR por TCP (sin mantener transacción SQL abierta).
        /// 4. Si "00" y datos coinciden -> confirma. Si rechazo definitivo -> revierte reserva.
        ///    Si respuesta incierta/timeout -> mantiene reserva y concilia con IB_ESTADO_TRANSFERENCIA.
        /// </summary>
        public async Task<(bool exito, string codigo, string tokenTransferencia, string nombreDestino, string mensaje)> EjecutarTransferenciaSalienteAsync(
            int idUsuarioOrigen,
            string codigoBancoDestino,
            string cuentaDestino,
            decimal monto,
            string concepto,
            string tokenExistente = null)
        {
            monto = Math.Round(monto, 2);
            if (monto <= 0)
                return (false, "30", "", "", "El monto debe ser mayor a Q.0.00");

            var banco = _bd.ObtenerBancoRedPorCodigo(codigoBancoDestino);
            if (banco == null || !banco.Activo || !banco.Autorizado)
                return (false, "57", "", "", "El banco destino no está registrado o no ha sido autorizado por el administrador.");

            if (!_bd.ObtenerDatosOrigenUsuario(idUsuarioOrigen, out string cuentaOrigen, out string nombreOrigen, out _))
                return (false, "14", "", "", "No se pudo obtener la cuenta origen activa del usuario.");

            var propio = _bd.ObtenerEstadoInterbancarioPropio();

            // 1. Verificar cuenta destino antes de reservar saldo
            var verif = await VerificarCuentaExternaDesdeServidorAsync(banco.CodigoBanco, cuentaDestino);
            if (!verif.exito)
                return (false, verif.codigo, "", "", $"Verificación de cuenta destino falló ({verif.codigo}): {verif.mensaje}");

            string nombreDestino = verif.nombreTitular;
            string token = string.IsNullOrWhiteSpace(tokenExistente)
                ? $"TRF-{DateTime.Now:yyyyMMdd-HHmmss}"
                : tokenExistente.Trim();

            // 2. Reservar saldo en transacción SQL local corta (sin red abierta)
            if (string.IsNullOrWhiteSpace(tokenExistente))
            {
                if (!_bd.ReservarTransferenciaSalienteIB(
                    idUsuarioOrigen, token, propio.codigoBanco, cuentaOrigen, nombreOrigen,
                    banco.CodigoBanco, cuentaDestino, nombreDestino, monto, concepto,
                    out string codRes, out string msgRes))
                {
                    return (false, codRes, token, nombreDestino, msgRes);
                }
            }

            // 3. Enviar IB_TRANSFERIR al banco destino
            try
            {
                JsonNode resp = await ClienteInterbancarioNetgear.EnviarTransferenciaRemotaAsync(
                    banco.IP, banco.PuertoTcp, token, propio.codigoBanco,
                    cuentaOrigen, nombreOrigen, banco.CodigoBanco, cuentaDestino,
                    nombreDestino, monto, concepto);

                string codigo = resp?["codigo"]?.ToString() ?? "96";
                string mensaje = resp?["mensaje"]?.ToString() ?? "";
                var datos = resp?["datos"];

                if (codigo == "00")
                {
                    string tokenResp = datos?["tokenTransferencia"]?.ToString() ?? "";
                    string bancoResp = datos?["bancoDestino"]?.ToString() ?? "";
                    string cuentaResp = datos?["cuentaDestino"]?.ToString() ?? "";
                    decimal montoResp = 0m;
                    if (datos?["monto"] != null)
                        decimal.TryParse(datos["monto"].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out montoResp);

                    // Validar confirmación completa según guía NETGEAR
                    bool confirmacionValida =
                        string.Equals(tokenResp, token, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(bancoResp, banco.CodigoBanco, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(cuentaResp, cuentaDestino.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        Math.Round(montoResp, 2) == monto;

                    if (confirmacionValida)
                    {
                        _bd.ConfirmarTransferenciaSalienteIB(propio.codigoBanco, token, banco.CodigoBanco, monto);
                        OnMensajeConsola?.Invoke($"[IB_TRANSFERIR SALIENTE OK] Token={token} -> {banco.CodigoBanco} ({cuentaDestino}) Q.{monto:0.00}");
                        return (true, "00", token, nombreDestino, $"Transferencia acreditada a {nombreDestino} ({banco.NombreBanco}). Token: {token}");
                    }
                    else
                    {
                        _bd.MarcarTransferenciaSalienteInciertaIB(propio.codigoBanco, token);
                        return (false, "96", token, nombreDestino, $"Confirmación incompleta del banco destino. Reserva mantenida con token {token} para conciliación.");
                    }
                }

                // Rechazo explícito y definitivo (no se acreditó en destino)
                if (codigo == "14" || codigo == "30" || codigo == "51" || codigo == "57" || codigo == "65" || codigo == "94")
                {
                    _bd.RevertirTransferenciaSalienteRechazadaIB(propio.codigoBanco, token, codigo);
                    return (false, codigo, token, nombreDestino, $"Rechazada por {banco.NombreBanco} ({codigo}): {mensaje}");
                }

                // Código "96" u otro incierto: mantener reserva e intentar conciliar con IB_ESTADO_TRANSFERENCIA
                _bd.MarcarTransferenciaSalienteInciertaIB(propio.codigoBanco, token);
                return await ConciliarTransferenciaSalienteAsync(banco, propio.codigoBanco, token, nombreDestino, monto);
            }
            catch (Exception exRed)
            {
                // Timeout o corte de red: mantener reserva (NO devolver ni generar otro token) e intentar conciliar
                OnMensajeConsola?.Invoke($"[IB_TRANSFERIR TIMEOUT/RED] Token={token}: {exRed.Message}. Intentando conciliar...");
                _bd.MarcarTransferenciaSalienteInciertaIB(propio.codigoBanco, token);
                return await ConciliarTransferenciaSalienteAsync(banco, propio.codigoBanco, token, nombreDestino, monto);
            }
        }

        public async Task<(bool exito, string codigo, string tokenTransferencia, string nombreDestino, string mensaje)> ConciliarTransferenciaSalienteAsync(
            BancoRedInfo banco, string miCodigoBanco, string tokenTransferencia, string nombreDestino, decimal monto)
        {
            try
            {
                JsonNode respEstado = await ClienteInterbancarioNetgear.ConsultarEstadoTransferenciaRemotaAsync(
                    banco.IP, banco.PuertoTcp, miCodigoBanco, banco.CodigoBanco, tokenTransferencia);

                string codigo = respEstado?["codigo"]?.ToString() ?? "96";
                if (codigo == "00")
                {
                    _bd.ConfirmarTransferenciaSalienteIB(miCodigoBanco, tokenTransferencia, banco.CodigoBanco, monto);
                    OnMensajeConsola?.Invoke($"[CONCILIACIÓN OK] Token={tokenTransferencia} confirmado en {banco.CodigoBanco}.");
                    return (true, "00", tokenTransferencia, nombreDestino, $"Transferencia conciliada y confirmada (Token: {tokenTransferencia}).");
                }

                return (false, codigo, tokenTransferencia, nombreDestino,
                    $"Transferencia en estado INCIERTO (Código conciliación: {codigo}). Se conserva la reserva del token {tokenTransferencia}; no se reenvía con otro token.");
            }
            catch (Exception ex)
            {
                return (false, "96", tokenTransferencia, nombreDestino,
                    $"Respuesta incierta ({ex.Message}). Reserva mantenida con token {tokenTransferencia} para conciliación posterior.");
            }
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
            if (_cajerosConectados.TryGetValue(codigoCajero, out TcpClient cliente) && cliente != null)
            {
                try
                {
                    using NetworkStream stream = cliente.GetStream();
                    using StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true, NewLine = "\n" };

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
                if ((ahora - kvp.Value).TotalSeconds > 25)
                {
                    OnMensajeConsola?.Invoke($"[ALERTA] El cajero {kvp.Key} se ha desconectado (sin heartbeat por 25s).");
                    _ultimoHeartbeat.TryRemove(kvp.Key, out _);
                    _cajerosConectados.TryRemove(kvp.Key, out _);
                    _tokensOperacionValidados.TryRemove(kvp.Key, out _);

                    _bd.MarcarCajeroDesconectado(kvp.Key);
                }
            }
        }
    }
}
