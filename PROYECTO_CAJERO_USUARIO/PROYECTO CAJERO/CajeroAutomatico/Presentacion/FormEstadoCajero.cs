using CajeroAutomatico.Comunicacion;
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
            timerEstado.Interval = 3000; // Revisa cada 3 segundos
            timerEstado.Tick += TimerEstado_Tick;

            this.Load += FormEstadoCajero_Load;
        }

        private void FormEstadoCajero_Load(object sender, EventArgs e)
        {
            ActualizarEstado();
            timerEstado.Start();
        }

        // El timer solo refresca el texto y el color; nunca cambia de pantalla.
        private void TimerEstado_Tick(object sender, EventArgs e)
        {
            if (yaAvanzo)
            {
                timerEstado.Stop();
                return;
            }

            ActualizarEstado();
        }

        private void ActualizarEstado()
        {
            var respuesta = Program.Servidor.ObtenerEstadoCajero();
            ActualizarEtiquetaEstado(EsActivo(respuesta));
        }

        private void ActualizarEtiquetaEstado(bool activo)
        {
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

        // Solo aquí se avanza: cuando el usuario toca la pantalla y el cajero está activo.
        private void FormEstadoCajero_Click(object sender, EventArgs e)
        {
            if (yaAvanzo) return;

            var respuesta = Program.Servidor.ObtenerEstadoCajero();
            bool activo = EsActivo(respuesta);

            ActualizarEtiquetaEstado(activo);

            if (!activo)
            {
                MessageBox.Show("Este cajero se encuentra temporalmente fuera de servicio (Inactivo).\nPor favor, acuda a otro cajero o intente más tarde.",
                                "Cajero Inactivo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Avanzar();
        }

        private void Avanzar()
        {
            if (yaAvanzo) return;
            yaAvanzo = true;

            timerEstado.Stop();
            timerEstado.Dispose();

            VigilanteEstado.Iniciar();
            Program.Navegar(this, new FormInsertarTarjeta());
        }

        private static bool EsActivo(RespuestaRed respuesta)
        {
            if (respuesta == null || !respuesta.Exito ||
                respuesta.Datos == null || !respuesta.Datos.ContainsKey("Estado"))
                return false;

            string estado = respuesta.Datos["Estado"]?.ToString()?.Trim() ?? "";
            return string.Equals(estado, "Activo", StringComparison.OrdinalIgnoreCase);
        }
    }
}