using System.Security.Cryptography;
using System.Text;

namespace CajeroRed.Dashboard.Servicios;

public static class Seguridad
{
    /// <summary>SHA-256 del texto, en hexadecimal minúscula (mismo formato que los hashes
    /// sembrados en la base de datos).</summary>
    public static string Sha256Hex(string texto)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
