using CajeroAutomatico.Negocio;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CajeroAutomatico.Presentacion
{
    public partial class FormToken : Form
    {
        private string tokenGenerado;
        private int segundosRestantes = 120;

        private readonly TipoOperacion tipoOperacion;
        private readonly Dictionary<int, int> desglose;
        private readonly decimal monto;
        private readonly string _token;

        public enum TipoOperacion { RETIRO, DEPOSITO }

        public FormToken(TipoOperacion tipoOperacion, decimal monto, Dictionary<int, int> desglose, string token)
        {
            InitializeComponent();
            this.tipoOperacion = tipoOperacion;
            this.monto = monto;
            this.desglose = desglose ?? new Dictionary<int, int>();
            this._token = token;

            lblUsuario.Text = Sesion.UsuarioActual?.Nombre ?? "Cliente";
            AsignarEventosTeclado();
        }

        private void FormToken_Load(object sender, EventArgs e)
        {
            try
            {
                string accion = tipoOperacion == TipoOperacion.RETIRO ? "retiro" : "depósito";

                lblUsuario.Text = Sesion.UsuarioActual?.Nombre ?? "Cliente";
                lblInstruccion.Text = $"Confirme su {accion} de Q.{monto:N2} con el token:";

                GenerarToken();   // esta es la única fuente del token

                timerExpiracion.Interval = 1000;
                timerExpiracion.Tick += TimerExpiracion_Tick;
                timerExpiracion.Start();

                VozService.Hablar($"Confirme su {accion} ingresando el token generado");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el formulario: " + ex.Message);
            }
        }

        private void GenerarToken()
        {
            var respuesta = Program.Servidor.GenerarTokenOperacion();

            if (!respuesta.Exito)
            {
                lblToken.Text = "------";
                MessageBox.Show(respuesta.Mensaje ?? "No se pudo generar el token.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Program.Navegar(this, new FormFinProceso());
                return;
            }

            tokenGenerado = respuesta.Texto("TokenOperacion");
            lblToken.Text = tokenGenerado;
            lblMensaje.Text = "Ingrese el token mostrado para autorizar la operación.";
            lblTiempo.Text = "Tiempo restante: 02:00";
            segundosRestantes = 120;
        }

        private void TimerExpiracion_Tick(object sender, EventArgs e)
        {
            segundosRestantes--;
            if (segundosRestantes <= 0)
            {
                timerExpiracion.Stop();
                lblTiempo.Text = "Tiempo restante: 00:00";
                lblToken.Text = "------";
                MessageBox.Show("El token ha expirado. Se generará uno nuevo.",
                    "Token expirado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                GenerarToken();
                timerExpiracion.Start();
                return;
            }

            int min = segundosRestantes / 60;
            int seg = segundosRestantes % 60;
            lblTiempo.Text = $"Tiempo restante: {min:00}:{seg:00}";
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
            if (txtToken.Text.Length < 6) txtToken.Text += boton.Text;
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            if (txtToken.Text.Length > 0)
                txtToken.Text = txtToken.Text.Substring(0, txtToken.Text.Length - 1);
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (txtToken.Text.Length != 6)
            {
                lblMensaje.Text = "El token debe tener 6 dígitos.";
                return;
            }

            var respuesta = Program.Servidor.ValidarTokenOperacion(txtToken.Text);

            if (!respuesta.Exito)
            {
                txtToken.Clear();
                lblMensaje.Text = respuesta.Mensaje ?? "Token incorrecto o expirado.";
                VozService.Hablar("Token incorrecto");
                return;
            }

            timerExpiracion.Stop();
            VozService.Hablar("Token válido, procesando transacción");

            EjecutarTransaccion();
        }

        private void EjecutarTransaccion()
        {
            try
            {
                var resultado = tipoOperacion == TipoOperacion.RETIRO
                    ? Program.Servidor.Retirar(monto, desglose)
                    : Program.Servidor.Depositar(desglose);

                if (resultado.Exito)
                {
                    MessageBox.Show(resultado.Mensaje, "Resultado",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (tipoOperacion == TipoOperacion.RETIRO)
                        Program.Navegar(this, new FormRetireDinero(desglose, monto));
                    else
                        Program.Navegar(this, new FormIngresarDinero(desglose, monto));
                }
                else
                {
                    MessageBox.Show(resultado.Mensaje, "Resultado",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Program.Navegar(this, new FormFinProceso());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar la transacción: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Program.Navegar(this, new FormFinProceso());
            }
        }

        private void btnReenviar_Click(object sender, EventArgs e)
        {
            timerExpiracion.Stop();
            txtToken.Clear();
            GenerarToken();
            timerExpiracion.Start();
        }

        private void btnRegresar_Click(object sender, EventArgs e)
        {
            timerExpiracion.Stop();

            if (tipoOperacion == TipoOperacion.RETIRO)
                Program.Navegar(this, new FormRetiro());
            else
                Program.Navegar(this, new FormDeposito());
        }

        private void lblTitulo_Click(object sender, EventArgs e)
        {

        }
    }
}