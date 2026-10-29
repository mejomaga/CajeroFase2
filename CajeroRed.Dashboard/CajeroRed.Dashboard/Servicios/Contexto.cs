using CajeroRed.Dashboard.Modelos;

namespace CajeroRed.Dashboard.Servicios;

/// <summary>Punto único donde se elige de dónde salen los datos y quién inició sesión.</summary>
public static class Contexto
{
    // Conexión a la base de datos SQL Server local (.\SQLEXPRESS)
    public static IServicioDatos Datos { get; set; } = new RepositorioSql(
        Environment.GetEnvironmentVariable("CADENA_CONEXION_CAJERO") 
        ?? @"Server=.\SQLEXPRESS;Database=BD_CajeroRed;Trusted_Connection=True;TrustServerCertificate=True;");

    /// <summary>Administrador que inició sesión (se llena al hacer login).</summary>
    public static Administrador? AdminActual { get; set; }
}
