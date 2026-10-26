using CajeroAutomatico.Entidades;
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
    public partial class FormModificarLimite : Form
    {
        private Usuario usuarioEncontrado;
        // Control actualmente enfocado para recibir las pulsaciones del teclado táctil
        private Control _controlActivo;
        private string _cadenaLimite = "0";
        public FormModificarLimite()
        {
            InitializeComponent();

            _controlActivo = txtTarjetaActual;

            AsignarEventosFoco();
            AsignarEventosTeclado();
        }

        private void AsignarEventosFoco()
        {
            txtTarjetaActual.Enter += (s, e) => _controlActivo = txtTarjetaActual;

            numNuevoLimite.Enter += (s, e) =>
            {
                _controlActivo = numNuevoLimite;
                _cadenaLimite = numNuevoLimite.Value.ToString("0");
            };
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
            if (_controlActivo == null || !_controlActivo.Enabled) return;

            Button boton = (Button)sender;
            string digito = boton.Text;

            // Caso 1: Escribiendo el número de tarjeta (TextBox)
            if (_controlActivo == txtTarjetaActual)
            {
                if (txtTarjetaActual.Text.Length < 16)
                {
                    txtTarjetaActual.Text += digito;
                }
            }
            // Caso 2: Escribiendo el nuevo límite diario (NumericUpDown)
            else if (_controlActivo == numNuevoLimite)
            {
                if (_cadenaLimite == "0")
                {
                    _cadenaLimite = digito;
                }
                else
                {
                    _cadenaLimite += digito;
                }

                if (decimal.TryParse(_cadenaLimite, out decimal nuevoValor))
                {
                    if (nuevoValor <= numNuevoLimite.Maximum)
                    {
                        numNuevoLimite.Value = nuevoValor;
                    }
                    else
                    {
                        _cadenaLimite = numNuevoLimite.Maximum.ToString();
                        numNuevoLimite.Value = numNuevoLimite.Maximum;
                    }
                }
            }
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            if (_controlActivo == null || !_controlActivo.Enabled) return;

            // Borrado en el campo de tarjeta
            if (_controlActivo == txtTarjetaActual)
            {
                if (txtTarjetaActual.Text.Length > 0)
                {
                    txtTarjetaActual.Text = txtTarjetaActual.Text.Substring(0, txtTarjetaActual.Text.Length - 1);
                }
            }
            // Borrado en el campo de límite numérico
            else if (_controlActivo == numNuevoLimite)
            {
                if (_cadenaLimite.Length > 1)
                {
                    _cadenaLimite = _cadenaLimite.Substring(0, _cadenaLimite.Length - 1);
                }
                else
                {
                    _cadenaLimite = "0";
                }

                if (decimal.TryParse(_cadenaLimite, out decimal nuevoValor))
                {
                    numNuevoLimite.Value = nuevoValor;
                }
            }
        }
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            usuarioEncontrado = Program.Servicio.BuscarPorTarjeta(txtTarjetaActual.Text.Trim());

            if (usuarioEncontrado == null)
            {
                lblNombreUsuario.Text = "No se encontró ningún usuario con esa tarjeta.";
                lblLimiteActual.Text = "";
                numNuevoLimite.Enabled = false;
                btnGuardar.Enabled = false;
                _controlActivo = txtTarjetaActual;
                return;
            }

            lblNombreUsuario.Text = $"Usuario: {usuarioEncontrado.Nombre}";
            lblLimiteActual.Text = $"Límite actual: Q.{usuarioEncontrado.LimiteDiario:N2}";
            numNuevoLimite.Value = usuarioEncontrado.LimiteDiario;
            numNuevoLimite.Enabled = true;
            btnGuardar.Enabled = true;

            // Enfocar automáticamente el NumericUpDown para ingresar el nuevo límite
            numNuevoLimite.Focus();
            _controlActivo = numNuevoLimite;
            _cadenaLimite = usuarioEncontrado.LimiteDiario.ToString("0");
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (usuarioEncontrado == null) return;

            var resultado = Program.Servicio.ModificarLimite(usuarioEncontrado, numNuevoLimite.Value);

            lblMensaje.ForeColor = resultado.Exito ? Color.DarkGreen : Color.DarkRed;
            lblMensaje.Text = resultado.Mensaje;

            if (resultado.Exito)
            {
                lblLimiteActual.Text = $"Límite actual: Q.{usuarioEncontrado.LimiteDiario:N2}";
                MessageBox.Show("Límite modificado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Program.Navegar(this, new FormMenuAdmin());
            }
            else
            {
                MessageBox.Show("No se pudo modificar el límite. Intente nuevamente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lblMensaje_Click(object sender, EventArgs e)
        {

        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormMenuAdmin());
        }
    }
}
