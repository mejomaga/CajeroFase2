using System;
using System.Drawing;
using System.Windows.Forms;

namespace CajeroAutomatico.Presentacion
{
    public partial class FormEstadoCajero : Form
    {
        private bool yaAvanzo = false;
        public FormEstadoCajero()
        {
            InitializeComponent();
            lblEstadoActual.Click += FormEstadoCajero_Click;
            this.Cursor = Cursors.Hand;
            lblEstadoActual.Cursor = Cursors.Hand;
            ActualizarEstado();
        }

        private void ActualizarEstado()
        {
            var respuesta = Program.Servidor.ObtenerEstadoCajero();
            bool activo = respuesta.Exito && string.Equals(respuesta.Texto("Estado"), "Activo", StringComparison.OrdinalIgnoreCase);

            lblEstadoActual.Text = activo ? "Cajero activo" : "Cajero inactivo";
            lblEstadoActual.ForeColor = activo ? Color.DarkGreen : Color.DarkRed;
        }

        private void FormEstadoCajero_Click(object sender, EventArgs e)
        {
            if (yaAvanzo) return;
            yaAvanzo = true; 
            this.Hide();
            FormInsertarTarjeta siguientePantalla = new FormInsertarTarjeta();
            siguientePantalla.FormClosed += (s, args) => this.Close();
            siguientePantalla.Show();
        }
    }
}
