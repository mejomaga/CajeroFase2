using CajeroAutomatico.Entidades;
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
            //this.Load += FormInsertarTarjeta_Load;
            this.FormClosing += FormInsertarTarjeta_FormClosing;
        }

        private void btnContinuar_Click(object sender, EventArgs e)
        {
            ValidarTarjetaYContinuar(txtNumeroTarjeta.Text.Trim());
        }

        private void ValidarTarjetaYContinuar(string numeroTarjeta)
        {
            if (numeroTarjeta.Length != 16)
            {
                MessageBox.Show("La tarjeta debe tener 16 dígitos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var usuario = new Usuario { NumeroTarjeta = numeroTarjeta, Nombre = "Cliente" };
            // Pasamos directo a la pantalla de PIN
            Program.Navegar(this, new FormPIN(usuario));
        }

        private void FormInsertarTarjeta_Load(object sender, EventArgs e)
        {
            VozService.Hablar("Bienvenido. Por favor, inserte su tarjeta o acérquela al lector.");
            
            // Mensaje visual
            lblMensaje.Text = "Inserte su tarjeta";
            lblEstado.Text = "";
            _procesando = false;

            // Suscribirse al evento del lector
            //Program.Servicio.TarjetaDetectada += TarjetaDetectada;

            // Empezar a escuchar automáticamente
            //Program.Servicio.IniciarLector();
            txtNumeroTarjeta.Focus();
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
            
        }

        private void FormInsertarTarjeta_FormClosing(object sender, FormClosingEventArgs e)
        {
           // Program.Servicio.TarjetaDetectada -= TarjetaDetectada;
           // Program.Servicio.DetenerLector();
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            ValidarTarjetaYContinuar(txtNumeroTarjeta.Text.Trim());
        }
    }
}
