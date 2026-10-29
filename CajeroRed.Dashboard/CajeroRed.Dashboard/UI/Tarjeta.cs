using System.Drawing;
using System.Windows.Forms;

namespace CajeroRed.Dashboard.UI;

/// <summary>Panel blanco con borde fino. Se usa como "tarjeta" para indicadores, tablas y gráficas.</summary>
public class Tarjeta : Panel
{
    public Tarjeta()
    {
        BackColor = Tema.Tarjeta;
        Padding = new Padding(16);
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var lapiz = new Pen(Tema.Borde);
        e.Graphics.DrawRectangle(lapiz, 0, 0, Width - 1, Height - 1);
    }
}
