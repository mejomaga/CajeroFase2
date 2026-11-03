using CajeroAutomatico.Negocio;
using System;
using System.Collections.Generic;
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
            this.Load += FormVerSaldo_Load;
        }

        private void FormVerSaldo_Load(object sender, EventArgs e)
        {
            CargarSaldo();
            VozService.Hablar("Su saldo ha sido consultado");
        }

        private void CargarSaldo()
        {
            try
            {
                var respuesta = Program.Servidor.ConsultarSaldo();

                if (respuesta == null || !respuesta.Exito)
                {
                    lblSaldoActual.Text = "—";
                    lblLimiteUsuario.Text = "—";
                    lblDisponibleHoy.Text = "—";
                    lblLimiteSistema.Text = "—";
                    MessageBox.Show(respuesta?.Mensaje ?? "No se pudo consultar el saldo.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Extracción segura directa desde el diccionario Datos
                _saldoActual = ObtenerDecimalSeguro(respuesta.Datos, "SaldoActual");
                _limiteUsuario = ObtenerDecimalSeguro(respuesta.Datos, "LimiteMaximo");
                _disponibleHoy = ObtenerDecimalSeguro(respuesta.Datos, "DisponibleHoy");
                _limiteSistema = ObtenerDecimalSeguro(respuesta.Datos, "LimiteSistema");

                lblSaldoActual.Text = $"Saldo actual: {_saldoActual:C}";
                lblLimiteUsuario.Text = $"Límite diario del usuario: {_limiteUsuario:C}";
                lblDisponibleHoy.Text = $"Disponible para hoy: {_disponibleHoy:C}";
                lblLimiteSistema.Text = $"Límite máximo por retiro: {_limiteSistema:C}";

                // Colores según estado (ajusta colores si tu fondo es oscuro)
                lblSaldoActual.ForeColor = _saldoActual > 0 ? Color.LimeGreen : Color.LightCoral;
                lblDisponibleHoy.ForeColor = _disponibleHoy > 0 ? Color.LimeGreen : Color.LightCoral;
                lblLimiteUsuario.ForeColor = Color.White;
                lblLimiteSistema.ForeColor = Color.White;
            }
            catch (Exception ex)
            {
                // Si algo falla, ahora sí verás exactamente por qué
                MessageBox.Show($"Ocurrió un error al procesar los datos de saldo:\n{ex.Message}",
                    "Error de lectura", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Método auxiliar para convertir con seguridad sin importar si viene como string, int o decimal
        private decimal ObtenerDecimalSeguro(Dictionary<string, object> datos, string clave)
        {
            if (datos != null && datos.TryGetValue(clave, out object valor) && valor != null)
            {
                try
                {
                    return Convert.ToDecimal(valor);
                }
                catch
                {
                    return 0m;
                }
            }
            return 0m;
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