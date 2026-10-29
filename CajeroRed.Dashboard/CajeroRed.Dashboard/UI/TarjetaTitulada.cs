using System.Drawing;
using System.Windows.Forms;

namespace CajeroRed.Dashboard.UI;

/// <summary>Tarjeta con un título arriba y un control (tabla, gráfica...) ocupando el resto.</summary>
public class TarjetaTitulada : Tarjeta
{
    public Label LblTitulo { get; } = new();

    public TarjetaTitulada(string titulo, Control contenido)
    {
        Padding = new Padding(16, 12, 16, 12);

        var tabla = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Tema.Tarjeta,
            Margin = Padding.Empty
        };
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        LblTitulo.Text = titulo;
        LblTitulo.Dock = DockStyle.Fill;
        LblTitulo.Margin = Padding.Empty;
        LblTitulo.TextAlign = ContentAlignment.MiddleLeft;
        LblTitulo.Font = Tema.Fuente(11f, FontStyle.Bold);
        LblTitulo.ForeColor = Tema.Texto;

        contenido.Dock = DockStyle.Fill;
        contenido.Margin = Padding.Empty;

        tabla.Controls.Add(LblTitulo, 0, 0);
        tabla.Controls.Add(contenido, 0, 1);
        Controls.Add(tabla);
    }
}
