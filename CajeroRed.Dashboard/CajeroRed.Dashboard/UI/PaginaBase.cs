using System.Windows.Forms;

namespace CajeroRed.Dashboard.UI;

/// <summary>Base de todas las pantallas (módulos) del dashboard.
/// Cada módulo hereda de esta clase y sobrescribe Titulo, Subtitulo y ActualizarAsync.</summary>
public abstract class PaginaBase : UserControl
{
    public abstract string Titulo { get; }
    public virtual string Subtitulo => "";

    protected PaginaBase()
    {
        Dock = DockStyle.Fill;
        BackColor = Tema.Fondo;
        Padding = new Padding(24);
        DoubleBuffered = true;
    }

    /// <summary>Se llama al abrir la pantalla y luego cada 5 segundos.</summary>
    public virtual Task ActualizarAsync() => Task.CompletedTask;
}
