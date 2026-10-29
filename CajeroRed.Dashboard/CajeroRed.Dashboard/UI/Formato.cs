using System.Globalization;

namespace CajeroRed.Dashboard.UI;

/// <summary>Formatos de presentación (el documento pide montos con formato Q 0,000.00).</summary>
public static class Formato
{
    private static readonly CultureInfo Cultura = CultureInfo.InvariantCulture;

    public static string Quetzales(decimal valor) => "Q " + valor.ToString("N2", Cultura);

    public static string Numero(double valor) => valor.ToString("N0", Cultura);

    public static string Hora(DateTime? fecha) => fecha.HasValue ? fecha.Value.ToString("HH:mm:ss") : "—";
}
