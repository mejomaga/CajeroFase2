using CajeroRed.Dashboard.Modelos;

namespace CajeroRed.Dashboard.Servicios;

/// <summary>Punto único donde se elige de dónde salen los datos y quién inició sesión.</summary>
public static class Contexto
{
    // >>> PARA CONECTAR LA BASE DE DATOS REAL, cambiar SOLO esta línea, por ejemplo:
    //     public static IServicioDatos Datos { get; set; } = new ServicioDatosSql("cadena de conexión");

    //public static IServicioDatos Datos { get; set; } = new DatosDemo();

    public static IServicioDatos Datos { get; set; } = new RepositorioSql("Server=MELQUI\\SQLEXPRESS;Database=BD_CajeroRed;Trusted_Connection=True;TrustServerCertificate=True;");

    /// <summary>Administrador que inició sesión (se llena al hacer login).</summary>
    public static Administrador? AdminActual { get; set; }
}
