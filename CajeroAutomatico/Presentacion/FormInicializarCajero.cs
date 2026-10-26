using CajeroAutomatico.Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CajeroAutomatico.Presentacion
{
    public partial class FormInicializarCajero : Form
    {
        private Dictionary<int, NumericUpDown> controles;

        // Diccionario para vincular cada denominación con su label de billetes disponibles
        private Dictionary<int, Label> labelsDisponibles;
        // Variables de respaldo para la impresión del voucher
        private Dictionary<int, int> _desgloseImpresion;
        private decimal _totalInicializado;

        // Rastreo táctil del control activo
        private NumericUpDown _controlActivo;
        private string _cantidadCadena = "0";
        public FormInicializarCajero()
        {
            InitializeComponent();
            controles = new Dictionary<int, NumericUpDown>
            {
                { 200, numeric200 },
                { 100, numeric100 },
                { 50,  numeric50 },
                { 20,  numeric20 },
                { 10,  numeric10 },
                { 5,   numeric5 },
                { 1,   numeric1 }
            };
            // Mapeo directo de las etiquetas existentes en el diseño
            labelsDisponibles = new Dictionary<int, Label>
            {
                { 200, lbl200Disponible },
                { 100, lbl100Disponible },
                { 50,  lbl50Disponible },
                { 20,  lbl20Disponible },
                { 10,  lbl10Disponible },
                { 5,   lbl5Disponible },
                { 1,   lbl1Disponible }
            };
            foreach (var numeric in controles.Values)
            {
                numeric.ValueChanged += ControlNumeric_ValueChanged;
            }

            // Seleccionar por defecto el de mayor denominación
            _controlActivo = numeric200;

            AsignarEventosControles();
            AsignarEventosTeclado();

            // Cargar existencias actuales en los labels
            CargarBilletesDisponibles();
        }

        private void CargarBilletesDisponibles()
        {
            try
            {
                var desgloseActual = Program.Servicio.ObtenerDesgloseActual();
                decimal totalEnBoveda = 0;

                foreach (var item in labelsDisponibles)
                {
                    int denominacion = item.Key;
                    Label lbl = item.Value;

                    if (lbl == null) continue;

                    if (desgloseActual != null && desgloseActual.TryGetValue(denominacion, out int cantidad))
                    {
                        lbl.Text = $"Disponibles: {cantidad}";
                        totalEnBoveda += (decimal)denominacion * cantidad;
                    }
                    else
                    {
                        lbl.Text = "Disponibles: 0";
                    }
                }

                if (lblTotalActual != null)
                {
                    lblTotalActual.Text = $"Total en Bóveda: Q.{totalEnBoveda:N2}";
                }
            }
            catch
            {
                foreach (var lbl in labelsDisponibles.Values)
                {
                    if (lbl != null) lbl.Text = "Disponibles: 0";
                }

                if (lblTotalActual != null)
                {
                    lblTotalActual.Text = "Total en Bóveda: Q.0.00";
                }
            }

        }

        private void AsignarEventosControles()
        {
            foreach (var numeric in controles.Values)
            {
                // Al tocar el control en pantalla, se convierte en el destino de los botones numéricos
                numeric.Enter += (s, e) =>
                {
                    _controlActivo = numeric;
                    _cantidadCadena = numeric.Value.ToString();
                };

                numeric.ValueChanged += ControlNumeric_ValueChanged;
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
        }

        private void BotonNumero_Click(object sender, EventArgs e)
        {
            if (_controlActivo == null) return;

            Button boton = (Button)sender;
            string digito = boton.Text;

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
        private void ControlNumeric_ValueChanged(object sender, EventArgs e)
        {
            ActualizarTotal();
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnInicializar_Click(object sender, EventArgs e)
        {
            var total = DenominacionesHelper.CalcularTotal(controles);

            if (total <= 0)
            {
                lblMensaje.ForeColor = Color.DarkRed;
                lblMensaje.Text = "Debe cargar al menos un billete para inicializar la bóveda.";
                return;
            }

            var desglose = DenominacionesHelper.ObtenerDesglose(controles);
            var resultado = Program.Servicio.InicializarCajero(desglose);

            lblMensaje.ForeColor = resultado.Exito ? Color.DarkGreen : Color.DarkRed;
            lblMensaje.Text = resultado.Mensaje;

            if (resultado.Exito)
            {
                // Guardar respaldo de los datos antes de vaciar los campos
                _desgloseImpresion = new Dictionary<int, int>(desglose);
                _totalInicializado = total;

                // Imprimir comprobante de apertura de bóveda/cajero
                ImprimirComprobanteApertura();

                MessageBox.Show("Cajero inicializado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DenominacionesHelper.Limpiar(controles);
                _cantidadCadena = "0";

                // Actualizar las etiquetas con los nuevos billetes cargados
                CargarBilletesDisponibles();

                Program.Navegar(this, new FormMenuAdmin());
            }
            else
            {
                MessageBox.Show(resultado.Mensaje, "Error al inicializar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ActualizarTotal()
        {
            var total = DenominacionesHelper.CalcularTotal(controles);
            lblTotal.Text = $"Total: Q.{total:N2}";
            lblTotal.ForeColor = total > CajeroService.MontoMaximoInicializacion ? Color.DarkRed : Color.Green;
        }

        private void ImprimirComprobanteApertura()
        {
            if (_desgloseImpresion == null || _totalInicializado <= 0) return;

            try
            {
                using (PrintDocument doc = new PrintDocument())
                {
                    // Impresión en segundo plano sin cuadros emergentes
                    doc.PrintController = new StandardPrintController();
                    doc.DefaultPageSettings.PaperSize = new PaperSize("Ticket", 300, 600);

                    doc.PrintPage += ImprimirVoucher_PrintPage;
                    doc.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir comprobante de inicialización: {ex.Message}", "Error de Impresión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ImprimirVoucher_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            using (Font fontTitulo = new Font("Courier New", 12, FontStyle.Bold))
            using (Font fontCuerpo = new Font("Courier New", 8, FontStyle.Regular))
            using (Font fontNegrita = new Font("Courier New", 8, FontStyle.Bold))
            using (SolidBrush brocha = new SolidBrush(Color.Black))
            {
                int x = 12;
                int y = 20;
                int interlineado = 16;

                // Encabezado principal
                g.DrawString("      CAJERO AUTOMATICO     ", fontTitulo, brocha, x, y);
                y += interlineado + 4;
                g.DrawString("   APERTURA E INICIALIZACION    ", fontNegrita, brocha, x, y);
                y += interlineado + 2;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Metadatos operativos
                g.DrawString($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("Perfil: ADMINISTRADOR", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("Cajero: #01-AGENCIA", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("Estado: BÓVEDA REINICIALIZADA", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Desglose de carga base
                g.DrawString("SALDO INICIAL CARGADO EN BÓVEDA:", fontNegrita, brocha, x, y);
                y += interlineado + 4;

                string cabecera = string.Format("{0,-12}{1,-8}{2,14}", "DENOM.", "CANT.", "SUBTOTAL");
                g.DrawString(cabecera, fontNegrita, brocha, x, y);
                y += interlineado;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                foreach (var item in _desgloseImpresion.OrderByDescending(d => d.Key))
                {
                    if (item.Value > 0)
                    {
                        decimal subtotal = item.Key * item.Value;
                        string linea = string.Format("{0,-12}{1,-8}{2,14}", $"Q.{item.Key}", item.Value, $"Q.{subtotal:N2}");
                        g.DrawString(linea, fontCuerpo, brocha, x, y);
                        y += interlineado;
                    }
                }

                // Balance total inicial
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;
                string totalLinea = string.Format("{0,-18}{1,16}", "FONDO INICIAL:", $"Q.{_totalInicializado:N2}");
                g.DrawString(totalLinea, fontNegrita, brocha, x, y);
                y += interlineado + 6;

                // Pie de auditoría
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("  *** ARQUEO INICIAL DE CAJA ***    ", fontNegrita, brocha, x, y);
                y += interlineado;
                g.DrawString("  Conserve como acta de apertura    ", fontCuerpo, brocha, x, y);
            }
        }

        private void numeric200_ValueChanged(object sender, EventArgs e) => ActualizarTotal();
        private void numeric100_ValueChanged(object sender, EventArgs e) => ActualizarTotal();
        private void numeric50_ValueChanged(object sender, EventArgs e) => ActualizarTotal();
        private void numeric20_ValueChanged(object sender, EventArgs e) => ActualizarTotal();
        private void numeric10_ValueChanged(object sender, EventArgs e) => ActualizarTotal();
        private void numeric5_ValueChanged(object sender, EventArgs e) => ActualizarTotal();
        private void numeric1_ValueChanged(object sender, EventArgs e) => ActualizarTotal();


        private void btnVolver_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormMenuAdmin());
        }

        private void FormInicializarCajero_Load(object sender, EventArgs e)
        {
            CargarBilletesDisponibles();
        }
    }
}
