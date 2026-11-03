using System;
using System.Drawing;
using System.Windows.Forms;

namespace CajeroAutomatico.Presentacion
{
    public partial class FormEstadoCajero : Form
    {
        private bool yaAvanzo = false;
        private Timer timerEstado;

        public FormEstadoCajero()
        {
            InitializeComponent();

            lblEstadoActual.Click += FormEstadoCajero_Click;
            this.Click += FormEstadoCajero_Click;

            this.Cursor = Cursors.Hand;
            lblEstadoActual.Cursor = Cursors.Hand;

            timerEstado = new Timer();
            timerEstado.Interval = 5000;
            timerEstado.Tick += TimerEstado_Tick;

            this.Load += FormEstadoCajero_Load;
        }

        private void FormEstadoCajero_Load(object sender, EventArgs e)
        {
            ActualizarEstado();
            timerEstado.Start();
        }

        private void TimerEstado_Tick(object sender, EventArgs e)
        {
            if (!yaAvanzo)
            {
                ActualizarEstado();
            }
        }

        private void ActualizarEstado()
        {
            var respuesta = Program.Servidor.ObtenerEstadoCajero();

            bool activo = false;
            if (respuesta != null && respuesta.Exito && respuesta.Datos != null && respuesta.Datos.ContainsKey("Estado"))
            {
                string estadoStr = respuesta.Datos["Estado"]?.ToString() ?? "";

                // Solo será verdadero si el servidor envía EXPLÍCITAMENTE la palabra "Activo"
                // (Ignora "Inactivo", "Desconectado" o "En línea Inactivo")
                activo = estadoStr.IndexOf("Activo", StringComparison.OrdinalIgnoreCase) >= 0 &&
                         estadoStr.IndexOf("Inactivo", StringComparison.OrdinalIgnoreCase) < 0;
            }

            if (activo)
            {
                lblEstadoActual.Text = "Cajero Activo\n(Toque la pantalla para iniciar)";
                lblEstadoActual.ForeColor = Color.DarkGreen;
            }
            else
            {
                lblEstadoActual.Text = "Cajero Inactivo\nTemporalmente fuera de servicio";
                lblEstadoActual.ForeColor = Color.DarkRed;
            }
        }

        private void FormEstadoCajero_Click(object sender, EventArgs e)
        {
            if (yaAvanzo) return;

            var respuesta = Program.Servidor.ObtenerEstadoCajero();

            bool activo = false;
            if (respuesta != null && respuesta.Exito && respuesta.Datos != null && respuesta.Datos.ContainsKey("Estado"))
            {
                string estadoStr = respuesta.Datos["Estado"]?.ToString() ?? "";
                activo = estadoStr.IndexOf("Activo", StringComparison.OrdinalIgnoreCase) >= 0 &&
                         estadoStr.IndexOf("Inactivo", StringComparison.OrdinalIgnoreCase) < 0;
            }

            if (!activo)
            {
                MessageBox.Show("Este cajero se encuentra temporalmente fuera de servicio (Inactivo).\nPor favor, acuda a otro cajero o intente más tarde.",
                                "Cajero Inactivo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            yaAvanzo = true;
            timerEstado.Stop();

            this.Hide();
            FormInsertarTarjeta siguientePantalla = new FormInsertarTarjeta();
            siguientePantalla.FormClosed += (s, args) => this.Close();
            siguientePantalla.Show();
        }
    }
}