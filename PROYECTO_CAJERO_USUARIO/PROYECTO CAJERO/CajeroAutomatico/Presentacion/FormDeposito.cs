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
    public partial class FormDeposito : Form
    {
        private readonly Dictionary<int, NumericUpDown> controlesDesglose;

        // Rastrea cuál NumericUpDown está tocando el usuario en pantalla
        private NumericUpDown _controlActivo;
        private string _cantidadCadena = "0";

        // Límite máximo de depósito permitido por operación (Ej: Q. 10,000)
        private const decimal LIMITE_DEPOSITO = 10000m;

        public FormDeposito()
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

            // Por defecto, enfocar la denominación de mayor valor
            _controlActivo = numeric200;

            AsignarEventosControles();
            AsignarEventosTeclado();

            // Conectar el evento Load
            this.Load += FormDeposito_Load;
        }

        private void FormDeposito_Load(object sender, EventArgs e)
        {
            try { VozService.Hablar("Indique la cantidad de billetes que va a depositar"); } catch { }
            ActualizarTotal();
        }

        private void AsignarEventosControles()
        {
            foreach (var numeric in controlesDesglose.Values)
            {
                // Al tocar el control en la pantalla táctil, se vuelve el control activo
                numeric.Enter += (s, e) =>
                {
                    _controlActivo = numeric;
                    _cantidadCadena = numeric.Value.ToString();
                };

                numeric.ValueChanged += (s, e) => ActualizarTotal();
            }
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

            // Asegurarnos de que el botón de borrar esté conectado
            btnBorrar.Click += btnBorrar_Click;
        }

        private void BotonNumero_Click(object sender, EventArgs e)
        {
            if (_controlActivo == null) return;

            Button boton = (Button)sender;
            string digito = boton.Text;

            // Evitar acumular ceros iniciales
            if (_cantidadCadena == "0")
            {
                _cantidadCadena = digito;
            }
            else
            {
                _cantidadCadena += digito;
            }

            if (decimal.TryParse(_cantidadCadena, out decimal nuevaCantidad))
            {
                if (nuevaCantidad <= _controlActivo.Maximum)
                {
                    _controlActivo.Value = nuevaCantidad;
                }
                else
                {
                    _cantidadCadena = _controlActivo.Maximum.ToString();
                    _controlActivo.Value = _controlActivo.Maximum;
                }
            }

            ActualizarTotal();
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            if (_controlActivo == null) return;

            if (_cantidadCadena.Length > 1)
            {
                _cantidadCadena = _cantidadCadena.Substring(0, _cantidadCadena.Length - 1);
            }
            else
            {
                _cantidadCadena = "0";
            }

            if (decimal.TryParse(_cantidadCadena, out decimal nuevaCantidad))
            {
                _controlActivo.Value = nuevaCantidad;
            }

            ActualizarTotal();
        }

        private void ActualizarTotal()
        {
            var total = DenominacionesHelper.CalcularTotal(controlesDesglose);
            lblTotalDeposito.Text = $"Total a depositar: Q.{total:N2}";

            // Pintar de rojo el total si excede el límite permitido
            if (total > LIMITE_DEPOSITO)
            {
                lblTotalDeposito.ForeColor = Color.DarkRed;
            }
            else
            {
                lblTotalDeposito.ForeColor = total > 0 ? Color.DarkGreen : Color.Black;
            }
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            var desglose = DenominacionesHelper.ObtenerDesglose(controlesDesglose);
            var total = DenominacionesHelper.CalcularTotal(controlesDesglose);

            // 1. Validar que el usuario haya seleccionado billetes
            if (total <= 0)
            {
                MostrarError("Debe ingresar al menos un billete para realizar el depósito.");
                return;
            }

            // 2. Validar que no se exceda la capacidad máxima por depósito
            if (total > LIMITE_DEPOSITO)
            {
                MostrarError($"El monto máximo permitido por depósito es de Q.{LIMITE_DEPOSITO:N2}.");
                return;
            }

            // Si pasa las validaciones, vamos a la pantalla del token
            Program.Navegar(this, new FormToken(FormToken.TipoOperacion.DEPOSITO, total, desglose));

            // Limpiamos los campos (para cuando el usuario regrese)
            DenominacionesHelper.Limpiar(controlesDesglose);
            _cantidadCadena = "0";
            ActualizarTotal();
        }

        // Método centralizado para mostrar advertencias al usuario
        private void MostrarError(string mensaje)
        {
            lblMensaje.ForeColor = Color.DarkRed;
            lblMensaje.Text = mensaje;
            MessageBox.Show(mensaje, "Validación de Depósito", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            // Regresar al menú principal en lugar de cerrar toda la sesión
            Program.Navegar(this, new FormMenuUsuario());
        }
    }
}