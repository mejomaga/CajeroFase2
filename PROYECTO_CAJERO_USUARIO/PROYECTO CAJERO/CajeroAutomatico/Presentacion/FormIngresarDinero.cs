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
    public partial class FormIngresarDinero : Form
    {
        private readonly Dictionary<int, int> _desglose;
        private readonly decimal _totalDepositado;
        public FormIngresarDinero(Dictionary<int, int> desglose, decimal totalDepositado)
        {
            InitializeComponent();

            _desglose = desglose;
            _totalDepositado = totalDepositado;

            timerIntroduzcaDinero.Interval = 5000; // 5 segundos
        }

        private void FormIntroduzcaDinero_Load(object sender, EventArgs e)
        {
            timerIntroduzcaDinero.Start();

            // Disparar la impresión silenciosa al mostrar la animación de la ranura
            ImprimirComprobanteDeposito();

            VozService.Hablar("Ingrese el dinero");
        }

        private void timerIntroduzcaDinero_Tick(object sender, EventArgs e)
        {
            timerIntroduzcaDinero.Stop();
            timerIntroduzcaDinero.Enabled = false;
            timerIntroduzcaDinero.Tick -= timerIntroduzcaDinero_Tick;
            timerIntroduzcaDinero.Dispose();

            // Ojo: Program.Navegar suele encargarse de ocultar/cerrar el form actual.
            // Si Program.Navegar ya hace this.Hide() o this.Close(), no necesitas this.Close() abajo.
            Program.Navegar(this, new FormFinProceso());
        }

        private void ImprimirComprobanteDeposito()
        {
            // Si no hay datos (por ejemplo, prueba de vista sin parámetros), no imprime
            if (_desglose == null || _totalDepositado <= 0) return;

            try
            {
                using (PrintDocument doc = new PrintDocument())
                {
                    doc.PrintController = new StandardPrintController();
                    doc.DefaultPageSettings.PaperSize = new PaperSize("Ticket", 300, 600);
                    doc.PrintPage += ImprimirVoucher_PrintPage;
                    doc.Print();
                }
            }
            catch (Exception ex)
            {
                // En modo kiosco puedes registrar el log o mostrar la advertencia
                MessageBox.Show($"Error al imprimir voucher: {ex.Message}", "Impresión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                // Datos de cuenta
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

                // Sección Depósito
                g.DrawString("      COMPROBANTE DE DEPOSITO       ", fontNegrita, brocha, x, y);
                y += interlineado + 4;

                string cabecera = string.Format("{0,-12}{1,-8}{2,14}", "BILLETE", "CANT.", "SUBTOTAL");
                g.DrawString(cabecera, fontNegrita, brocha, x, y);
                y += interlineado;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Desglose de billetes recibidos
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

                // Total depositado
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;
                string totalLinea = string.Format("{0,-18}{1,16}", "TOTAL DEPOSITADO:", $"Q.{_totalDepositado:N2}");
                g.DrawString(totalLinea, fontNegrita, brocha, x, y);
                y += interlineado + 6;

                // Pie
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("   ¡Deposito acreditado con exito!  ", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString(" Guarde este comprobante como       ", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("      respaldo de su operacion.     ", fontCuerpo, brocha, x, y);
            }
        }
    }
}
