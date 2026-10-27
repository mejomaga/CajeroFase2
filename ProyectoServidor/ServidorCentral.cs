using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ProyectoServidor.Comunicaciones
{
    public class ServidorCentral
    {
        private TcpListener _listener;
        private CancellationTokenSource _cancellationTokenSource;

        
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
                    // Espera a que un cajero cliente se conecte
                    TcpClient cliente = await _listener.AcceptTcpClientAsync();
                    Console.WriteLine($"[RED] Nueva conexión detectada desde: {cliente.Client.RemoteEndPoint}");

                    // Crea un Task (hilo) independiente para atender exclusivamente a este cajero
                    _ = Task.Run(() => AtenderCajeroAsync(cliente, token));
                }
                catch (ObjectDisposedException)
                {
                    // El servidor se apagó, salir del bucle tranquilamente
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR RED] {ex.Message}");
                }
            }
        }

        private async Task AtenderCajeroAsync(TcpClient cliente, CancellationToken token)
        {
            string idCajero = "Desconocido"; // Se actualizará al recibir REGISTRAR_CAJERO

            try
            {
                using NetworkStream stream = cliente.GetStream();
                using StreamReader reader = new StreamReader(stream);
                using StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

                while (cliente.Connected && !token.IsCancellationRequested)
                {
                    // Lee el JSON entrante de forma asíncrona
                    string mensajeJson = await reader.ReadLineAsync();

                    // Si el mensaje es nulo, el cajero se desconectó físicamente
                    if (string.IsNullOrEmpty(mensajeJson)) break;

                    Console.WriteLine($"[MENSAJE RECIBIDO] {mensajeJson}");

                    // --------------------------------------------------------
                    // AQUÍ PROCESAREMOS EL JSON Y APLICAREMOS LA LÓGICA DE NEGOCIO
                    // --------------------------------------------------------

                    // Respuesta temporal genérica en formato JSON
                    string respuesta = "{\"tipo\": \"RESPUESTA\", \"codigo\":\"00\", \"mensaje\":\"Conectado al servidor central\"}";
                    await writer.WriteLineAsync(respuesta);
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

                // Si el cajero estaba registrado, lo quitamos de la lista activa
                if (idCajero != "Desconocido")
                {
                    _cajerosConectados.TryRemove(idCajero, out _);
                }
            }
        }
    }
}