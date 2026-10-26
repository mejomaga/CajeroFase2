using CajeroAutomatico.Entidades;
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
            var estado = Program.Servicio.ObtenerEstado();
            lblEstadoActual.Text = estado == EstadoCajero.Activo ? "Cajero activo" : "Cajero inactivo";
            lblEstadoActual.ForeColor = estado == EstadoCajero.Activo ? Color.DarkGreen : Color.DarkRed;
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
