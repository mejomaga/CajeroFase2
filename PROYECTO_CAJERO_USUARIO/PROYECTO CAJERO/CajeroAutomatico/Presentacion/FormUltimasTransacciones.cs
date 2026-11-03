using CajeroAutomatico.Entidades;
using CajeroAutomatico.Negocio;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace CajeroAutomatico.Presentacion
{
    public partial class FormUltimasTransacciones : Form
    {
        public FormUltimasTransacciones()
        {
            InitializeComponent();
            this.Load += FormUltimasTransacciones_Load;
        }

        private void FormUltimasTransacciones_Load(object sender, EventArgs e)
        {
            CargarTransacciones();
            try { VozService.Hablar("Estas son sus últimas transacciones"); } catch { }
        }

        private void CargarTransacciones()
        {
            if (Sesion.UsuarioActual == null) return;

            try
            {
                var respuesta = Program.Servidor.UltimasTransacciones(5);

                // 1. Crear columnas si el grid está vacío
                if (dgvTransacciones.Columns.Count == 0)
                {
                    dgvTransacciones.Columns.Add("Fecha", "Fecha");
                    dgvTransacciones.Columns.Add("Hora", "Hora");
                    dgvTransacciones.Columns.Add("Tipo", "Tipo Transacción");
                    dgvTransacciones.Columns.Add("Monto", "Monto");
                }

                dgvTransacciones.Rows.Clear();
                bool hayFilas = false;

                if (respuesta != null && respuesta.Exito && respuesta.Datos != null && respuesta.Datos.ContainsKey("Transacciones"))
                {
                    // 2. Extraemos el string JSON crudo del diccionario
                    string jsonTransacciones = respuesta.Datos["Transacciones"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(jsonTransacciones))
                    {
                        // 3. Deserializamos a una lista de diccionarios de JsonElement
                        var lista = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(jsonTransacciones);

                        if (lista != null)
                        {
                            foreach (var t in lista)
                            {
                                // El servidor ya envía la fecha y hora separadas y formateadas
                                string fechaStr = ObtenerTexto(t, "fecha", "Fecha");
                                string horaStr = ObtenerTexto(t, "hora", "Hora");
                                string tipo = ObtenerTexto(t, "tipo", "Tipo");
                                decimal monto = ObtenerDecimal(t, "monto", "Monto");

                                dgvTransacciones.Rows.Add(
                                    fechaStr,
                                    horaStr,
                                    tipo,
                                    $"Q.{monto:N2}");

                                hayFilas = true;
                            }
                        }
                    }
                }

                if (!hayFilas)
                {
                    dgvTransacciones.Rows.Add("—", "—", "Sin transacciones registradas", "—");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al cargar el historial:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dgvTransacciones.Rows.Add("—", "—", "Error de lectura", "—");
            }
        }

        // Helpers ajustados para leer Dictionary<string, JsonElement> sin importar mayúsculas
        private static string ObtenerTexto(Dictionary<string, JsonElement> dict, params string[] claves)
        {
            foreach (var clave in claves)
            {
                var keyReal = dict.Keys.FirstOrDefault(k => string.Equals(k, clave, StringComparison.OrdinalIgnoreCase));
                if (keyReal != null)
                {
                    return dict[keyReal].ToString();
                }
            }
            return "";
        }

        private static decimal ObtenerDecimal(Dictionary<string, JsonElement> dict, params string[] claves)
        {
            string s = ObtenerTexto(dict, claves);
            return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;
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
                    doc.DefaultPageSettings.PaperSize = new PaperSize("Ticket", 300, 600);
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
                g.DrawString($"Cajero: {Program.Servidor.CodigoCajero}", fontCuerpo, brocha, x, y);
                y += interlineado;

                // Datos del usuario
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

                // Título de sección
                g.DrawString("       ULTIMOS MOVIMIENTOS          ", fontNegrita, brocha, x, y);
                y += interlineado + 4;

                string cabecera = string.Format("{0,-11}{1,-10}{2,13}", "FECHA", "TIPO", "MONTO");
                g.DrawString(cabecera, fontNegrita, brocha, x, y);
                y += interlineado;
                g.DrawString("------------------------------------", fontCuerpo, brocha, x, y);
                y += interlineado;

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

                    if (tipo.Length > 9) tipo = tipo.Substring(0, 9);

                    string linea = string.Format("{0,-11}{1,-10}{2,13}", fecha, tipo, monto);
                    g.DrawString(linea, fontCuerpo, brocha, x, y);
                    y += interlineado;
                }

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