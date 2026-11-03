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
    public partial class FormCambiarPin : Form
    {
        // Rastrear cuál de los 3 campos está seleccionado por el usuario en la pantalla táctil
        private TextBox _cajaActiva;

        public FormCambiarPin()
        {
            InitializeComponent();
            // Por defecto, iniciar escribiendo en el PIN actual
            _cajaActiva = txtPinActual;

            AsignarEventosFoco();
            AsignarEventosTeclado();
        }

        private void AsignarEventosFoco()
        {
            // Detectar cuando el usuario toca cualquiera de los 3 inputs
            txtPinActual.Enter += (s, e) => _cajaActiva = txtPinActual;
            txtPinNuevo.Enter += (s, e) => _cajaActiva = txtPinNuevo;
            txtConfirmarPinNuevo.Enter += (s, e) => _cajaActiva = txtConfirmarPinNuevo;
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

            btnBorrar.Click += btnBorrar_Click;
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            if (_cajaActiva != null && _cajaActiva.Text.Length > 0)
            {
                _cajaActiva.Text = _cajaActiva.Text.Substring(0, _cajaActiva.Text.Length - 1);
            }
        }

        private void BotonNumero_Click(object sender, EventArgs e)
        {
            if (_cajaActiva == null) return;

            Button boton = (Button)sender;

            // Restringir a 4 dígitos por campo
            if (_cajaActiva.Text.Length < 4)
            {
                _cajaActiva.Text += boton.Text;
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            // 1. Validar que todos los campos tengan exactamente 4 dígitos
            if (txtPinActual.Text.Length != 4 || txtPinNuevo.Text.Length != 4 || txtConfirmarPinNuevo.Text.Length != 4)
            {
                MostrarError("Todos los campos de PIN deben contener 4 dígitos.");
                return;
            }

            // 2. Validar que el PIN nuevo y la confirmación coincidan
            if (txtPinNuevo.Text != txtConfirmarPinNuevo.Text)
            {
                MostrarError("El PIN nuevo y su confirmación no coinciden.");
                txtPinNuevo.Clear();
                txtConfirmarPinNuevo.Clear();
                txtPinNuevo.Focus();
                return;
            }

            // 3. Validar que el nuevo PIN no sea igual al actual
            if (txtPinActual.Text == txtPinNuevo.Text)
            {
                MostrarError("El PIN nuevo no puede ser igual al PIN actual.");
                txtPinNuevo.Clear();
                txtConfirmarPinNuevo.Clear();
                txtPinNuevo.Focus();
                return;
            }

            // 4. Validar que el PIN actual ingresado sea el verdadero
            // Llamamos a Login de forma silenciosa para verificar la identidad
            var validacionPin = Program.Servidor.Login(Sesion.UsuarioActual.NumeroTarjeta, txtPinActual.Text);
            if (!validacionPin.Exito)
            {
                MostrarError("El PIN actual ingresado es incorrecto.");
                txtPinActual.Clear();
                txtPinActual.Focus();
                return;
            }

            // 5. Si superó todas las validaciones, cambiamos el PIN en la BD
            var resultado = Program.Servidor.CambiarPin(txtPinActual.Text, txtPinNuevo.Text);

            if (resultado.Exito)
            {
                lblMensaje.ForeColor = Color.DarkGreen;
                lblMensaje.Text = "PIN actualizado con éxito.";

                txtPinActual.Clear();
                txtPinNuevo.Clear();
                txtConfirmarPinNuevo.Clear();
                _cajaActiva = txtPinActual;

                MessageBox.Show("Su PIN ha sido actualizado de manera exitosa.", "Actualización Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Lo enviamos de regreso al menú para que siga operando o cierre sesión
                Program.Navegar(this, new FormFinProceso());
            }
            else
            {
                MostrarError(resultado.Mensaje);
                txtPinNuevo.Clear();
                txtConfirmarPinNuevo.Clear();
                txtPinNuevo.Focus();
            }
        }

        // Método de apoyo para limpiar la interfaz al mostrar un error
        private void MostrarError(string mensaje)
        {
            lblMensaje.ForeColor = Color.DarkRed;
            lblMensaje.Text = mensaje;
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormFinProceso());
        }
    }
}