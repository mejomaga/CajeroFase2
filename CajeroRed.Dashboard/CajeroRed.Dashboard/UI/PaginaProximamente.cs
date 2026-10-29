using System.Drawing;
using System.Windows.Forms;

namespace CajeroRed.Dashboard.UI;

/// <summary>Pantalla temporal para los módulos que todavía no están construidos.</summary>
public class PaginaProximamente : PaginaBase
{
    private readonly string _titulo;
    private readonly string _subtitulo;

    public override string Titulo => _titulo;
    public override string Subtitulo => _subtitulo;

    public PaginaProximamente(string titulo, string subtitulo, string etapa)
    {
        _titulo = titulo;
        _subtitulo = subtitulo;

        var tarjeta = new Tarjeta { Dock = DockStyle.Fill };
        tarjeta.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Tema.TextoSuave,
            Font = Tema.Fuente(12f),
            Text = $"Módulo «{titulo}»\n\nSe construye en: {etapa}."
        });
        Controls.Add(tarjeta);
    }
}
