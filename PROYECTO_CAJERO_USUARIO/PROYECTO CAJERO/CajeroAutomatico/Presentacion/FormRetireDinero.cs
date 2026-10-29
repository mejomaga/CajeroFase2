using CajeroAutomatico.Negocio;
using CajeroAutomatico.Presentacion;
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

namespace CajeroAutomatico
{
    public partial class FormRetireDinero : Form
    {
        private readonly Dictionary<int, int> _desglose;
        private readonly decimal _montoRetirado;
        public FormRetireDinero(Dictionary<int, int> desglose, decimal montoRetirado)
        {
            InitializeComponent();

            _desglose = desglose;
            _montoRetirado = montoRetirado;

            timerRetiro.Interval = 5000; // 5 segundos
        }

        private void FormRetireDinero_Load(object sender, EventArgs e)
        {
            timerRetiro.Start();
            ImprimirComprobanteRetiro();

            VozService.Hablar("Por favor, retire su dinero.");
        }
        private void timerRetiro_Tick(object sender, EventArgs e)
        {
            timerRetiro.Stop();
            timerRetiro.Enabled = false;
            timerRetiro.Tick -= timerRetiro_Tick;
            timerRetiro.Dispose();

            Program.Navegar(this, new FormFinProceso());
        }

        private void ImprimirComprobanteRetiro()
        {
            if (_desglose == null || _montoRetirado <= 0) return;

            try
            {
                using (PrintDocument doc = new PrintDocument())
                {
                    // Impresión silenciosa sin ventana emergente
                    doc.PrintController = new StandardPrintController();
                    doc.DefaultPageSettings.PaperSize = new PaperSize("Ticket", 300, 600);

                    doc.PrintPage += ImprimirVoucher_PrintPage;
                    doc.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir voucher de retiro: {ex.Message}", "Error de Impresión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                // Encabezado
                g.DrawString("      CAJERO AUTOMATICO     ", fontTitulo, brocha, x, y);
                y += interlineado + 6;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Metadatos
                g.DrawString($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("Cajero: #01-AGENCIA", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Información del usuario
                var u = Sesion.UsuarioActual;
                string nombre = u?.Nombre ?? "CLIENTE";
                string tarjeta = u?.NumeroTarjeta != null && u.NumeroTarjeta.Length >= 4
                    ? $"**** **** **** {u.NumeroTarjeta.Substring(u.NumeroTarjeta.Length - 4)}"
                    : "**** **** **** ****";

                g.DrawString($"Usuario: {nombre}", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString($"Tarjeta: {tarjeta}", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Título de la operación
                g.DrawString("       COMPROBANTE DE RETIRO        ", fontNegrita, brocha, x, y);
                y += interlineado + 4;

                // Encabezado de la tabla de dispensado
                string cabecera = string.Format("{0,-12}{1,-8}{2,14}", "BILLETE", "CANT.", "SUBTOTAL");
                g.DrawString(cabecera, fontNegrita, brocha, x, y);
                y += interlineado;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Billetes entregados (de mayor a menor valor)
                foreach (var item in _desglose.OrderByDescending(d => d.Key))
                {
                    if (item.Value > 0)
                    {
                        decimal subtotal = item.Key * item.Value;
                        string linea = string.Format("{0,-12}{1,-8}{2,14}", $"Q.{item.Key}", item.Value, $"Q.{subtotal:N2}");
                        g.DrawString(linea, fontCuerpo, brocha, x, y);
                        y += interlineado;
                    }
                }

                // Monto total retirado
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;
                string totalLinea = string.Format("{0,-18}{1,16}", "TOTAL RETIRADO:", $"Q.{_montoRetirado:N2}");
                g.DrawString(totalLinea, fontNegrita, brocha, x, y);
                y += interlineado + 6;

                // Pie de página
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("    ¡Retiro procesado con exito!    ", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString(" No olvide tomar su dinero y tarjeta", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("      respaldo de su operacion.     ", fontCuerpo, brocha, x, y);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
