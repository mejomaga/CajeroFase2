using System.Drawing;

namespace CajeroRed.Dashboard.UI;

/// <summary>Colores y fuentes del dashboard. Para cambiar el aspecto de TODA la
/// aplicación basta con editar este archivo.</summary>
public static class Tema
{
    public static readonly Color Fondo = ColorTranslator.FromHtml("#F1F5F9");
    public static readonly Color Tarjeta = Color.White;
    public static readonly Color Borde = ColorTranslator.FromHtml("#E2E8F0");

    public static readonly Color Lateral = ColorTranslator.FromHtml("#0F172A");
    public static readonly Color LateralHover = ColorTranslator.FromHtml("#1E293B");
    public static readonly Color LateralActivo = ColorTranslator.FromHtml("#1E3A5F");
    public static readonly Color TextoLateral = ColorTranslator.FromHtml("#CBD5E1");
    public static readonly Color TextoSuaveLateral = ColorTranslator.FromHtml("#94A3B8");

    public static readonly Color Acento = ColorTranslator.FromHtml("#3B82F6");
    public static readonly Color Texto = ColorTranslator.FromHtml("#0F172A");
    public static readonly Color TextoSuave = ColorTranslator.FromHtml("#64748B");
    public static Color TextoSecundario => Color.FromArgb(100, 116, 139); // Gris elegante corporativo

    // Colores con significado (requisito del documento)
    public static readonly Color Verde = ColorTranslator.FromHtml("#16A34A");    // correcto
    public static readonly Color Naranja = ColorTranslator.FromHtml("#F59E0B");  // advertencia
    public static readonly Color Rojo = ColorTranslator.FromHtml("#DC2626");     // falla

    public static Font Fuente(float tamano, FontStyle estilo = FontStyle.Regular)
        => new Font("Segoe UI", tamano, estilo);
}
