using CajeroRed.Dashboard.Modelos;

namespace CajeroRed.Dashboard.Servicios;

/// <summary>Datos ficticios para probar el dashboard sin base de datos.
/// Los números cambian un poco en cada consulta para simular "tiempo real".</summary>
public class DatosDemo : IServicioDatos
{
    // SHA-256 de la palabra "admin" (es el hash que trae el script SQL).
    private const string HashAdmin = "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918";

    private readonly Random _azar = new();
    private readonly List<Cajero> _cajeros;
    private readonly DateTime _ultimaConexionAtm05 = DateTime.Now.AddMinutes(-47);

    private decimal _retirado = 12480m;
    private decimal _depositado = 9120m;
    private int _transacciones = 38;
    private readonly List<PuntoHora> _horas = new();

    public string Descripcion => "Datos de demostración (sin base de datos)";

    private readonly List<Usuario> _usuarios = new()
{
    new Usuario { IdUsuario = 1, Nombre = "Juan Pérez", TarjetaEnmascarada = "**** 4521", Saldo = 5200.00m, LimiteDiario = 2000.00m, RetiradoHoy = 300.00m, Estado = "Activo" },
    new Usuario { IdUsuario = 2, Nombre = "María López", TarjetaEnmascarada = "**** 8890", Saldo = 8900.50m, LimiteDiario = 3000.00m, RetiradoHoy = 500.00m, Estado = "Activo" },
    new Usuario { IdUsuario = 3, Nombre = "Carlos Gómez", TarjetaEnmascarada = "**** 1122", Saldo = 1200.00m, LimiteDiario = 1000.00m, RetiradoHoy = 1000.00m, Estado = "Bloqueado" }
    };

    private readonly List<Transaccion> _transaccionesSimuladas = new()
    {
        new Transaccion { IdTransaccion = 101, Fecha = DateTime.Now.AddMinutes(-15), CodigoCajero = "ATM-01", IdUsuario = 1, NombreUsuario = "Juan Pérez", TarjetaEnmascarada = "**** 4521", Tipo = "RETIRO", Monto = 300.00m, CodigoRespuesta = "00" },
        new Transaccion { IdTransaccion = 102, Fecha = DateTime.Now.AddMinutes(-30), CodigoCajero = "ATM-02", IdUsuario = 2, NombreUsuario = "María López", TarjetaEnmascarada = "**** 8890", Tipo = "DEPOSITO", Monto = 500.00m, CodigoRespuesta = "00" },
        new Transaccion { IdTransaccion = 103, Fecha = DateTime.Now.AddMinutes(-45), CodigoCajero = "ATM-03", IdUsuario = 3, NombreUsuario = "Carlos Gómez", TarjetaEnmascarada = "**** 1122", Tipo = "RETIRO", Monto = 1500.00m, CodigoRespuesta = "51", MotivoRechazo = "Fondos insuficientes" },
        new Transaccion { IdTransaccion = 104, Fecha = DateTime.Now.AddHours(-1), CodigoCajero = "ATM-01", IdUsuario = 1, NombreUsuario = "Juan Pérez", TarjetaEnmascarada = "**** 4521", Tipo = "CAMBIO_PIN", Monto = 0m, CodigoRespuesta = "00" }
    };



    public DatosDemo()
    {
        var azarFijo = new Random(7);
        for (int hora = 8; hora <= 17; hora++)
        {
            _horas.Add(new PuntoHora
            {
                Hora = hora,
                Retiros = azarFijo.Next(8, 30) * 100,
                Depositos = azarFijo.Next(4, 22) * 100
            });
        }


        _cajeros = new List<Cajero>
        {
            Crear("ATM-01", "Cajero Sede Central", "Sede Central", "192.168.1.21", EstadoCajero.EnLinea,        40, 60, 80, 150, 200, 100, 200),
            Crear("ATM-02", "Cajero Sede Norte",   "Sede Norte",   "192.168.1.22", EstadoCajero.EnLinea,        30, 50, 60, 100, 100,  80, 150),
            Crear("ATM-03", "Cajero Sede Sur",     "Sede Sur",     "192.168.1.23", EstadoCajero.EfectivoBajo,    0,  2,  5,  10,  10,  10,  20),
            Crear("ATM-04", "Cajero Oficinas",     "Oficinas",     "192.168.1.24", EstadoCajero.FueraDeServicio,20, 30, 40,  60,  60,  50, 100),
            Crear("ATM-05", "Cajero Bodega",       "Bodega",       "192.168.1.25", EstadoCajero.Desconectado,   25, 40, 50,  80, 100,  60, 100),
        };


    }

