using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ProyectoServidor.Modelos; // Importa las clases MensajeRed y MensajeTransaccion

namespace ProyectoServidor.Comunicaciones
{
    public class ServidorCentral
    {
        private TcpListener _listener;
        private CancellationTokenSource _cancellationTokenSource;

        // Diccionario seguro para guardar los cajeros conectados y su estado en tiempo real
        private ConcurrentDictionary<string, TcpClient> _cajerosConectados;

        public ServidorCentral()
        {
            _cajerosConectados = new ConcurrentDictionary<string, TcpClient>();
        }

        // Inicia el servidor en el puerto 5000 por defecto
        public void Iniciar(int puerto = 5000)
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, puerto);
                _listener.Start();
                _cancellationTokenSource = new CancellationTokenSource();

                Console.WriteLine($"[SERVIDOR] Iniciado correctamente en el puerto {puerto}.");
                Console.WriteLine("[SERVIDOR] Esperando conexiones de cajeros...");

                // Hilo principal de escucha (No bloqueante)
                Task.Run(() => EscucharClientesAsync(_cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] No se pudo iniciar el servidor: {ex.Message}");
            }
        }

        // Detiene el servidor de forma segura liberando el puerto de red
        public void Detener()
        {
            _cancellationTokenSource?.Cancel();
            _listener?.Stop();
            Console.WriteLine("[SERVIDOR] Apagado.");
        }

        private async Task EscucharClientesAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient cliente = await _listener.AcceptTcpClientAsync();
                    Console.WriteLine($"[RED] Nueva conexión detectada desde: {cliente.Client.RemoteEndPoint}");

                    // Crea un Task (hilo) independiente para atender exclusivamente a este cajero
                    _ = Task.Run(() => AtenderCajeroAsync(cliente, token));
                }
                catch (ObjectDisposedException)
                {
                    break; // El servidor se apagó, salir del bucle tranquilamente
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR RED] {ex.Message}");
                }
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
                    // Lee el JSON entrante de forma asíncrona
                    string mensajeJson = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(mensajeJson)) break;

                    Console.WriteLine($"[MENSAJE RECIBIDO] {mensajeJson}");

                    try
                    {
                        // 1. Convertir el texto JSON en un objeto básico
                        var mensajeBase = JsonSerializer.Deserialize<MensajeRed>(mensajeJson);

                        if (mensajeBase == null || string.IsNullOrEmpty(mensajeBase.Tipo))
                        {
                            await writer.WriteLineAsync("{\"tipo\": \"RESPUESTA\", \"codigo\":\"96\", \"mensaje\":\"Formato JSON inválido\"}");
                            continue;
                        }

                        // 2. Enrutar la petición a la lógica de negocio correcta
                        switch (mensajeBase.Tipo.ToUpper())
                        {
                            case "REGISTRAR_CAJERO":
                                idCajero = mensajeBase.Cajero;
                                _cajerosConectados.TryAdd(idCajero, cliente);
                                Console.WriteLine($"[REGISTRO] Cajero {idCajero} registrado con éxito.");
                                await writer.WriteLineAsync("{\"tipo\": \"RESPUESTA\", \"codigo\":\"00\", \"mensaje\":\"Cajero Registrado\"}");
                                break;

                            case "HEARTBEAT":
                                await writer.WriteLineAsync("{\"tipo\": \"RESPUESTA\", \"codigo\":\"00\", \"mensaje\":\"Vivo\"}");
                                break;

                            case "LOGIN":
                                // Aquí Juan validará en SQL Server si el PIN es correcto para generar el Token
                                await writer.WriteLineAsync($"{{\"tipo\": \"RESPUESTA\", \"idSolicitud\":\"{mensajeBase.IdSolicitud}\", \"codigo\":\"00\", \"mensaje\":\"Credenciales validadas, ingrese Token\"}}");
                                break;

                            case "VALIDAR_TOKEN":
                                // Aquí validaremos que el token tenga menos de 2 minutos
                                await writer.WriteLineAsync($"{{\"tipo\": \"RESPUESTA\", \"idSolicitud\":\"{mensajeBase.IdSolicitud}\", \"codigo\":\"00\", \"mensaje\":\"Token válido, sesión iniciada\"}}");
                                break;

                            case "RETIRO":
                                var mensajeRetiro = JsonSerializer.Deserialize<MensajeTransaccion>(mensajeJson);
                                Console.WriteLine($"[TRANSACCION] El cajero {idCajero} solicita retirar Q {mensajeRetiro.Monto}");
                                await writer.WriteLineAsync($"{{\"tipo\": \"RESPUESTA\", \"idSolicitud\":\"{mensajeBase.IdSolicitud}\", \"codigo\":\"00\", \"mensaje\":\"Retiro aprobado\"}}");
                                break;

                            case "DEPOSITO":
                                var mensajeDeposito = JsonSerializer.Deserialize<MensajeTransaccion>(mensajeJson);
                                Console.WriteLine($"[TRANSACCION] El cajero {idCajero} deposita Q {mensajeDeposito.Monto}");
                                await writer.WriteLineAsync($"{{\"tipo\": \"RESPUESTA\", \"idSolicitud\":\"{mensajeBase.IdSolicitud}\", \"codigo\":\"00\", \"mensaje\":\"Depósito aprobado\"}}");
                                break;

                            default:
                                Console.WriteLine($"[ADVERTENCIA] Tipo de mensaje desconocido: {mensajeBase.Tipo}");
                                await writer.WriteLineAsync("{\"tipo\": \"RESPUESTA\", \"codigo\":\"96\", \"mensaje\":\"Operación no soportada\"}");
                                break;
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        Console.WriteLine($"[ERROR JSON] {jsonEx.Message}");
                        await writer.WriteLineAsync("{\"tipo\": \"RESPUESTA\", \"codigo\":\"96\", \"mensaje\":\"Error de parseo JSON\"}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DESCONEXIÓN] Error con cajero {idCajero}: {ex.Message}");
            }
            finally
            {
                cliente.Close();
                Console.WriteLine($"[RED] El cajero {idCajero} se ha desconectado.");

                if (idCajero != "Desconocido")
                {
                    _cajerosConectados.TryRemove(idCajero, out _);
                }
            }
        }
    }
}