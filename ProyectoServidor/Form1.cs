using System;
using System.Drawing;
using System.Windows.Forms;
using ProyectoServidor.Comunicaciones;

namespace ProyectoServidor
{
    public partial class Form1 : Form
    {
        private ServidorCentral _servidor;

        public Form1()
        {
            InitializeComponent();
            _servidor = new ServidorCentral();

            // Nos suscribimos a un evento (que crearemos en el Paso 3) para recibir los mensajes
            _servidor.OnMensajeConsola += AgregarMensajeConsola;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Inicia la escucha en el puerto 5000 cuando abre la ventana
            _servidor.Iniciar(5000);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Detiene el servidor de forma limpia al cerrar
            _servidor?.Detener();
        }

        private void btnEncender_Click(object sender, EventArgs e)
        {
            _servidor.Iniciar(5000);

            lblEstado.Text = "Servidor Central - ESCUCHANDO (Puerto 5000)";
            lblEstado.ForeColor = Color.Green;
            btnEncender.Enabled = false;
            btnApagar.Enabled = true;
        }

        private void btnApagar_Click(object sender, EventArgs e)
        {
            _servidor.Detener();

            lblEstado.Text = "Servidor Central - APAGADO";
            lblEstado.ForeColor = Color.Red;
            btnEncender.Enabled = true;
            btnApagar.Enabled = false;
        }

        private void AgregarMensajeConsola(string mensaje)
        {
            if (txtConsola.InvokeRequired)
            {
                txtConsola.Invoke(new Action(() => AgregarMensajeConsola(mensaje)));
                return;
            }

            txtConsola.AppendText($"[{DateTime.Now:HH:mm:ss}] {mensaje}{Environment.NewLine}");
            txtConsola.SelectionStart = txtConsola.Text.Length;
            txtConsola.ScrollToCaret();
        }

        private void btnProbarBD_Click(object sender, EventArgs e)
        {
            var conexionBD = new ProyectoServidor.Datos.ConexionServidor();

            if (conexionBD.ProbarConexion(out string msj))
            {
                MessageBox.Show(msj, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(msj, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