    private static Cajero Crear(string codigo, string nombre, string ubicacion, string ip, EstadoCajero estado,
        int b200, int b100, int b50, int b20, int b10, int b5, int b1)
    {
        return new Cajero
        {
            Codigo = codigo,
            Nombre = nombre,
            Ubicacion = ubicacion,
            IP = ip,
            Estado = estado,
            Inicializado = true,
            UltimaTransaccion = DateTime.Now.AddMinutes(-new Random().Next(1, 40)),
            Inventario = new Dictionary<int, int>
            {
                [200] = b200, [100] = b100, [50] = b50, [20] = b20, [10] = b10, [5] = b5, [1] = b1
            }
        };
    }

    public async Task<Administrador?> IniciarSesionAdminAsync(string usuario, string contrasena)
    {
        await Task.Delay(300); // simula la consulta a la base de datos

        bool usuarioOk = usuario.Trim().Equals("admin", StringComparison.OrdinalIgnoreCase);
        bool claveOk = contrasena == HashAdmin;

        return usuarioOk && claveOk
            ? new Administrador { IdAdmin = 1, Usuario = "admin", Rol = "Admin" }
            : null;
    }

    public async Task<ResumenDia> ObtenerResumenAsync()
    {
        await Task.Delay(120);

        // De vez en cuando "ocurre" una operación nueva para ver el refresco automático.
        if (_azar.Next(0, 10) < 4)
        {
            _retirado += _azar.Next(1, 6) * 100;
            _depositado += _azar.Next(1, 4) * 100;
            _transacciones += 2;
        }

        return new ResumenDia
        {
            CajerosEnLinea = _cajeros.Count(c => c.Estado is EstadoCajero.EnLinea or EstadoCajero.EfectivoBajo),
            CajerosTotal = _cajeros.Count,
            EfectivoTotalRed = _cajeros.Sum(c => c.EfectivoTotal),
            TotalRetirado = _retirado,
            TotalDepositado = _depositado,
            PromedioDepositadoPorUsuario = _depositado / 8,
            NumeroTransacciones = _transacciones,
            CambiosPin = 3,
            OperacionesRechazadas = 3,
            AlertasActivas = 3
        };
    }

    public async Task<List<PuntoHora>> ObtenerSerieHorariaAsync()
    {
        await Task.Delay(120);

        // La última hora crece de vez en cuando para simular movimiento en tiempo real.
        if (_azar.Next(0, 10) < 3)
        {
            _horas[^1].Retiros += 100;
            _horas[^1].Depositos += 100;
        }

        return _horas
            .Select(p => new PuntoHora { Hora = p.Hora, Retiros = p.Retiros, Depositos = p.Depositos })
            .ToList();
    }

    public async Task<List<Cajero>> ObtenerCajerosAsync()
    {
        await Task.Delay(120);

        foreach (var c in _cajeros)
        {
            c.UltimoHeartbeat = c.Estado == EstadoCajero.Desconectado
                ? _ultimaConexionAtm05
                : DateTime.Now.AddSeconds(-_azar.Next(0, 10));
        }
        return _cajeros.ToList();
    }

    public async Task<bool> InicializarCajeroAsync(string codigoCajero, Dictionary<int, int> desglose)
    {
        await Task.Delay(150);

        // Calcular el total del efectivo a cargar
        decimal total = desglose.Sum(kvp => kvp.Key * kvp.Value);

        // Regla RN-02 / D-05: Máximo Q 10,000.00 para inicializar
        if (total > 10000m) return false;

        var cajero = _cajeros.FirstOrDefault(c => c.Codigo == codigoCajero);
        if (cajero != null)
        {
            cajero.Inicializado = true;
            cajero.Estado = EstadoCajero.EnLinea;
            foreach (var kvp in desglose)
            {
                cajero.Inventario[kvp.Key] = cajero.Inventario.ContainsKey(kvp.Key)
                    ? cajero.Inventario[kvp.Key] + kvp.Value
                    : kvp.Value;
            }
            return true;
        }
        return false;
    }

    public async Task<bool> AgregarEfectivoCajeroAsync(string codigoCajero, Dictionary<int, int> desglose)
    {
        await Task.Delay(150);

        decimal totalAdicional = desglose.Sum(kvp => kvp.Key * kvp.Value);
        var cajero = _cajeros.FirstOrDefault(c => c.Codigo == codigoCajero);

        if (cajero == null || !cajero.Inicializado) return false; // Debe estar inicializado

        // Regla RN-03 / D-05: El efectivo total nunca puede superar Q 30,000.00
        if (cajero.EfectivoTotal + totalAdicional > 30000m) return false;

        foreach (var kvp in desglose)
        {
            cajero.Inventario[kvp.Key] = cajero.Inventario.ContainsKey(kvp.Key)
                ? cajero.Inventario[kvp.Key] + kvp.Value
                : kvp.Value;
        }
        return true;
    }

