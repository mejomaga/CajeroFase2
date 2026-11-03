using CajeroAutomatico.Negocio;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CajeroAutomatico.Presentacion
{
    public partial class FormMenuUsuario : Form
    {
        private Button btnTransferirOtroBanco;

        public FormMenuUsuario()
        {
            InitializeComponent();
            AgregarBotonInterbancario();
            this.Load += FormMenuUsuario_Load;
        }

        private void AgregarBotonInterbancario()
        {
            this.btnTransferirOtroBanco = new Button();
            this.btnTransferirOtroBanco.Anchor = AnchorStyles.None;
            this.btnTransferirOtroBanco.BackColor = Color.FromArgb(41, 128, 185);
            this.btnTransferirOtroBanco.FlatAppearance.BorderSize = 0;
            this.btnTransferirOtroBanco.FlatStyle = FlatStyle.Flat;
            this.btnTransferirOtroBanco.Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.btnTransferirOtroBanco.ForeColor = Color.White;
            this.btnTransferirOtroBanco.Location = new Point(90, 438);
            this.btnTransferirOtroBanco.Name = "btnTransferirOtroBanco";
            this.btnTransferirOtroBanco.Size = new Size(588, 49);
            this.btnTransferirOtroBanco.TabIndex = 21;
            this.btnTransferirOtroBanco.Text = "Transferir Dinero (Otros Bancos)";
            this.btnTransferirOtroBanco.UseVisualStyleBackColor = false;
            this.btnTransferirOtroBanco.Click += btnTransferirOtroBanco_Click;
            this.Controls.Add(this.btnTransferirOtroBanco);
        }

        private void btnTransferirOtroBanco_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormDepositoOtroBanco());
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
