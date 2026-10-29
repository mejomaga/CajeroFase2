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
    public partial class FormMenuUsuario : Form
    {
        public FormMenuUsuario()
        {
            InitializeComponent();
        }

        private void btnCambiarPIN_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormCambiarPin());
        }

        private void btnRetiro_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormRetiro());
        }

        private void btnDeposito_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormDeposito());
        }

        private void btnUltimastransacciones_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormUltimasTransacciones());
        }

        private void btnVerSaldo_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormVerSaldo());
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            Sesion.CerrarSesion();
            Program.Navegar(this, new FormFinProceso());
        }

        private void FormMenuUsuario_Load(object sender, EventArgs e)
        {
            if (Sesion.UsuarioActual != null)
                lblBienvenida.Text = $"Bienvenido: {Sesion.UsuarioActual.Nombre}";
            else
                lblBienvenida.Text = "Bienvenido";
        }
    }
}