    public async Task<bool> CambiarEstadoCajeroAsync(string codigoCajero, string nuevoEstadoStr)
    {
        await Task.Delay(100);
        var cajero = _cajeros.FirstOrDefault(c => c.Codigo == codigoCajero);
        if (cajero != null)
        {
            if (Enum.TryParse<EstadoCajero>(nuevoEstadoStr, out var estadoParsed))
            {
                cajero.Estado = estadoParsed;
            }
            else if (nuevoEstadoStr.Contains("Fuera", StringComparison.OrdinalIgnoreCase))
            {
                cajero.Estado = EstadoCajero.FueraDeServicio;
            }
            else
            {
                cajero.Estado = EstadoCajero.EnLinea;
            }
            return true;
        }
        return false;
    }

    public async Task<bool> GuardarCajeroAsync(string codigo, string nombre, string ubicacion, string ip)
    {
        await Task.Delay(150);
        var existente = _cajeros.FirstOrDefault(c => c.Codigo == codigo);
        if (existente != null)
        {
            existente.Nombre = nombre;
            existente.Ubicacion = ubicacion;
            existente.IP = ip;
        }
        else
        {
            _cajeros.Add(new Cajero
            {
                Codigo = codigo,
                Nombre = nombre,
                Ubicacion = ubicacion,
                IP = ip,
                Estado = EstadoCajero.EnLinea,
                Inicializado = false,
                UltimaTransaccion = DateTime.Now,
                Inventario = new Dictionary<int, int> { { 200, 0 }, { 100, 0 }, { 50, 0 }, { 20, 0 }, { 10, 0 }, { 5, 0 }, { 1, 0 } }
            });
        }
        return true;
    }

    public async Task<List<Usuario>> ObtenerUsuariosAsync()
    {
        await Task.Delay(100);
        return _usuarios.ToList();
    }

    public async Task<bool> CrearUsuarioAsync(string nombre, string tarjeta, string pin, decimal saldo, decimal limiteDiario)
    {
        await Task.Delay(150);
        string enmascarada = tarjeta.Length >= 4 ? "**** " + tarjeta[^4..] : "**** 0000";
        _usuarios.Add(new Usuario
        {
            IdUsuario = _usuarios.Count > 0 ? _usuarios.Max(u => u.IdUsuario) + 1 : 1,
            Nombre = nombre,
            TarjetaEnmascarada = enmascarada,
            Saldo = saldo,
            LimiteDiario = limiteDiario,
            RetiradoHoy = 0m,
            Estado = "Activo"
        });
        return true;
    }

    public async Task<bool> ModificarUsuarioAsync(int idUsuario, string nombre, decimal saldo, decimal limiteDiario)
    {
        await Task.Delay(150);
        var u = _usuarios.FirstOrDefault(x => x.IdUsuario == idUsuario);
        if (u != null)
        {
            u.Nombre = nombre;
            u.Saldo = saldo;
            u.LimiteDiario = limiteDiario;
            return true;
        }
        return false;
    }

    public async Task<bool> CambiarTarjetaUsuarioAsync(int idUsuario, string nuevaTarjeta)
    {
        await Task.Delay(150);
        var u = _usuarios.FirstOrDefault(x => x.IdUsuario == idUsuario);
        if (u != null)
        {
            // Cambia la tarjeta conservando el historial de lo retirado en el día (RN-04 / D-10)
            u.TarjetaEnmascarada = nuevaTarjeta.Length >= 4 ? "**** " + nuevaTarjeta[^4..] : "**** 9999";
            return true;
        }
        return false;
    }

    public async Task<bool> CambiarEstadoUsuarioAsync(int idUsuario, string nuevoEstado)
    {
        await Task.Delay(100);
        var u = _usuarios.FirstOrDefault(x => x.IdUsuario == idUsuario);
        if (u != null)
        {
            u.Estado = nuevoEstado; // "Activo" o "Bloqueado"
            return true;
        }
        return false;
    }

