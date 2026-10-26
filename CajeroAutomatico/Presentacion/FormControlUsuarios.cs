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
    public partial class FormControlUsuarios : Form
    {
        public FormControlUsuarios()
        {
            InitializeComponent();
            CargarDatos();
        }
        private void CargarDatos()
        {
            var dto = Program.Servicio.ObtenerControlUsuarios();

            lblTotalRetirado.Text = $"Total retirado hoy (todos los usuarios): Q.{dto.TotalRetiradoTodos:N2}";
            lblPromedioDepositado.Text = $"Promedio depositado hoy: Q.{dto.PromedioDepositado:N2}";

            lstUsuariosCambioPin.Items.Clear();
            if (dto.UsuariosQueCambiaronPin.Count == 0)
                lstUsuariosCambioPin.Items.Add("(Ninguno)");
            else
                foreach (var nombre in dto.UsuariosQueCambiaronPin)
                    lstUsuariosCambioPin.Items.Add(nombre);

            lblUltimoUsuario.Text = $"Último usuario en acceder: {dto.UltimoUsuarioAcceso ?? "N/A"}";
            lblFechaUltimoAcceso.Text = dto.FechaUltimoAcceso == System.DateTime.MinValue
                ? "Fecha: N/A"
                : $"Fecha: {dto.FechaUltimoAcceso:g}";
        }

        private void btnGenerarPdf_Click(object sender, EventArgs e)
        {
            var ruta = System.IO.Path.Combine("Datos", $"reporte_control_usuarios_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt");
            Program.Servicio.GenerarReporteControlUsuarios(ruta);
            lblMensaje.Text = $"Reporte generado en: {ruta}";
            MessageBox.Show($"Reporte generado en: {ruta}", "Reporte generado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Program.Navegar(this, new FormMenuAdmin());
        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormMenuAdmin());
        }

        private void btnBoleta_Click(object sender, EventArgs e)
        {
            try
            {
                using (PrintDocument doc = new PrintDocument())
                {
                    doc.PrintController = new StandardPrintController();
                    doc.DefaultPageSettings.PaperSize = new PaperSize("Ticket", 300, 650);

                    doc.PrintPage += ImprimirBoletaAdmin_PrintPage;
                    doc.Print();
                }

                MessageBox.Show("Boleta de control administrativo impresa con éxito.", "Impresión Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Se mantiene en este formulario para que el administrador decida cuándo volver con btnVolver
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar a la impresora: {ex.Message}", "Error de Impresión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImprimirBoletaAdmin_PrintPage(object sender, PrintPageEventArgs e)
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
                g.DrawString("   CORTE Y CONTROL DE USUARIOS  ", fontNegrita, brocha, x, y);
                y += interlineado + 2;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Metadatos
                g.DrawString($"Fecha Emisión: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("Perfil: ADMINISTRADOR", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("Terminal: #01-AGENCIA", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Estadísticas generales del día
                g.DrawString("RESUMEN DE OPERACIONES (HOY)", fontNegrita, brocha, x, y);
                y += interlineado + 4;

                // Leemos los textos de los labels que ya tienen el cálculo de la capa de servicio
                g.DrawString(lblTotalRetirado.Text, fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString(lblPromedioDepositado.Text, fontCuerpo, brocha, x, y);
                y += interlineado + 4;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Última sesión registrada
                g.DrawString("REGISTRO DE ACCESOS", fontNegrita, brocha, x, y);
                y += interlineado + 4;
                g.DrawString(lblUltimoUsuario.Text, fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString(lblFechaUltimoAcceso.Text, fontCuerpo, brocha, x, y);
                y += interlineado + 4;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Lista de usuarios que cambiaron PIN
                g.DrawString("CAMBIOS DE PIN REGISTRADOS:", fontNegrita, brocha, x, y);
                y += interlineado + 4;

                foreach (var item in lstUsuariosCambioPin.Items)
                {
                    g.DrawString($" - {item}", fontCuerpo, brocha, x, y);
                    y += interlineado;
                }

                // Pie de comprobante administrativo
                y += 6;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("  *** DOCUMENTO ADMINISTRATIVO ***  ", fontNegrita, brocha, x, y);
                y += interlineado;
                g.DrawString("      FIN DE INFORME DEL SISTEMA    ", fontCuerpo, brocha, x, y);
            }
        }

        private void btnVoucher_Click(object sender, EventArgs e)
        {

        }
    }
}
