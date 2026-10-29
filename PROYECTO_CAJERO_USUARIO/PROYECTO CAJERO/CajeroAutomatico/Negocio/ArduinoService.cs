using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace CajeroAutomatico.Negocio
{
    // Contrato reservado para la futura integración con Arduino
    // (sensor de huellas dactilares y lector de tarjetas NFC).
    public interface IArduinoService
    {
        bool VerificarConexion();
        string LeerTarjetaNFC();
        bool VerificarHuella(string numeroTarjeta);

        event Action<string> TarjetaDetectada;
        event Action<int> HuellaRegistrada;
        event Action<int> HuellaDetectada;

        void Iniciar();
        void Detener();
        void RegistrarHuella();
        void ModoEsperarHuella(bool esperar);
        void BorrarHuella(int slot);
    }

    // Implementación temporal mientras no se conecta el hardware real.
    public class ArduinoService : IArduinoService, IDisposable
    {
        private SerialPort _serialPort;
        private const string Puerto = "COM4";
        private const int Baudios = 9600;

        private bool _esperandoHuella = false;

        // Evento que se dispara cuando llega un UID
        public event Action<string> TarjetaDetectada;
        public event Action<int> HuellaRegistrada;
        public event Action<int> HuellaDetectada;

        public bool VerificarConexion()
        {
            /*try
            {
                if (_serialPort == null)
                {
                    _serialPort = new SerialPort(Puerto, Baudios);
                    _serialPort.DataReceived += SerialPort_DataReceived;
                }

                if (!_serialPort.IsOpen)
                    _serialPort.Open();

                return true;
            }
            catch*/
            {
                return false;
            }
        }

        public void Iniciar()
        {
           //VerificarConexion();
        }

        public void Detener()
        {
            /*if (_serialPort != null)
            {
                try
                {
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    if (_serialPort.IsOpen) _serialPort.Close();
                    _serialPort.Dispose();
                }
                catch { }
                _serialPort = null;  // 👈 CLAVE: destruirlo
            }*/
        }

        public void RegistrarHuella()
        {
            //if (!VerificarConexion()) return;
            //_serialPort.WriteLine("REGISTRAR_HUELLA");
        }

        public void BorrarHuella(int slot)
        {
            //if (!VerificarConexion()) return;
            //_serialPort.WriteLine("BORRAR_HUELLA:" + slot);
        }

        public void ModoEsperarHuella(bool esperar)
        {
            //_esperandoHuella = esperar;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp = (SerialPort)sender;
                System.Threading.Thread.Sleep(100);

                while (sp.BytesToRead > 0)
                {
                    string linea = sp.ReadLine().Trim();

                    if (linea.StartsWith("Tag id:"))
                    {
                        string uid = linea.Substring("Tag id:".Length).Trim();
                        TarjetaDetectada?.Invoke(uid);
                    }
                    else if (linea.StartsWith("HUELLA_OK:"))
                    {
                        string slotStr = linea.Substring("HUELLA_OK:".Length).Trim();
                        if (int.TryParse(slotStr, out int slot))
                        {
                            HuellaRegistrada?.Invoke(slot);
                        }
                    }
                    else if (linea.StartsWith("HUELLA_ERROR"))
                    {
                        // Notificar error (opcional)
                        System.Diagnostics.Debug.WriteLine("Error de huella: " + linea);
                    }
                    else if (linea.StartsWith("Huella:") && _esperandoHuella)
                    {
                        string slotStr = linea.Substring("Huella:".Length).Trim();

                        if (slotStr == "NO_RECONOCIDA")
                        {
                            // Huella no reconocida
                            HuellaDetectada?.Invoke(-1);
                        }
                        else if (int.TryParse(slotStr, out int slot))
                        {
                            HuellaDetectada?.Invoke(slot);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Para evitar el MessageBox peligroso si algún día se dispara, cambia solo el catch
                System.Diagnostics.Debug.WriteLine("Error en DataReceived: " + ex.Message);
            }

        }

        public string LeerTarjetaNFC()
        {
            // TODO: leer el número de tarjeta real desde el módulo NFC
            throw new NotImplementedException("Integración con NFC pendiente.");
        }

        public bool VerificarHuella(string numeroTarjeta)
        {
            // TODO: comparar la huella leída contra la registrada
            throw new NotImplementedException("Integración con sensor de huella pendiente.");
        }

        public void Dispose()
        {
            Detener();
        }
    }
}
