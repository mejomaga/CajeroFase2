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
    public partial class FormRetiro : Form
    {
        private readonly Dictionary<int, NumericUpDown> controlesDesglose;
        // Almacena los dígitos ingresados mediante el teclado táctil
        private string _montoCadena = "0";
        public FormRetiro()
        {
            InitializeComponent();
            controlesDesglose = new Dictionary<int, NumericUpDown>
            {
                { 200, numeric200 },
                { 100, numeric100 },
                { 50,  numeric50 },
                { 20,  numeric20 },
                { 10,  numeric10 },
                { 5,   numeric5 },
                { 1,   numeric1 }
            };

            foreach (var numeric in controlesDesglose.Values)
            {
                numeric.ValueChanged += ControlNumeric_ValueChanged;
            }
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
            string digito = boton.Text;

            // Evitar acumular ceros a la izquierda
            if (_montoCadena == "0")
            {
                _montoCadena = digito;
            }
            else
            {
                _montoCadena += digito;
            }

            if (decimal.TryParse(_montoCadena, out decimal nuevoMonto))
            {
                // Limitar al valor máximo permitido por el control
                if (nuevoMonto <= numMonto.Maximum)
                {
                    numMonto.Value = nuevoMonto;
                }
                else
                {
                    _montoCadena = numMonto.Maximum.ToString();
                    numMonto.Value = numMonto.Maximum;
                }
            }

            ActualizarTotalDesglose();
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            if (_montoCadena.Length > 1)
            {
                _montoCadena = _montoCadena.Substring(0, _montoCadena.Length - 1);
            }
            else
            {
                _montoCadena = "0";
            }

            if (decimal.TryParse(_montoCadena, out decimal nuevoMonto))
            {
                numMonto.Value = nuevoMonto;
            }

            ActualizarTotalDesglose();
        }

        private void ControlNumeric_ValueChanged(object sender, EventArgs e)
        {
            ActualizarTotalDesglose();
        }
        private void ActualizarTotalDesglose()
        {
            var total = DenominacionesHelper.CalcularTotal(controlesDesglose);
            lblTotalDesglose.Text = $"Total del desglose: Q.{total:N2}";
            lblTotalDesglose.ForeColor = total == numMonto.Value ? Color.DarkGreen : Color.DarkRed;
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            var usuario = Sesion.UsuarioActual;
            var desglose = DenominacionesHelper.ObtenerDesglose(controlesDesglose);
            decimal montoRetiro = numMonto.Value;

            if (montoRetiro <= 0)
            {
                lblMensaje.ForeColor = Color.DarkRed;
                lblMensaje.Text = "Debe ingresar un monto mayor a Q.0.00";
                return;
            }

            var resultado = Program.Servicio.Retirar(usuario, montoRetiro, desglose);

            lblMensaje.ForeColor = resultado.Exito ? Color.DarkGreen : Color.DarkRed;
            lblMensaje.Text = resultado.Mensaje;

            if (resultado.Exito)
            {
                MessageBox.Show(resultado.Mensaje, "Resultado de la operación", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Program.Navegar(this, new FormRetireDinero(desglose, montoRetiro));

                _montoCadena = "0";
                numMonto.Value = 0;
                DenominacionesHelper.Limpiar(controlesDesglose);
                ActualizarTotalDesglose();
            }
            else
            {
                MessageBox.Show(resultado.Mensaje, "Resultado de la operación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Program.Navegar(this, new FormFinProceso());
            }
        }

        private void FormRetiro_Load(object sender, EventArgs e)
        {
            
        }

        private void numeric100_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numeric50_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numeric20_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numeric200_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numeric10_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numeric5_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numeric1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormFinProceso());
        }
    }
}
