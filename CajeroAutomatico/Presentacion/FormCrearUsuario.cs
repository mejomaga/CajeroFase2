using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CajeroAutomatico.Presentacion
{
    public partial class FormCrearUsuario : Form
    {
        private bool esperandoHuella = false;
        private string _uidVinculado = "";
        private int _slotHuella = -1;

        // 1. IMPORTACIONES DE KERNEL32 PARA EL SALTO DE 32 A 64 BITS
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);

        public FormCrearUsuario()
        {
            InitializeComponent();
            this.Load += FormCrearUsuario_Load;
            this.FormClosing += FormCrearUsuario_FormClosing;
        }

        // 2. AQUÍ VA EL MÉTODO
        private void AbrirTecladoVirtual()
        {
            try
            {
                // Si ya está abierto en pantalla, no hacemos nada
                if (Process.GetProcessesByName("osk").Length > 0)
                    return;

                IntPtr ptr = IntPtr.Zero;
                bool isRedirected = false;

                // Desactiva la redirección de 32 bits a SysWOW64
                if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
                {
                    isRedirected = Wow64DisableWow64FsRedirection(ref ptr);
                }

                try
                {
                    string rutaOsk = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "osk.exe");

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = rutaOsk,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                finally
                {
                    // Restauramos la redirección del sistema
                    if (isRedirected)
                    {
                        Wow64RevertWow64FsRedirection(ptr);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el teclado virtual: {ex.Message}", "Teclado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CerrarTecladoVirtual()
        {
            try
            {
                foreach (var p in Process.GetProcessesByName("osk"))
                {
                    p.Kill();
                }
            }
            catch { }
        }

        // 3. AQUÍ LO LLAMAS AL HACER CLIC EN EL BOTÓN (reemplaza el nombre por el de tu botón)
        private void btnTeclado_Click(object sender, EventArgs e)
        {
            AbrirTecladoVirtual();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            /*if (string.IsNullOrWhiteSpace(_uidVinculado))
            {
                lblMensaje.ForeColor = Color.Red;
                lblMensaje.Text = "Debe vincular una tarjeta antes de guardar.";
                return;
            }

            if (_slotHuella <= 0)
            {
                lblMensaje.ForeColor = Color.Red;
                lblMensaje.Text = "Debe registrar la huella del usuario antes de guardar.";
                return;
            }*/

            var resultado = Program.Servicio.CrearUsuario(
                txtNombre.Text,
                txtNumeroTarjeta.Text,
                txtPin.Text,
                numSaldoInicial.Value,
                numLimiteDiario.Value,
                /*txtHuella.Text,
                _uidVinculado,
                _slotHuella*/
                "",   // huella vacía
                "",   // UID vacío
                -1    // slot de huella no asignado

            );

            lblMensaje.ForeColor = resultado.Exito ? Color.Green : Color.Red;
            lblMensaje.Text = resultado.Mensaje;

            if (resultado.Exito)
            {
                txtNombre.Text = "";
                txtNumeroTarjeta.Text = "";
                txtPin.Text = "";
                numSaldoInicial.Value = 0;
                numLimiteDiario.Value = 0;
                txtHuella.Text = "";
                _uidVinculado = "";
                _slotHuella = -1;
                lblEstadoVinculacion.Text = "";
                lblEstadoHuella.Text = "";

                CerrarTecladoVirtual();
            }

            MessageBox.Show(resultado.Mensaje, resultado.Exito ? "Éxito" : "Error",
                MessageBoxButtons.OK, resultado.Exito ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            if (resultado.Exito)
                Program.Navegar(this, new FormMenuAdmin());
        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            CerrarTecladoVirtual();
            Program.Navegar(this, new FormMenuAdmin());
        }

        private void txtNumeroTarjeta_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
        }

        private void FormCrearUsuario_Load(object sender, EventArgs e)
        {
            //Program.Servicio.TarjetaDetectada += TarjetaDetectada;

            lblEstadoVinculacion.Text = "";
            btnVincularTarjeta.Enabled = false;
            //Program.Servicio.HuellaRegistrada += HuellaRegistrada;
        }

        private void btnVincularTarjeta_Click(object sender, EventArgs e)
        {
            _uidVinculado = "";
            lblEstadoVinculacion.ForeColor = Color.Blue;
            lblEstadoVinculacion.Text = "Acerca la tarjeta al lector...";
            btnVincularTarjeta.Enabled = false;

            Program.Servicio.IniciarLector();
        }

        private void TarjetaDetectada(string uid)
        {
            // Si el formulario ya no está vivo, ignorar
            if (!this.IsHandleCreated || this.IsDisposed) return;

            this.Invoke(new Action(() =>
            {
                if (btnVincularTarjeta.Enabled) return;

                _uidVinculado = uid;
                lblEstadoVinculacion.ForeColor = Color.Green;
                lblEstadoVinculacion.Text = "Tarjeta vinculada: " + uid;
                btnVincularTarjeta.Enabled = true;
            }));
        }

        private void FormCrearUsuario_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Program.Servicio.TarjetaDetectada -= TarjetaDetectada;
            //Program.Servicio.HuellaRegistrada -= HuellaRegistrada;
            //Program.Servicio.DetenerLector();
        }

        private void btnRegistrarHuella_Click(object sender, EventArgs e)
        {
            var confirmacion = MessageBox.Show(
       "¿Registrar la huella del usuario nuevo?\n\n" +
       "El usuario debe poner el dedo en el sensor dos veces.",
       "Confirmar registro",
       MessageBoxButtons.YesNo,
       MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes) return;

            _slotHuella = -1;
            lblEstadoHuella.ForeColor = Color.Blue;
            lblEstadoHuella.Text = "Iniciando registro de huella...";
            btnRegistrarHuella.Enabled = false;

            Program.Servicio.IniciarLector();
            Program.Servicio.RegistrarHuella();
        }

        private void HuellaRegistrada(int slot)
        {
            if (!this.IsHandleCreated || this.IsDisposed) return;

            this.Invoke(new Action(() =>
            {
                _slotHuella = slot;
                lblEstadoHuella.ForeColor = Color.Green;
                lblEstadoHuella.Text = $"Huella registrada en slot {slot}.";
                btnRegistrarHuella.Enabled = true;
            }));
        }
    }
}