using System.Drawing;
using System.Windows.Forms;

namespace CajeroRed.Dashboard.UI;

/// <summary>Botón del menú lateral, con barra azul cuando está seleccionado.</summary>
public class BotonMenu : Button
{
    private bool _activo;

    public bool Activo
    {
        get => _activo;
        set
        {
            _activo = value;
            BackColor = value ? Tema.LateralActivo : Tema.Lateral;
            ForeColor = value ? Color.White : Tema.TextoLateral;
            Invalidate();
        }
    }

    public BotonMenu(string texto)
    {
        Text = texto;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseOverBackColor = Tema.LateralHover;
        FlatAppearance.MouseDownBackColor = Tema.LateralHover;
        UseVisualStyleBackColor = false;
        BackColor = Tema.Lateral;
        ForeColor = Tema.TextoLateral;
        Font = Tema.Fuente(10.5f);
        TextAlign = ContentAlignment.MiddleLeft;
        Padding = new Padding(26, 0, 0, 0);
        Height = 44;
        Cursor = Cursors.Hand;
        TabStop = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_activo)
        {
            using var pincel = new SolidBrush(Tema.Acento);
            e.Graphics.FillRectangle(pincel, 0, 0, 4, Height);
        }
    }
}
