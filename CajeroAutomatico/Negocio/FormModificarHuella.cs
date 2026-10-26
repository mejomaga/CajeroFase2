using CajeroAutomatico.Entidades;
using CajeroAutomatico.Presentacion;
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

namespace CajeroAutomatico.Negocio
{
    public partial class FormModificarHuella : Form
    {
        private Usuario usuarioEncontrado;
        private int _slotRegistrado = -1;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);

        public FormModificarHuella()
        {
            InitializeComponent();
        }

        private void FormModificarHuella_Load(object sender, EventArgs e)
        {
            // Suscribirse a los eventos del lector
            Program.Servicio.HuellaRegistrada += HuellaRegistrada;

            // Estado inicial
            btnRegistrar.Enabled = false;
            lblEstadoHuella.Text = "";
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            usuarioEncontrado = Program.Servicio.BuscarPorTarjeta(txtTarjetaActual.Text.Trim());

            if (usuarioEncontrado == null)
            {
                MessageBox.Show("No se encontró ningún usuario con esa tarjeta.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblNombreUsuario.Text = "Usuario: No encontrado";
                btnRegistrar.Enabled = false;
            }
            else
            {
                lblNombreUsuario.Text = $"Usuario encontrado: {usuarioEncontrado.Nombre}";
                btnRegistrar.Enabled = true;
            }
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            if (usuarioEncontrado == null)
            {
                MessageBox.Show("Primero busque un usuario.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Registrar la huella del usuario {usuarioEncontrado.Nombre}?\n\n" +
                "El usuario debe poner el dedo en el sensor dos veces.",
                "Confirmar registro",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes) return;

            _slotRegistrado = -1;
            lblEstadoHuella.ForeColor = Color.Blue;
            lblEstadoHuella.Text = "Iniciando registro de huella...";
            btnRegistrar.Enabled = false;

            Program.Servicio.IniciarLector();

            // 👇 NUEVO: si el usuario ya tenía huella, borrarla antes
            if (usuarioEncontrado.HuellaSlot > 0)
            {
                Program.Servicio.BorrarHuella(usuarioEncontrado.HuellaSlot);
                System.Threading.Thread.Sleep(500); // Dar tiempo al Arduino
            }

            Program.Servicio.RegistrarHuella();
        }

        // ----- EVENTO: HUELLA REGISTRADA -----
        private void HuellaRegistrada(int slot)
        {
            // 👇 Protección: si el formulario ya no está vivo, ignorar
            if (!this.IsHandleCreated || this.IsDisposed) return;

            this.Invoke(new Action(() =>
            {
                _slotRegistrado = slot;

                var resultado = Program.Servicio.AsignarHuella(usuarioEncontrado, slot);

                if (resultado.Exito)
                {
                    lblEstadoHuella.ForeColor = Color.Green;
                    lblEstadoHuella.Text = $"Huella registrada en slot {slot} y asignada a {usuarioEncontrado.Nombre}.";
                    lblMensaje.Text = resultado.Mensaje;
                }
                else
                {
                    lblEstadoHuella.ForeColor = Color.Red;
                    lblEstadoHuella.Text = "Error al asignar la huella: " + resultado.Mensaje;
                }

                btnRegistrar.Enabled = true;
            }));
        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormMenuAdmin());
        }

        private void FormModificarHuella_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Servicio.HuellaRegistrada -= HuellaRegistrada;
            Program.Servicio.DetenerLector();
        }

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

        private void btnTeclado_Click(object sender, EventArgs e)
        {
            AbrirTecladoVirtual();
        }
    }
}
