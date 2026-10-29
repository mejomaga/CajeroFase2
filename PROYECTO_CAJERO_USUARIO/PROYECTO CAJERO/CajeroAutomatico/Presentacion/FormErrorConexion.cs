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
    public partial class FormErrorConexion : Form
    {
        public FormErrorConexion(string mensaje)
        {
            InitializeComponent();
            lblMensaje.Text = mensaje;

        }

        private void btnReintentar_Click(object sender, EventArgs e)
        {
            bool conectadoAlServidor = Program.Servidor.VerificarConexion();

            if (conectadoAlServidor)
                Program.Navegar(this, new FormEstadoCajero());
            else
                lblMensaje.Text = "Sigue sin haber conexión con el servidor. Verifique la red e intente de nuevo.";
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
