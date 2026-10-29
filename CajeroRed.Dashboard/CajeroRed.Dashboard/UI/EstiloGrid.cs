using System.Drawing;
using System.Windows.Forms;

namespace CajeroRed.Dashboard.UI;

/// <summary>Estilo profesional compartido por todas las tablas (DataGridView) del dashboard.</summary>
public static class EstiloGrid
{
    public static void Aplicar(DataGridView g)
    {
        var gris = ColorTranslator.FromHtml("#F8FAFC");

        g.BackgroundColor = Tema.Tarjeta;
        g.BorderStyle = BorderStyle.None;
        g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        g.GridColor = Tema.Borde;
        g.EnableHeadersVisualStyles = false;

        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        g.ColumnHeadersHeight = 36;
        g.ColumnHeadersDefaultCellStyle.BackColor = gris;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Tema.TextoSuave;
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = gris;
        g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Tema.TextoSuave;
        g.ColumnHeadersDefaultCellStyle.Font = Tema.Fuente(9f, FontStyle.Bold);
        g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);

        g.RowHeadersVisible = false;
        g.RowTemplate.Height = 36;
        g.DefaultCellStyle.BackColor = Tema.Tarjeta;
        g.DefaultCellStyle.ForeColor = Tema.Texto;
        g.DefaultCellStyle.Font = Tema.Fuente(9.5f);
        g.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#DBEAFE");
        g.DefaultCellStyle.SelectionForeColor = Tema.Texto;
        g.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
        g.AlternatingRowsDefaultCellStyle.BackColor = gris;

        g.AllowUserToAddRows = false;
        g.AllowUserToDeleteRows = false;
        g.AllowUserToResizeRows = false;
        g.ReadOnly = true;
        g.MultiSelect = false;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
    }
}
