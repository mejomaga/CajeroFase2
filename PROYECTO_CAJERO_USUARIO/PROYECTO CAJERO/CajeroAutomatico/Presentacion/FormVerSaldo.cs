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
    public partial class FormVerSaldo : Form
    {
        private decimal _limiteMaximo;
        private decimal _disponibleHoy;
        public FormVerSaldo()
        {
            InitializeComponent();
            CargarSaldo();
        }

        private void FormVerSaldo_Load(object sender, EventArgs e)
        {
         
        }
        private void CargarSaldo()
        {
            var respuesta = Program.Servidor.ConsultarSaldo();

            if (!respuesta.Exito)
            {
                lblLimiteMaximo.Text = "No se pudo consultar el saldo.";
                lblDisponibleHoy.Text = respuesta.Mensaje;
                return;
            }

            _limiteMaximo = respuesta.Decimal("LimiteMaximo");
            _disponibleHoy = respuesta.Decimal("DisponibleHoy");

            lblLimiteMaximo.Text = $"Límite máximo de retiro: {_limiteMaximo:C}";
            lblDisponibleHoy.Text = $"Disponible para hoy: {_disponibleHoy:C}";
        }
        private void btnSalir_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormFinProceso());
        }

        private void btnVoucher_Click(object sender, EventArgs e)
        {
            try
            {
                using (PrintDocument doc = new PrintDocument())
                {
                    // Oculta el cuadro emergente predeterminado de Windows "Imprimiendo..."
                    doc.PrintController = new StandardPrintController();

                    // Configuración del tamaño de papel (80mm térmica ~300px)
                    doc.DefaultPageSettings.PaperSize = new PaperSize("Ticket", 300, 500);

                    // Si tienes una impresora térmica específica, puedes asignarla directamente por nombre:
                    // doc.PrinterSettings.PrinterName = "NombreDeTuImpresoraTermica";

                    doc.PrintPage += ImprimirVoucher_PrintPage;

                    // Envío directo a la impresora predeterminada
                    doc.Print();
                }

                Program.Navegar(this, new FormVoucher());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar a la impresora: {ex.Message}", "Error de Impresión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Program.Navegar(this, new FormFinProceso());
            }
        }

        private void ImprimirVoucher_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            using (Font fontTitulo = new Font("Courier New", 12, FontStyle.Bold))
            using (Font fontCuerpo = new Font("Courier New", 9, FontStyle.Regular))
            using (Font fontNegrita = new Font("Courier New", 9, FontStyle.Bold))
            using (SolidBrush brocha = new SolidBrush(Color.Black))
            {
                int x = 15;
                int y = 20;
                int interlineado = 18;

                // Encabezado
                g.DrawString("      CAJERO AUTOMATICO     ", fontTitulo, brocha, x, y);
                y += interlineado + 5;
                g.DrawString("--------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Metadatos
                g.DrawString($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("Cajero: #01-AGENCIA", fontCuerpo, brocha, x, y);
                y += interlineado;

                // 1. OBTENER PROPIEDAD REAL DEL USUARIO (ajusta .Nombre por la propiedad de tu modelo)
                var u = Sesion.UsuarioActual;
                string nombreUsuario = u?.Nombre ?? "CLIENTE";
                g.DrawString($"Usuario: {nombreUsuario}", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("--------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Detalle de saldos
                g.DrawString("CONSULTA DE SALDOS", fontNegrita, brocha, x, y);
                y += interlineado + 5;

                // 2. LEER DIRECTAMENTE EL TEXTO YA FORMATEADO DE TUS LABELS
                g.DrawString(lblLimiteMaximo.Text, fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString(lblDisponibleHoy.Text, fontCuerpo, brocha, x, y);
                y += interlineado + 5;

                // Pie del comprobante
                g.DrawString("--------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("   ¡Gracias por su preferencia!  ", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString(" Guarde este comprobante como   ", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("      respaldo de su operacion.  ", fontCuerpo, brocha, x, y);
            }
        }
    }
}
