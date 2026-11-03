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
        private Timer _timerEstado;

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

            // 1. Preguntamos al servidor si la tarjeta es válida
            var respuesta = Program.Servidor.ValidarTarjeta(numeroTarjeta);

            // 2. Si no es válida, mostramos el error y detenemos el proceso
            if (!respuesta.Exito)
            {
                MessageBox.Show(respuesta.Mensaje ?? "Tarjeta no válida.",
                    "Verificación fallida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNumeroTarjeta.Clear();
                txtNumeroTarjeta.Focus();
                return;
            }

            // 3. Si todo está bien, pasamos a la pantalla del PIN
            var usuario = new Usuario { NumeroTarjeta = numeroTarjeta, Nombre = "Cliente" };
            Program.Navegar(this, new FormPIN(usuario));
        }

        private void FormInsertarTarjeta_Load(object sender, EventArgs e)
        {
            VozService.Hablar("Bienvenido. Por favor, Ingrese el número de su tarjeta.");

            // Mensaje visual
            //lblMensaje.Text = "Inserte su tarjeta";
            lblEstado.Text = "";
            _procesando = false;

            txtNumeroTarjeta.Focus();

            // AQUÍ USAMOS EL TIMER EN LUGAR DE OnComandoRecibido
            _timerEstado = new Timer();
            _timerEstado.Interval = 5000; // Consulta cada 5 segundos
            _timerEstado.Tick += (s, ev) =>
            {
                // El cajero le pregunta al servidor si sigue activo
                bool habilitado = Program.Servidor.VerificarConexion();

                if (!habilitado)
                {
                    _timerEstado.Stop(); // Deja de preguntar para no saturar
                    Program.Navegar(this, new FormErrorConexion("Cajero Inhabilitado Temporalmente por el Administrador."));
                }
            };
            _timerEstado.Start();
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
