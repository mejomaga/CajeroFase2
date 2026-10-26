using CajeroAutomatico.Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CajeroAutomatico.Presentacion
{
    public partial class FormInsertarTarjeta : Form
    {

        private bool _procesando = false;

        public FormInsertarTarjeta()
        {
            InitializeComponent();
            this.Load += FormInsertarTarjeta_Load;
            this.FormClosing += FormInsertarTarjeta_FormClosing;
        }

        private void btnContinuar_Click(object sender, EventArgs e)
        {
            var (usuario, permitido, mensaje) = Program.Servicio.ValidarTarjeta(txtNumeroTarjeta.Text.Trim());

            if (!permitido)
            {
                mensaje = string.IsNullOrEmpty(mensaje) ? "Tarjeta inválida" : mensaje;
                MessageBox.Show(mensaje, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Program.Navegar(this, new FormVerificacion(usuario));
        }

        private void FormInsertarTarjeta_Load(object sender, EventArgs e)
        {
            VozService.Hablar("Bienvenido. Por favor, inserte su tarjeta o acérquela al lector.");
            // Mensaje visual
            lblMensaje.Text = "Inserte su tarjeta";
            lblEstado.Text = "";
            _procesando = false;

            // Suscribirse al evento del lector
            Program.Servicio.TarjetaDetectada += TarjetaDetectada;

            // Empezar a escuchar automáticamente
            Program.Servicio.IniciarLector();
        }

        private void TarjetaDetectada(string uid)
        {
            // Si el formulario ya no está vivo, ignorar
            if (!this.IsHandleCreated || this.IsDisposed) return;

            if (_procesando) return;

            this.Invoke(new Action(() =>
            {
                ProcesarTarjeta(uid);
            }));
        }

        private void ProcesarTarjeta(string uid)
        {
            _procesando = true;

            var (usuario, permitido, mensaje) = Program.Servicio.ValidarTarjetaPorUID(uid);

            if (!permitido)
            {
                MessageBox.Show(
                    string.IsNullOrEmpty(mensaje) ? "Tarjeta no reconocida." : mensaje,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                _procesando = false;
                return;
            }

            lblMensaje.Text = "Tarjeta reconocida";
            lblEstado.ForeColor = Color.Green;
            lblEstado.Text = "Procesando...";

            var timer = new Timer();
            timer.Interval = 800;
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                timer.Dispose();

                Program.Navegar(this, new FormVerificacion(usuario));
            };
            timer.Start();
        }

        private void FormInsertarTarjeta_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Servicio.TarjetaDetectada -= TarjetaDetectada;
            Program.Servicio.DetenerLector();
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            var (usuario, permitido, mensaje) = Program.Servicio.ValidarTarjeta(txtNumeroTarjeta.Text.Trim());

            if (!permitido)
            {
                mensaje = string.IsNullOrEmpty(mensaje) ? "Tarjeta inválida" : mensaje;
                MessageBox.Show(mensaje, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Program.Navegar(this, new FormVerificacion(usuario));
        }
    }
}
