using CajeroAutomatico.Negocio;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace CajeroAutomatico.Presentacion
{
    public partial class FormVerSaldo : Form
    {
        // Datos que se muestran en pantalla y se imprimen en el voucher
        private decimal _saldoActual;
        private decimal _limiteUsuario;    // Límite diario del usuario (LimiteDiario)
        private decimal _disponibleHoy;    // Cuánto le queda por retirar hoy
        private decimal _limiteSistema;    // Máximo por operación (MAX_CAPACIDAD_CAJERO o similar)

        public FormVerSaldo()
        {
            InitializeComponent();
        }

        private void FormVerSaldo_Load(object sender, EventArgs e)
        {
            CargarSaldo();
            VozService.Hablar("Su saldo ha sido consultado");
        }

        private void CargarSaldo()
        {
            var respuesta = Program.Servidor.ConsultarSaldo();

            if (!respuesta.Exito)
            {
                lblSaldoActual.Text = "—";
                lblLimiteUsuario.Text = "—";
                lblDisponibleHoy.Text = "—";
                lblLimiteSistema.Text = "—";
                MessageBox.Show(respuesta.Mensaje ?? "No se pudo consultar el saldo.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Claves tal como las envía ServidorCentral
            _saldoActual = respuesta.Decimal("SaldoActual");
            _limiteUsuario = respuesta.Decimal("LimiteMaximo");
            _disponibleHoy = respuesta.Decimal("DisponibleHoy");
            _limiteSistema = respuesta.Decimal("LimiteSistema");

            lblSaldoActual.Text = $"Saldo actual: {_saldoActual:C}";
            lblLimiteUsuario.Text = $"Límite diario del usuario: {_limiteUsuario:C}";
            lblDisponibleHoy.Text = $"Disponible para hoy: {_disponibleHoy:C}";
            lblLimiteSistema.Text = $"Límite máximo por retiro: {_limiteSistema:C}";

            // Colores según estado
            lblSaldoActual.ForeColor = _saldoActual > 0 ? Color.DarkGreen : Color.DarkRed;
            lblDisponibleHoy.ForeColor = _disponibleHoy > 0 ? Color.DarkGreen : Color.DarkRed;
            lblLimiteUsuario.ForeColor = Color.Black;
            lblLimiteSistema.ForeColor = Color.Black;
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
                    doc.PrintController = new StandardPrintController();
                    doc.DefaultPageSettings.PaperSize = new PaperSize("Ticket", 300, 550);
                    doc.PrintPage += ImprimirVoucher_PrintPage;
                    doc.Print();
                }

                Program.Navegar(this, new FormVoucher());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar a la impresora: {ex.Message}", "Error de Impresión",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                g.DrawString($"Cajero: {Program.Servidor.CodigoCajero}", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Usuario y tarjeta
                var u = Sesion.UsuarioActual;
                string nombre = u?.Nombre ?? "CLIENTE";
                string tarjeta = u?.NumeroTarjeta != null && u.NumeroTarjeta.Length >= 4
                    ? $"**** **** **** {u.NumeroTarjeta.Substring(u.NumeroTarjeta.Length - 4)}"
                    : "**** **** **** ****";

                g.DrawString($"Usuario: {nombre}", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString($"Tarjeta: {tarjeta}", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("--------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Sección de saldos
                g.DrawString("CONSULTA DE SALDOS", fontNegrita, brocha, x, y);
                y += interlineado + 5;

                // Leer los textos ya formateados de los labels
                g.DrawString(lblSaldoActual.Text, fontCuerpo, brocha, x, y); y += interlineado;
                g.DrawString(lblLimiteUsuario.Text, fontCuerpo, brocha, x, y); y += interlineado;
                g.DrawString(lblDisponibleHoy.Text, fontCuerpo, brocha, x, y); y += interlineado;
                g.DrawString(lblLimiteSistema.Text, fontCuerpo, brocha, x, y); y += interlineado + 5;

                // Pie
                g.DrawString("--------------------------------", fontCuerpo, brocha, x, y); y += interlineado;
                g.DrawString("   ¡Gracias por su preferencia!  ", fontCuerpo, brocha, x, y); y += interlineado;
                g.DrawString(" Guarde este comprobante como   ", fontCuerpo, brocha, x, y); y += interlineado;
                g.DrawString("      respaldo de su operacion.  ", fontCuerpo, brocha, x, y);
            }
        }
    }
}