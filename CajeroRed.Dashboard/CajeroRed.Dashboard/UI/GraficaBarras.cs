using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CajeroRed.Dashboard.UI;

/// <summary>Una serie de la gráfica (por ejemplo "Retiros"): nombre, color y un valor por categoría.</summary>
public class SerieGrafica
{
    public string Nombre { get; set; } = "";
    public Color Color { get; set; } = Tema.Acento;
    public double[] Valores { get; set; } = Array.Empty<double>();
}

/// <summary>Gráfica de barras (agrupadas si hay varias series) dibujada a mano:
/// no necesita instalar ningún paquete externo.</summary>
public class GraficaBarras : Control
{
    public string[] Categorias { get; private set; } = Array.Empty<string>();
    public List<SerieGrafica> Series { get; } = new();

    /// <summary>Muestra el valor sobre cada barra (útil con pocas categorías).</summary>
    public bool MostrarValores { get; set; }
    public Func<double, string> FormatoValor { get; set; } = v => Formato.Numero(v);
    public Func<double, string> FormatoEje { get; set; } =
        v => v >= 1000 ? (v / 1000).ToString("0.#") + "k" : v.ToString("0");
    public string MensajeVacio { get; set; } = "Sin datos para mostrar";

    public GraficaBarras()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Tema.Tarjeta;
    }

    public void Actualizar(string[] categorias, IEnumerable<SerieGrafica> series)
    {
        Categorias = categorias;
        Series.Clear();
        Series.AddRange(series);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        using var fuente = Tema.Fuente(8.5f);

        if (Categorias.Length == 0 || Series.Count == 0)
        {
            TextRenderer.DrawText(g, MensajeVacio, fuente, ClientRectangle, Tema.TextoSuave,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            return;
        }

        // ----- escala "bonita" del eje Y (4 divisiones aprox.) -----
        double max = Series.SelectMany(s => s.Valores).DefaultIfEmpty(0).Max();
        if (max <= 0) max = 1;
        double bruto = max / 4;
        double magnitud = Math.Pow(10, Math.Floor(Math.Log10(bruto)));
        double norm = bruto / magnitud;
        double paso = (norm <= 1 ? 1 : norm <= 2 ? 2 : norm <= 5 ? 5 : 10) * magnitud;
        int divisiones = Math.Max(1, (int)Math.Ceiling(max / paso - 1e-9));
        double tope = paso * divisiones;

        // ----- área de dibujo -----
        int margenIzq = 54, margenDer = 12, margenInf = 26;
        int margenSup = Series.Count > 1 ? 28 : 16;
        var area = new Rectangle(margenIzq, margenSup, Width - margenIzq - margenDer, Height - margenSup - margenInf);
        if (area.Width < 40 || area.Height < 40) return;

        // ----- leyenda (solo si hay varias series) -----
        if (Series.Count > 1)
        {
            int ancho = 0;
            foreach (var s in Series)
                ancho += 14 + TextRenderer.MeasureText(s.Nombre, fuente).Width + 14;
            int xLeyenda = Width - margenDer - ancho;
            foreach (var s in Series)
            {
                using var pincelLeyenda = new SolidBrush(s.Color);
                g.FillRectangle(pincelLeyenda, xLeyenda, 9, 10, 10);
                TextRenderer.DrawText(g, s.Nombre, fuente, new Point(xLeyenda + 14, 5), Tema.TextoSuave);
                xLeyenda += 14 + TextRenderer.MeasureText(s.Nombre, fuente).Width + 14;
            }
        }

        // ----- líneas guía y etiquetas del eje Y -----
        using (var lapizGuia = new Pen(Tema.Borde))
        {
            for (int i = 0; i <= divisiones; i++)
            {
                int y = area.Bottom - (int)Math.Round(area.Height * (i * paso / tope));
                g.DrawLine(lapizGuia, area.Left, y, area.Right, y);
                TextRenderer.DrawText(g, FormatoEje(i * paso), fuente,
                    new Rectangle(0, y - 8, margenIzq - 6, 16), Tema.TextoSuave,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }

        // ----- barras -----
        int n = Categorias.Length;
        double anchoGrupo = area.Width / (double)n;
        double anchoBarra = Math.Min(44, anchoGrupo * 0.72 / Series.Count);

        for (int i = 0; i < n; i++)
        {
            double inicioGrupo = area.Left + anchoGrupo * i + (anchoGrupo - anchoBarra * Series.Count) / 2;

            for (int j = 0; j < Series.Count; j++)
            {
                var serie = Series[j];
                double valor = i < serie.Valores.Length ? serie.Valores[i] : 0;
                int alto = (int)Math.Round(area.Height * valor / tope);
                int x = (int)(inicioGrupo + anchoBarra * j);
                int anchoPx = (int)Math.Max(2, anchoBarra - (Series.Count > 1 ? 2 : 0));

                if (alto > 0)
                {
                    using var pincel = new SolidBrush(serie.Color);
                    g.FillRectangle(pincel, x, area.Bottom - alto, anchoPx, alto);

                    if (MostrarValores)
                    {
                        TextRenderer.DrawText(g, FormatoValor(valor), fuente,
                            new Rectangle(x - 24, area.Bottom - alto - 17, anchoPx + 48, 16), Tema.Texto,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    }
                }
            }

            // etiqueta del eje X
            TextRenderer.DrawText(g, Categorias[i], fuente,
                new Rectangle((int)(area.Left + anchoGrupo * i), area.Bottom + 5, (int)anchoGrupo, 18),
                Tema.TextoSuave,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        using var lapizBase = new Pen(Color.FromArgb(203, 213, 225));
        g.DrawLine(lapizBase, area.Left, area.Bottom, area.Right, area.Bottom);
    }
}