    public async Task<List<Transaccion>> ObtenerTransaccionesAsync(string? codigoCajero = null, string? tipo = null)
    {
        await Task.Delay(120);
        var query = _transaccionesSimuladas.AsEnumerable();
        if (!string.IsNullOrEmpty(codigoCajero))
            query = query.Where(t => t.CodigoCajero.Equals(codigoCajero, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(tipo))
            query = query.Where(t => t.Tipo.Equals(tipo, StringComparison.OrdinalIgnoreCase));
        return query.OrderByDescending(t => t.Fecha).ToList();
    }

    public async Task<List<Transaccion>> ObtenerDepositosAsync(string? codigoCajero = null)
    {
        await Task.Delay(120);
        var query = _transaccionesSimuladas.Where(t => t.Tipo.Equals("DEPOSITO", StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(codigoCajero))
            query = query.Where(t => t.CodigoCajero.Equals(codigoCajero, StringComparison.OrdinalIgnoreCase));
        return query.OrderByDescending(t => t.Fecha).ToList();
    }

    public async Task<List<Transaccion>> ObtenerRetirosAsync(string? codigoCajero = null, bool? soloRechazados = null)
    {
        await Task.Delay(120);
        var query = _transaccionesSimuladas.Where(t => t.Tipo.Equals("RETIRO", StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(codigoCajero))
            query = query.Where(t => t.CodigoCajero.Equals(codigoCajero, StringComparison.OrdinalIgnoreCase));
        if (soloRechazados.HasValue && soloRechazados.Value)
            query = query.Where(t => !t.Aprobada);
        return query.OrderByDescending(t => t.Fecha).ToList();
    }

    public async Task<List<Transaccion>> ObtenerCambiosPinAsync()
    {
        await Task.Delay(120);
        return _transaccionesSimuladas
            .Where(t => t.Tipo.Equals("CAMBIO_PIN", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(t => t.Fecha)
            .ToList();
    }

    public async Task<string> RestablecerPinTemporalAsync(int idUsuario)
    {
        await Task.Delay(150);
        var usuario = _usuarios.FirstOrDefault(u => u.IdUsuario == idUsuario);
        if (usuario == null) return "";

        // Generar un PIN temporal seguro de 4 dígitos aleatorios
        string pinTemporal = new Random().Next(1000, 9999).ToString();

        // Aquí tu compañero guardará este PIN temporal en la BD y activará la bandera 'RequiereCambioPin = 1'
        return pinTemporal;
    }

    private readonly List<AlertaRed> _alertasSimuladas = new()
    {
        new AlertaRed { IdAlerta = 1, Fecha = DateTime.Now.AddMinutes(-10), CodigoCajero = "ATM-02", Tipo = "Efectivo Bajo", Descripcion = "El cajero cuenta con menos del 15% de su capacidad en billetes de Q100.", Atendida = false },
        new AlertaRed { IdAlerta = 2, Fecha = DateTime.Now.AddHours(-2), CodigoCajero = "ATM-03", Tipo = "Sin Comunicación", Descripcion = "Pérdida temporal de enlace con el servidor central.", Atendida = true }
    };

    public async Task<List<AlertaRed>> ObtenerAlertasAsync()
    {
        await Task.Delay(100);
        return _alertasSimuladas.OrderByDescending(a => a.Fecha).ToList();
    }

    public async Task<bool> AtenderAlertaAsync(int idAlerta)
    {
        await Task.Delay(100);
        var alerta = _alertasSimuladas.FirstOrDefault(a => a.IdAlerta == idAlerta);
        if (alerta != null)
        {
            alerta.Atendida = true;
            return true;
        }
        return false;
    }

    // Métodos mock para satisfacer la interfaz IServicioDatos
    public async Task<List<object>> ObtenerBitacoraAsync(string? categoria = null, DateTime? inicio = null, DateTime? fin = null)
    {
        return await Task.FromResult(new List<object>());
    }

    public async Task<Dictionary<string, string>> ObtenerConfiguracionAsync()
    {
        return await Task.FromResult(new Dictionary<string, string>
        {
            { "Puerto", "5000" },
            { "UmbralEfectivoBajo", "1000.00" },
            { "VigenciaTokenMinutos", "2" },
            { "MaximoCajero", "30000.00" }
        });
    }

    public async Task<bool> GuardarConfiguracionAsync(string nombre, string valor)
    {
        return await Task.FromResult(true);
    }
    public async Task<bool> DarDeBajaCajeroAsync(string codigoCajero)
    {
        // Simulación para que la interfaz se cumpla
        await Task.Delay(100);
        var cajero = _cajeros.FirstOrDefault(c => c.Codigo == codigoCajero);
        if (cajero != null)
        {
            cajero.Estado = EstadoCajero.Desconectado;
            cajero.Inicializado = false;
            return true;
        }
        return false;
    }
}
