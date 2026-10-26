using CajeroAutomatico.Entidades;
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
    public partial class FormUltimasTransacciones : Form
    {
        public FormUltimasTransacciones()
        {
            InitializeComponent();
            CargarTransacciones();
        }

        private void CargarTransacciones()
        {
            var usuario = Sesion.UsuarioActual;
            if (usuario == null) return;

            var transacciones = Program.Servicio.ObtenerUltimasTransacciones(usuario.NumeroTarjeta, 5);

            // 1. CREAR LAS COLUMNAS POR CÓDIGO SI EL GRID ESTÁ VACÍO
            if (dgvTransacciones.Columns.Count == 0)
            {
                dgvTransacciones.Columns.Add("Fecha", "Fecha");
                dgvTransacciones.Columns.Add("Hora", "Hora");
                dgvTransacciones.Columns.Add("Tipo", "Tipo Transacción");
                dgvTransacciones.Columns.Add("Monto", "Monto");
            }

            // 2. Limpiar filas viejas e insertar los datos
            dgvTransacciones.Rows.Clear();
            foreach (var t in transacciones)
            {
                var tipo = t.Tipo == TipoTransaccion.Retiro ? "Retiro" : "Depósito";
                dgvTransacciones.Rows.Add(t.Fecha.ToString("d"), t.Hora.ToString(@"hh\:mm"), tipo, $"Q.{t.Monto:N2}");
            }

            if (transacciones.Count == 0)
            {
                dgvTransacciones.Rows.Add("—", "—", "Sin transacciones registradas", "—");
            }
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
                    // Impresión silenciosa sin cuadro emergente
                    doc.PrintController = new StandardPrintController();

                    // Alto de 600px para dar margen a la lista de movimientos
                    doc.DefaultPageSettings.PaperSize = new PaperSize("Ticket", 300, 600);

                    doc.PrintPage += ImprimirVoucher_PrintPage;
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

                // Datos de usuario y tarjeta enmascarada
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

                // Título de sección y cabecera de columnas
                g.DrawString("       ULTIMOS MOVIMIENTOS          ", fontNegrita, brocha, x, y);
                y += interlineado + 4;

                // Columnas: Fecha(8) | Tipo(10) | Monto(14)
                string cabecera = string.Format("{0,-9}{1,-11}{2,14}", "FECHA", "TIPO", "MONTO");
                g.DrawString(cabecera, fontNegrita, brocha, x, y);
                y += interlineado;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Iterar directamente sobre las filas del DataGridView
                foreach (DataGridViewRow fila in dgvTransacciones.Rows)
                {
                    if (fila.IsNewRow) continue;

                    string fecha = fila.Cells["Fecha"].Value?.ToString() ?? "";
                    string tipo = fila.Cells["Tipo"].Value?.ToString() ?? "";
                    string monto = fila.Cells["Monto"].Value?.ToString() ?? "";

                    if (tipo.Contains("Sin transacciones"))
                    {
                        g.DrawString("    Sin transacciones registradas   ", fontCuerpo, brocha, x, y);
                        y += interlineado;
                        break;
                    }

                    // Abreviar el tipo si es necesario para mantener alineación
                    if (tipo.Length > 9) tipo = tipo.Substring(0, 9);

                    // Formato tabular alineado
                    string linea = string.Format("{0,-9}{1,-11}{2,14}", fecha, tipo, monto);
                    g.DrawString(linea, fontCuerpo, brocha, x, y);
                    y += interlineado;
                }

                // Pie de comprobante
                y += 5;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("   ¡Gracias por su preferencia!     ", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString(" Guarde este comprobante como       ", fontCuerpo, brocha, x, y);
                y += interlineado;
                g.DrawString("      respaldo de su operacion.     ", fontCuerpo, brocha, x, y);
            }
        }
    }
}
