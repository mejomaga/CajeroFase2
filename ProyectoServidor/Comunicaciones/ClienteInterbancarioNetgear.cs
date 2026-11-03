using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ProyectoServidor.Comunicaciones
{
    public class BancoRedInfo
    {
        public string CodigoBanco { get; set; } = string.Empty;
        public string NombreBanco { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public int PuertoTcp { get; set; } = 5000;
        public int PuertoUdp { get; set; } = 5005;
        public decimal LimiteDiario { get; set; } = 25000.00m;
        public decimal TransferidoHoy { get; set; } = 0.00m;
        public bool Activo { get; set; } = true;
        public bool Autorizado { get; set; } = false;

        public decimal CupoDisponible => Math.Max(0m, LimiteDiario - TransferidoHoy);

        public override string ToString()
        {
            string estadoAut = Autorizado ? "AUTORIZADO" : "NO AUTORIZADO";
            return $"{CodigoBanco} | {NombreBanco} | {IP}:{PuertoTcp} | Cupo: {CupoDisponible:0.00} | {estadoAut}";
        }
    }

    public static class ClienteInterbancarioNetgear
    {
        private static readonly Encoding Utf8SinBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        public const int PuertoUdpDefault = 5005;

        /// <summary>
        /// Determina la IPv4 local de nuestro adaptador que enruta hacia el destino indicado.
        /// Equivalente a ip_hacia(destino) de la guía NETGEAR.
        /// </summary>
        public static string ObtenerIpLocalHacia(string ipDestino)
        {
            try
            {
                if (!IPAddress.TryParse(ipDestino, out IPAddress addr))
                    addr = IPAddress.Parse("8.8.8.8");

                return ObtenerIpLocalHacia(addr);
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        public static string ObtenerIpLocalHacia(IPAddress destino)
        {
            try
            {
                if (IPAddress.IsLoopback(destino) || destino.Equals(IPAddress.Broadcast) || destino.Equals(IPAddress.Any))
                {
                    using Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    s.Connect("8.8.8.8", PuertoUdpDefault);
                    return ((IPEndPoint)s.LocalEndPoint).Address.ToString();
                }

                using Socket socketRuta = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socketRuta.Connect(destino, PuertoUdpDefault);
                return ((IPEndPoint)socketRuta.LocalEndPoint).Address.ToString();
            }
            catch
            {
                try
                {
                    foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip) && !ip.ToString().StartsWith("169.254."))
                            return ip.ToString();
                    }
                }
                catch { }
                return "127.0.0.1";
            }
        }

        /// <summary>
        /// Escanea bancos en la LAN enviando DISCOVER_BANCO_ATM|MI_CODIGO|MI_NOMBRE|MI_IP|5000 por UDP Broadcast al puerto 5005
        /// y espera respuestas HERE_BANCO_ATM|... durante 3 segundos.
        /// </summary>
        public static async Task<List<BancoRedInfo>> EscanearBancosUdpAsync(
            string miCodigo,
            string miNombre,
            int miPuertoTcp = 5000,
            int timeoutMs = 2800)
        {
            var encontrados = new Dictionary<string, BancoRedInfo>(StringComparer.OrdinalIgnoreCase);
            string miIp = ObtenerIpLocalHacia("8.8.8.8");

            using var udp = new UdpClient(new IPEndPoint(IPAddress.Any, 0))
            {
                EnableBroadcast = true
            };

            // Exactamente 5 campos separados por |
            string tramaDiscover = string.Join("|", new[]
            {
                "DISCOVER_BANCO_ATM",
                miCodigo.Trim(),
                miNombre.Replace("|", "").Trim(),
                miIp,
                miPuertoTcp.ToString(CultureInfo.InvariantCulture)
            });

            byte[] payload = Utf8SinBom.GetBytes(tramaDiscover);

            // Enviar a broadcast global 255.255.255.255:5005 y al broadcast de la subred local /24
            await udp.SendAsync(payload, payload.Length, new IPEndPoint(IPAddress.Broadcast, PuertoUdpDefault));
            try
            {
                string[] octetos = miIp.Split('.');
                if (octetos.Length == 4)
                {
                    string broadcastSubred = $"{octetos[0]}.{octetos[1]}.{octetos[2]}.255";
                    if (IPAddress.TryParse(broadcastSubred, out IPAddress ipSubred))
                        await udp.SendAsync(payload, payload.Length, new IPEndPoint(ipSubred, PuertoUdpDefault));
                }
            }
            catch { }

            DateTime limiteTiempo = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < limiteTiempo)
            {
                TimeSpan restante = limiteTiempo - DateTime.UtcNow;
                if (restante.TotalMilliseconds <= 0) break;

                var receiveTask = udp.ReceiveAsync();
                var completada = await Task.WhenAny(receiveTask, Task.Delay(restante));
                if (completada != receiveTask) break;

                UdpReceiveResult datagrama = await receiveTask;
                if (datagrama.Buffer == null || datagrama.Buffer.Length > 1024) continue;

                string texto = Utf8SinBom.GetString(datagrama.Buffer).Trim();
                string[] campos = texto.Split('|');

                // HERE_BANCO_ATM|BANC03|BANCO_PYTHON|192.168.0.40|5000|25000.00|0.00 (7 campos exactos)
                if (campos.Length != 7 || campos[0] != "HERE_BANCO_ATM")
                    continue;

                string codRemoto = campos[1].Trim();
                if (string.IsNullOrWhiteSpace(codRemoto) ||
                    string.Equals(codRemoto, miCodigo.Trim(), StringComparison.OrdinalIgnoreCase))
                    continue;

                string nomRemoto = campos[2].Trim();
                int puertoTcp = int.TryParse(campos[4].Trim(), out int p) && p > 0 ? p : 5000;
                decimal.TryParse(campos[5].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lim);
                decimal.TryParse(campos[6].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal usado);

                // Tomar la IP real del datagrama recibido (como hace NETGEAR)
                string ipReal = datagrama.RemoteEndPoint.Address.ToString();

                encontrados[codRemoto] = new BancoRedInfo
                {
                    CodigoBanco = codRemoto,
                    NombreBanco = nomRemoto,
                    IP = ipReal,
                    PuertoTcp = puertoTcp,
                    PuertoUdp = PuertoUdpDefault,
                    LimiteDiario = lim > 0 ? lim : 25000.00m,
                    TransferidoHoy = Math.Max(0m, usado),
                    Activo = true
                };
            }

            return new List<BancoRedInfo>(encontrados.Values);
        }

        /// <summary>
        /// Envía una petición TCP interbancaria (IB_PING, IB_VERIFICAR_CUENTA, IB_TRANSFERIR, IB_ESTADO_TRANSFERENCIA)
        /// terminada en LF (\n) sin BOM y lee la respuesta JSON hasta LF (\n). Timeout por defecto: 10s.
        /// </summary>
        public static async Task<JsonNode> EnviarPeticionIbAsync(string ipDestino, int puertoTcp, object peticion, int timeoutMs = 10000)
        {
            using var tcp = new TcpClient();
            var connectTask = tcp.ConnectAsync(ipDestino, puertoTcp);
            if (await Task.WhenAny(connectTask, Task.Delay(timeoutMs)) != connectTask)
                throw new TimeoutException($"Tiempo de espera agotado ({timeoutMs} ms) conectando a {ipDestino}:{puertoTcp}");

            await connectTask;
            tcp.ReceiveTimeout = timeoutMs;
            tcp.SendTimeout = timeoutMs;

            using NetworkStream stream = tcp.GetStream();
            using var writer = new StreamWriter(stream, Utf8SinBom) { AutoFlush = true, NewLine = "\n" };
            using var reader = new StreamReader(stream, Utf8SinBom);

            string jsonEnvio = JsonSerializer.Serialize(peticion);
            await writer.WriteLineAsync(jsonEnvio);
            await writer.FlushAsync();

            var readTask = reader.ReadLineAsync();
            if (await Task.WhenAny(readTask, Task.Delay(timeoutMs)) != readTask)
                throw new TimeoutException($"Tiempo de espera agotado ({timeoutMs} ms) esperando respuesta de {ipDestino}:{puertoTcp}");

            string jsonRespuesta = await readTask;
            if (string.IsNullOrWhiteSpace(jsonRespuesta))
                throw new IOException("El servidor bancario remoto cerró la conexión sin devolver JSON.");

            return JsonNode.Parse(jsonRespuesta);
        }

        public static Task<JsonNode> EnviarPingAsync(string ipDestino, int puertoTcp, string miCodigoBanco, string miNombreBanco)
        {
            string idMsg = $"PING-{DateTime.Now:HHmmssfff}";
            var req = new
            {
                tipo = "IB_PING",
                cajero = "SERVER",
                idMensaje = idMsg,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                bancoOrigen = miCodigoBanco,
                nombreBancoOrigen = miNombreBanco
            };
            return EnviarPeticionIbAsync(ipDestino, puertoTcp, req);
        }

        public static Task<JsonNode> VerificarCuentaRemotaAsync(
            string ipDestino, int puertoTcp, string miCodigoBanco, string codigoBancoDestino, string numeroCuenta)
        {
            string idMsg = $"CUENTA-{DateTime.Now:HHmmssfff}";
            var req = new
            {
                tipo = "IB_VERIFICAR_CUENTA",
                cajero = "SERVER",
                idMensaje = idMsg,
                bancoOrigen = miCodigoBanco,
                bancoDestino = codigoBancoDestino,
                numeroCuenta = numeroCuenta.Trim()
            };
            return EnviarPeticionIbAsync(ipDestino, puertoTcp, req);
        }

        public static Task<JsonNode> EnviarTransferenciaRemotaAsync(
            string ipDestino,
            int puertoTcp,
            string tokenTransferencia,
            string miCodigoBanco,
            string cuentaOrigen,
            string nombreOrigen,
            string bancoDestino,
            string cuentaDestino,
            string nombreDestino,
            decimal monto,
            string concepto)
        {
            string idMsg = $"ENVIO-{DateTime.Now:HHmmssfff}";
            var req = new
            {
                tipo = "IB_TRANSFERIR",
                cajero = "SERVER",
                idMensaje = idMsg,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                tokenTransferencia = tokenTransferencia,
                bancoOrigen = miCodigoBanco,
                cuentaOrigen = cuentaOrigen,
                nombreOrigen = nombreOrigen,
                bancoDestino = bancoDestino,
                cuentaDestino = cuentaDestino,
                nombreDestino = nombreDestino,
                monto = Math.Round(monto, 2),
                concepto = string.IsNullOrWhiteSpace(concepto) ? "Transferencia interbancaria" : concepto
            };
            return EnviarPeticionIbAsync(ipDestino, puertoTcp, req);
        }

        public static Task<JsonNode> ConsultarEstadoTransferenciaRemotaAsync(
            string ipDestino, int puertoTcp, string miCodigoBanco, string bancoDestino, string tokenTransferencia)
        {
            string idMsg = $"ESTADO-{DateTime.Now:HHmmssfff}";
            var req = new
            {
                tipo = "IB_ESTADO_TRANSFERENCIA",
                idMensaje = idMsg,
                bancoOrigen = miCodigoBanco,
                bancoDestino = bancoDestino,
                tokenTransferencia = tokenTransferencia
            };
            return EnviarPeticionIbAsync(ipDestino, puertoTcp, req);
        }
    }
}
