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
    public partial class FormConsultaUsuario : Form
    {
        private string tarjetaActual = null;
        public FormConsultaUsuario()
        {
            InitializeComponent();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            var dto = Program.Servicio.ConsultarUsuario(txtNumeroTarjeta.Text.Trim());

            if (dto == null)
            {
                lblNombre.Text = "No se encontró ningún usuario con esa tarjeta.";
                lblMontoMaximoDisponible.Text = "";
                lblCantidadRetiradaHastaElMomento.Text = "";
                lblSaldoActual.Text = "";
                lblUltimoAcceso.Text = "";
                lblEstadoCuenta.Text = "";
                btnCambiarEstado.Visible = false;
                tarjetaActual = null;
                return;
            }

            tarjetaActual = txtNumeroTarjeta.Text.Trim();

            lblNombre.Text = $"Nombre: {dto.Nombre}";
            lblMontoMaximoDisponible.Text = $"Monto máximo disponible (límite diario): Q.{dto.MontoMaximoDisponible:N2}";
            lblCantidadRetiradaHastaElMomento.Text = $"Retirado hoy hasta el momento: Q.{dto.CantidadRetiradaHastaElMomento:N2}";
            lblSaldoActual.Text = $"Saldo actual: Q.{dto.SaldoActual:N2}";
            lblUltimoAcceso.Text = dto.UltimoAcceso == DateTime.MinValue
                ? "Último acceso: nunca ha ingresado"
                : $"Último acceso: {dto.UltimoAcceso:g}";

            ActualizarEstadoCuenta(dto.bloqueado);
        }
        private void ActualizarEstadoCuenta(bool bloqueado)
        {
            if (bloqueado)
            {
                lblEstadoCuenta.Text = "Estado: BLOQUEADO";
                lblEstadoCuenta.ForeColor = System.Drawing.Color.Red;
                btnCambiarEstado.Text = "Desbloquear cuenta";
                btnCambiarEstado.BackColor = System.Drawing.Color.LightGreen;
            }
            else
            {
                lblEstadoCuenta.Text = "Estado: ACTIVO";
                lblEstadoCuenta.ForeColor = System.Drawing.Color.Green;
                btnCambiarEstado.Text = "Bloquear cuenta";
                btnCambiarEstado.BackColor = System.Drawing.Color.LightCoral;
            }
            btnCambiarEstado.Visible = true;
            btnCambiarEstado.Enabled = true;
        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormMenuAdmin());
        }

        private void btnCambiarEstado_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tarjetaActual)) return;

            bool bloquear = btnCambiarEstado.Text == "Bloquear cuenta";
            string accion = bloquear ? "bloquear" : "desbloquear";

            var resp = MessageBox.Show(
                $"¿Está seguro que desea {accion} la cuenta del usuario {tarjetaActual}?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resp != DialogResult.Yes) return;

            bool ok = Program.Servicio.CambiarEstadoBloqueo(tarjetaActual, bloquear);
            if (!ok)
            {
                MessageBox.Show("No se pudo actualizar el estado de la cuenta.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show($"Cuenta {(bloquear ? "bloqueada" : "desbloqueada")} correctamente.",
                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

            btnBuscar_Click(sender, e);
        }

        private void btnVoucher_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tarjetaActual))
            {
                MessageBox.Show("Debe consultar un usuario válido antes de imprimir el voucher.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (PrintDocument doc = new PrintDocument())
                {
                    // Impresión silenciosa en segundo plano
                    doc.PrintController = new StandardPrintController();
                    doc.DefaultPageSettings.PaperSize = new PaperSize("Ticket", 300, 650);

                    doc.PrintPage += ImprimirFichaUsuario_PrintPage;
                    doc.Print();
                }

                MessageBox.Show("Ficha de usuario impresa correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar a la impresora: {ex.Message}", "Error de Impresión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImprimirFichaUsuario_PrintPage(object sender, PrintPageEventArgs e)
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
                y += interlineado + 4;
                g.DrawString("    FICHA DE AUDITORIA CLIENTE  ", fontNegrita, brocha, x, y);
                y += interlineado + 2;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Metadatos
                g.DrawString($"Fecha Consulta: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("Perfil: ADMINISTRADOR", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("Terminal: #01-AGENCIA", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Datos de la cuenta
                g.DrawString("INFORMACION DE CUENTA:", fontNegrita, brocha, x, y);
                y += interlineado + 4;

                string tarjetaOculta = tarjetaActual.Length >= 4
                    ? $"**** **** **** {tarjetaActual.Substring(tarjetaActual.Length - 4)}"
                    : tarjetaActual;

                g.DrawString($"Tarjeta: {tarjetaOculta}", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString(lblNombre.Text, fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString(lblEstadoCuenta.Text, fontNegrita, brocha, x, y);
                y += interlineado + 4;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Balances y límites
                g.DrawString("BALANCES Y MOVIMIENTOS:", fontNegrita, brocha, x, y);
                y += interlineado + 4;

                g.DrawString(lblSaldoActual.Text, fontNegrita, brocha, x, y);
                y += interlineado;
                g.DrawString(lblMontoMaximoDisponible.Text, fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString(lblCantidadRetiradaHastaElMomento.Text, fontCuerpo, brocha, x, y);
                y += interlineado + 4;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Historial de accesos
                g.DrawString("ACCESO:", fontNegrita, brocha, x, y);
                y += interlineado + 4;
                g.DrawString(lblUltimoAcceso.Text, fontCuerpo, brocha, x, y);
                y += interlineado + 6;

                // Pie de comprobante administrativo
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("  *** DOCUMENTO CONFIDENCIAL ***    ", fontNegrita, brocha, x, y);
                y += interlineado;
                g.DrawString("   Uso exclusivo para auditoria     ", fontCuerpo, brocha, x, y);
            }
        }

        private void FormConsultaUsuario_Load(object sender, EventArgs e)
        {

        }
    }
}
