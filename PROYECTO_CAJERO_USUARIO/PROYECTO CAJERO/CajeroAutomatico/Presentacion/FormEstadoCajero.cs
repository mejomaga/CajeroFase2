using System;
using System.Drawing;
using System.Windows.Forms;

namespace CajeroAutomatico.Presentacion
{
    public partial class FormEstadoCajero : Form
    {
        private bool yaAvanzo = false;

        private Timer _timerEstado;
        public FormEstadoCajero()
        {
            InitializeComponent();
            lblEstadoActual.Click += FormEstadoCajero_Click;
            this.Cursor = Cursors.Hand;
            lblEstadoActual.Cursor = Cursors.Hand;
            ActualizarEstado();

            _timerEstado = new Timer();
            _timerEstado.Interval = 3000; // Revisa cada 3 segundos
            _timerEstado.Tick += TimerEstado_Tick;
            _timerEstado.Start();
        }

        private void ActualizarEstado()
        {
            var respuesta = Program.Servidor.ObtenerEstadoCajero();
            bool activo = respuesta.Exito && string.Equals(respuesta.Texto("Estado"), "Activo", StringComparison.OrdinalIgnoreCase);

            lblEstadoActual.Text = activo ? "Cajero inactivo" : "Cajero activo";
            lblEstadoActual.ForeColor = activo ? Color.DarkGreen : Color.DarkGreen;
        }

        private void TimerEstado_Tick(object sender, EventArgs e)
        {
            if (yaAvanzo)
            {
                _timerEstado.Stop();
                return;
            }

            var respuesta = Program.Servidor.ObtenerEstadoCajero();
            bool activo = respuesta.Exito && string.Equals(respuesta.Texto("Estado"), "En línea", StringComparison.OrdinalIgnoreCase);

            if (activo)
            {
                // ¡El administrador lo habilitó! Avanzamos solos
                yaAvanzo = true;
                _timerEstado.Stop();
                this.Hide();
                FormInsertarTarjeta siguiente = new FormInsertarTarjeta();
                siguiente.FormClosed += (s, args) => this.Close();
                siguiente.Show();
            }
            else
            {
                ActualizarEstado(); // Si está bloqueado, se queda aquí
            }
        }

        private void FormEstadoCajero_Click(object sender, EventArgs e)
        {
            if (yaAvanzo) return;

            // 1. Verificamos el estado justo antes de dejar pasar al usuario
            var respuesta = Program.Servidor.ObtenerEstadoCajero();
            bool activo = respuesta.Exito && string.Equals(respuesta.Texto("Estado"), "Activo", StringComparison.OrdinalIgnoreCase);

            if (!activo)
            {
                // 2. Si está inactivo, bloqueamos el paso
                MessageBox.Show("Este cajero se encuentra temporalmente fuera de servicio.\nPor favor, acuda a otro cajero o intente más tarde.",
                                "Cajero Inhabilitado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // No hace nada más, se queda en la pantalla de inicio
            }

            // 3. Si está activo, procede normal
            yaAvanzo = true;
            this.Hide();
            FormInsertarTarjeta siguientePantalla = new FormInsertarTarjeta();
            siguientePantalla.FormClosed += (s, args) => this.Close();
            siguientePantalla.Show();
        }
    }
}
