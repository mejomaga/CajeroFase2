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
    public partial class FormMenuAdmin : Form
    {
        public FormMenuAdmin()
        {
            InitializeComponent();
        }

        private void btnCrearUsuario_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormCrearUsuario());
        }

        private void btnInicializarCajero_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormInicializarCajero());
        }

        private void btnAgregarEfectivo_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormAgregarEfectivo());
        }

        private void btnModificarTarjeta_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormModificarTarjeta());
        }

        private void btnModificarLimite_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormModificarLimite());
        }

        private void btnConsultaUsuario_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormConsultaUsuario());
        }

        private void btnControlUsuarios_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormControlUsuarios());
        }

        private void btnApagarCajero_Click(object sender, EventArgs e)
        {
            DialogResult resultado = MessageBox.Show("¿Está seguro de que desea desactivar y apagar el cajero automático? Esto cerrará la aplicación.","Confirmar Apagado",MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (resultado == DialogResult.Yes)
            {
                Program.Servicio.ApagarCajero();
                MessageBox.Show("Cajero desactivado con éxito. Cerrando el sistema...", "Sistema Apagado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            Sesion.CerrarSesion();
            Program.Navegar(this, new FormFinProceso());

        }

        private void btnModificarHuella_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormModificarHuella());
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void FormMenuAdmin_Load(object sender, EventArgs e)
        {
            btnModificarHuella.Visible = false;
        }
    }
}
