namespace CajeroRed.Dashboard.Modelos;

// ============================================================================
//  MODELOS DE DATOS DEL DASHBOARD
//  Son clases "planas" (solo datos). El compañero que conecta SQL Server debe
//  llenar estas clases con lo que devuelvan las consultas / vistas / procedimientos.
// ============================================================================

/// <summary>Estados de un cajero. En SQL Server: 'En línea', 'Fuera de servicio',
/// 'Efectivo bajo', 'Desconectado' (columna Cajero.Estado).</summary>
public enum EstadoCajero { EnLinea, FueraDeServicio, EfectivoBajo, Desconectado }

/// <summary>Administrador del dashboard (tabla Administrador).</summary>
public class Administrador
{
    public int IdAdmin { get; set; }
    public string Usuario { get; set; } = "";
    /// <summary>"Admin" o "Supervisor".</summary>
    public string Rol { get; set; } = "Admin";
    public bool EsSupervisor => Rol == "Supervisor";
}

/// <summary>Un cajero de la red (tablas Cajero + InventarioCajero).</summary>
public class Cajero
{
    public string Codigo { get; set; } = "";          // ATM-01
    public string Nombre { get; set; } = "";
    public string Ubicacion { get; set; } = "";
    public string IP { get; set; } = "";
    public EstadoCajero Estado { get; set; } = EstadoCajero.Desconectado;
    public bool Inicializado { get; set; }
    public DateTime? UltimoHeartbeat { get; set; }
    public DateTime? UltimaTransaccion { get; set; }

    /// <summary>Billetes por denominación: clave = valor (200, 100, 50, 20, 10, 5, 1), valor = cantidad.</summary>
    public Dictionary<int, int> Inventario { get; set; } = new();

    /// <summary>Se calcula solo a partir del inventario.</summary>
    public decimal EfectivoTotal => Inventario.Sum(p => (decimal)p.Key * p.Value);

    /// <summary>Convierte el texto de la BD ('En línea', ...) al enum. Tolera problemas de tildes.</summary>
    public static EstadoCajero ParsearEstado(string? texto)
    {
        string t = (texto ?? "").Trim().ToLowerInvariant();
        if (t.StartsWith("en l")) return EstadoCajero.EnLinea;
        if (t.StartsWith("fuera")) return EstadoCajero.FueraDeServicio;
        if (t.StartsWith("efectivo")) return EstadoCajero.EfectivoBajo;
        return EstadoCajero.Desconectado;
    }

    /// <summary>Texto para mostrar en pantalla / guardar en la BD.</summary>
    public static string TextoDeEstado(EstadoCajero estado) => estado switch
    {
        EstadoCajero.EnLinea => "En línea",
        EstadoCajero.FueraDeServicio => "Fuera de servicio",
        EstadoCajero.EfectivoBajo => "Efectivo bajo",
        _ => "Desconectado"
    };
}

/// <summary>Usuario dueño de una cuenta (tablas Usuario + Tarjeta activa).</summary>
public class Usuario
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = "";
    /// <summary>Siempre enmascarada, por ejemplo "**** 4444". Nunca el número completo.</summary>
    public string TarjetaEnmascarada { get; set; } = "";
    public decimal Saldo { get; set; }
    public decimal LimiteDiario { get; set; }
    public decimal RetiradoHoy { get; set; }
    /// <summary>"Activo", "Bloqueado" o "Inactivo".</summary>
    public string Estado { get; set; } = "Activo";
    public DateTime? UltimoAcceso { get; set; }
    public string? UltimoCajero { get; set; }
    public decimal DisponibleHoy => Math.Max(0, LimiteDiario - RetiradoHoy);
}

/// <summary>Retiro o depósito (tablas Transaccion + TransaccionDetalle).</summary>
public class Transaccion
{
    public int IdTransaccion { get; set; }
    public DateTime Fecha { get; set; }
    public string CodigoCajero { get; set; } = "";
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = "";
    public string TarjetaEnmascarada { get; set; } = "";
    /// <summary>"RETIRO" o "DEPOSITO".</summary>
    public string Tipo { get; set; } = "";
    public decimal Monto { get; set; }
    public Dictionary<int, int> Desglose { get; set; } = new();
    /// <summary>"00" = aprobada. Otros códigos: 51, 61, 92, 93, 96...</summary>
    public string CodigoRespuesta { get; set; } = "00";
    public string? MotivoRechazo { get; set; }
    public bool Aprobada => CodigoRespuesta == "00";
}

/// <summary>Alerta del sistema (tabla Alerta).</summary>
public class Alerta
{
    public int IdAlerta { get; set; }
    public string Tipo { get; set; } = "";
    public string CodigoCajero { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public DateTime Fecha { get; set; }
    public bool Atendida { get; set; }
    public string? AtendidaPor { get; set; }
}

/// <summary>Indicadores del día para el Resumen y el encabezado (vistas vw_ResumenDiario y vw_EstadoCajeros).</summary>
public class ResumenDia
{
    public int CajerosEnLinea { get; set; }
    public int CajerosTotal { get; set; }
    public decimal EfectivoTotalRed { get; set; }
    public decimal TotalRetirado { get; set; }
    public decimal TotalDepositado { get; set; }
    public decimal PromedioDepositadoPorUsuario { get; set; }
    public int NumeroTransacciones { get; set; }
    public int CambiosPin { get; set; }
    public int OperacionesRechazadas { get; set; }
    public int AlertasActivas { get; set; }
}

/// <summary>Retiros y depósitos aprobados de una hora del día (gráfica de D-03).</summary>
public class PuntoHora
{
    /// <summary>Hora del día, de 0 a 23.</summary>
    public int Hora { get; set; }
    public decimal Retiros { get; set; }
    public decimal Depositos { get; set; }
}

public class AlertaRed
{
    public int IdAlerta { get; set; }
    public DateTime Fecha { get; set; }
    public string CodigoCajero { get; set; } = "";
    public string Tipo { get; set; } = ""; // "Efectivo Bajo", "Sin Comunicación", "Falla Hardware"
    public string Descripcion { get; set; } = "";
    public bool Atendida { get; set; }
}
