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
    public partial class FormVerificacion : Form
    {
        private Usuario usuario;

        public FormVerificacion(Usuario usuario)
        {
            InitializeComponent();
            this.usuario = usuario; 
            lblUsuario.Text = $"{usuario.Nombre}";
        }

        private void btnPin_Click(object sender, EventArgs e)
        {
          Program.Navegar(this, new FormPIN(usuario));
        }

        private void btnHuella_Click(object sender, EventArgs e)
        {
          Program.Navegar(this, new FormHuellaDigital(usuario));
        }

        private void FormVerificacion_Load(object sender, EventArgs e)
        {
            VozService.Hablar("Elija alguna de las opciones");
            btnHuella.Visible = usuario.RequiereHuella;
            btnHuella.Enabled = usuario.RequiereHuella;
        }
    }
}
