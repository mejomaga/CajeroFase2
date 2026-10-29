using System.Drawing;
using System.Windows.Forms;

namespace CajeroRed.Dashboard.UI;

/// <summary>Tarjeta de indicador (KPI): título pequeño, valor grande y barra de color a la izquierda.</summary>
public class TarjetaIndicador : Tarjeta
{
    private string _valor = "—";
    private string _detalle = "";
    private Color _acento;

    public string Titulo { get; }

    public string Valor
    {
        get => _valor;
        set { if (_valor != value) { _valor = value; Invalidate(); } }
    }

    public string Detalle
    {
        get => _detalle;
        set { if (_detalle != value) { _detalle = value; Invalidate(); } }
    }

    /// <summary>Verde = correcto, naranja = advertencia, rojo = falla.</summary>
    public Color ColorAcento
    {
        get => _acento;
        set { if (_acento != value) { _acento = value; Invalidate(); } }
    }

    public TarjetaIndicador(string titulo, Color acento)
    {
        Titulo = titulo;
        _acento = acento;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); // dibuja el fondo y el borde

        using (var barra = new SolidBrush(_acento))
            e.Graphics.FillRectangle(barra, 0, 0, 5, Height);

        using var fuenteTitulo = Tema.Fuente(9f);
        using var fuenteValor = Tema.Fuente(17f, FontStyle.Bold);
        using var fuenteDetalle = Tema.Fuente(8.5f);

        TextRenderer.DrawText(e.Graphics, Titulo, fuenteTitulo, new Point(16, 10), Tema.TextoSuave);
        TextRenderer.DrawText(e.Graphics, _valor, fuenteValor, new Point(14, 30), Tema.Texto);
        if (_detalle.Length > 0)
            TextRenderer.DrawText(e.Graphics, _detalle, fuenteDetalle, new Point(16, 64), Tema.TextoSuave);
    }
}
