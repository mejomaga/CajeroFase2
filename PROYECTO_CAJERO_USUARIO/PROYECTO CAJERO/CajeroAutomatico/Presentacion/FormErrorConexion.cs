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
            // En lugar de congelar la pantalla verificando aquí, 
            // regresamos al FormCargando para que el usuario vea la animación de la barra de nuevo
            Program.Navegar(this, new FormCargando());
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            // Cierra este formulario. Como en FormCargando estamos escuchando este cierre, 
            // se cerrará toda la aplicación de forma limpia.
            this.Close();
        }
    }
}