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
        }

        // Si tienes botón de borrar/retroceso en tu diseño (btnBorrar o btnLimpiar)
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
            // Validar que todos los campos tengan exactamente 4 dígitos
            if (txtPinActual.Text.Length != 4 || txtPinNuevo.Text.Length != 4 || txtConfirmarPinNuevo.Text.Length != 4)
            {
                lblMensaje.ForeColor = Color.DarkRed;
                lblMensaje.Text = "Todos los campos de PIN deben contener 4 dígitos.";
                return;
            }

            var usuario = Sesion.UsuarioActual;
            var resultado = Program.Servicio.CambiarPin(usuario, txtPinActual.Text, txtPinNuevo.Text, txtConfirmarPinNuevo.Text);

            lblMensaje.ForeColor = resultado.Exito ? Color.DarkGreen : Color.DarkRed;
            lblMensaje.Text = resultado.Mensaje;

            if (resultado.Exito)
            {
                txtPinActual.Clear();
                txtPinNuevo.Clear();
                txtConfirmarPinNuevo.Clear();
                _cajaActiva = txtPinActual;

                MessageBox.Show(resultado.Mensaje, "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Program.Navegar(this, new FormFinProceso());
            }
            else
            {
                MessageBox.Show(resultado.Mensaje, "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Si falló, limpiar solo los nuevos para que el usuario reintente sin sacarlo de la pantalla
                txtPinNuevo.Clear();
                txtConfirmarPinNuevo.Clear();
                txtPinNuevo.Focus();
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormFinProceso());
        }
    }
}
