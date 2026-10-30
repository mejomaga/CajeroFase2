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
    public partial class FormPIN : Form
    {
        private Usuario usuario;
        public FormPIN(Usuario usuario)
        {
            InitializeComponent();
            this.usuario = usuario;
            lblUsuario.Text = $"{usuario.Nombre}";
            AsignarEventosTeclado();
        }
        private void AsignarEventosTeclado()
        {
            btn0.Click += BotonNumero_Click;
            btn1.Click += BotonNumero_Click;
            btn2.Click += BotonNumero_Click;
            btn3.Click += BotonNumero_Click;
            btn4.Click += BotonNumero_Click;
            btn5.Click += BotonNumero_Click;
            btn6.Click += BotonNumero_Click;
            btn7.Click += BotonNumero_Click;
            btn8.Click += BotonNumero_Click;
            btn9.Click += BotonNumero_Click;
        }
        private void BotonNumero_Click(object sender, EventArgs e)
        {
            Button boton = (Button)sender;

            // Limitamos a un máximo de 4 dígitos para el PIN
            if (txtPin.Text.Length < 4)
            {
                txtPin.Text += boton.Text;
            }
        }

        private void btn1_Click(object sender, EventArgs e)
        {

        }

        private void btn2_Click(object sender, EventArgs e)
        {

        }

        private void btn3_Click(object sender, EventArgs e)
        {

        }

        private void btn4_Click(object sender, EventArgs e)
        {

        }

        private void btn5_Click(object sender, EventArgs e)
        {

        }

        private void btn6_Click(object sender, EventArgs e)
        {

        }

        private void btn7_Click(object sender, EventArgs e)
        {

        }

        private void btn8_Click(object sender, EventArgs e)
        {

        }

        private void btn9_Click(object sender, EventArgs e)
        {

        }

        private void btn0_Click(object sender, EventArgs e)
        {

        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            if (txtPin.Text.Length > 0)
            {
                txtPin.Text = txtPin.Text.Substring(0, txtPin.Text.Length - 1);
            }
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (txtPin.Text.Length != 4)
            {
                lblIntentos.Text = "El PIN debe tener 4 dígitos.";
                return;
            }

            var respuesta = Program.Servidor.Login(usuario.NumeroTarjeta, txtPin.Text);

            if (respuesta.Exito)
            {
                Sesion.UsuarioActual = usuario;
                Program.Navegar(this, new FormMenuUsuario());
                return;
            }

            if (respuesta.Booleano("Bloqueado"))
            {
                MessageBox.Show("Tarjeta bloqueada por demasiados intentos fallidos.\nContacte al administrador.",
                    "Tarjeta bloqueada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Program.Navegar(this, new FormInsertarTarjeta());
                return;
            }

            txtPin.Clear();
            var intentosRestantes = respuesta.Entero("IntentosRestantes");
            lblIntentos.Text = string.IsNullOrEmpty(respuesta.Mensaje)
                ? $"PIN incorrecto. Intentos restantes: {intentosRestantes}"
                : respuesta.Mensaje;
        }

        private void txtPin_TextChanged(object sender, EventArgs e)
        {

        }

        private void FormPIN_Load(object sender, EventArgs e)
        {
            VozService.Hablar("Ingrese su pin");
        }
    }
}
