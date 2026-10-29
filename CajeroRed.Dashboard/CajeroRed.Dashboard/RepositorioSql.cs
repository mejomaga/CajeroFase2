using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using CajeroRed.Dashboard.Modelos;

namespace CajeroRed.Dashboard.Servicios;

public class RepositorioSql : IServicioDatos
{
    private readonly string _cadenaConexion;

    public string Descripcion => "SQL Server: BD_CajeroRed (Conectado)";

    public RepositorioSql(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<Administrador?> IniciarSesionAdminAsync(string usuario, string contrasenaHash)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        using var comando = new SqlCommand("SELECT IdAdmin, Usuario, Rol, Estado FROM Administrador WHERE Usuario = @Usuario AND ContrasenaHash = @Hash AND Estado = 'Activo'", conexion);

        comando.Parameters.AddWithValue("@Usuario", usuario);
        comando.Parameters.AddWithValue("@Hash", contrasenaHash);

        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Administrador
            {
                IdAdmin = reader.GetInt32(0),
                Usuario = reader.GetString(1),
                Rol = reader.GetString(2)
            };
        }
        return null;
    }

    public async Task<ResumenDia> ObtenerResumenAsync()
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        using var comando = new SqlCommand(@"
            SELECT 
                (SELECT COUNT(*) FROM Cajero WHERE Estado = 'En línea') AS CajerosEnLinea,
                (SELECT COUNT(*) FROM Cajero) AS CajerosTotal,
                ISNULL((SELECT SUM(i.Valor * i.Cantidad) FROM InventarioCajero i), 0) AS EfectivoTotalRed,
                ISNULL((SELECT SUM(Monto) FROM Transaccion WHERE Tipo = 'RETIRO' AND CodigoRespuesta = '00' AND CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE)), 0) AS TotalRetirado,
                ISNULL((SELECT SUM(Monto) FROM Transaccion WHERE Tipo = 'DEPOSITO' AND CodigoRespuesta = '00' AND CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE)), 0) AS TotalDepositado,
                0 AS PromedioDepositadoPorUsuario,
                (SELECT COUNT(*) FROM Transaccion WHERE CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE)) AS NumeroTransacciones,
                (SELECT COUNT(*) FROM CambioPin WHERE CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE)) AS CambiosPin,
                (SELECT COUNT(*) FROM Transaccion WHERE CodigoRespuesta <> '00' AND CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE)) AS OperacionesRechazadas,
                (SELECT COUNT(*) FROM Alerta WHERE Atendida = 0) AS AlertasActivas
        ", conexion);

        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();

        var resumen = new ResumenDia();
        if (await reader.ReadAsync())
        {
            resumen.CajerosEnLinea = reader.GetInt32(0);
            resumen.CajerosTotal = reader.GetInt32(1);
            resumen.EfectivoTotalRed = Convert.ToDecimal(reader[2]);
            resumen.TotalRetirado = Convert.ToDecimal(reader[3]);
            resumen.TotalDepositado = Convert.ToDecimal(reader[4]);
            resumen.PromedioDepositadoPorUsuario = Convert.ToDecimal(reader[5]);
            resumen.NumeroTransacciones = reader.GetInt32(6);
            resumen.CambiosPin = reader.GetInt32(7);
            resumen.OperacionesRechazadas = reader.GetInt32(8);
            resumen.AlertasActivas = reader.GetInt32(9);
        }
        return resumen;
    }

    public async Task<List<Cajero>> ObtenerCajerosAsync()
    {
        var lista = new List<Cajero>();
        using var conexion = new SqlConnection(_cadenaConexion);
        using var comando = new SqlCommand("SELECT CodigoCajero, Nombre, Ubicacion, IP, Estado, Inicializado, UltimoHeartbeat FROM Cajero", conexion);

        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            string estadoStr = reader.GetString(4);
            var estadoEnum = Cajero.ParsearEstado(estadoStr);

            lista.Add(new Cajero
            {
                Codigo = reader.GetString(0),
                Nombre = reader.GetString(1),
                Ubicacion = reader.GetString(2),
                IP = reader.GetString(3),
                Estado = estadoEnum,
                Inicializado = reader.GetBoolean(5),
                UltimoHeartbeat = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6)
            });
        }
        return lista;
    }

    public async Task<List<PuntoHora>> ObtenerSerieHorariaAsync()
    {
        var lista = new List<PuntoHora>();
        using var conexion = new SqlConnection(_cadenaConexion);
        using var comando = new SqlCommand(@"
        SELECT 
            DATEPART(HOUR, Fecha) AS Hora,
            SUM(CASE WHEN Tipo = 'RETIRO' AND CodigoRespuesta = '00' THEN Monto ELSE 0 END) AS Retiros,
            SUM(CASE WHEN Tipo = 'DEPOSITO' AND CodigoRespuesta = '00' THEN Monto ELSE 0 END) AS Depositos
        FROM Transaccion
        WHERE CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE)
        GROUP BY DATEPART(HOUR, Fecha)
        ORDER BY Hora", conexion);

        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new PuntoHora
            {
                Hora = reader.GetInt32(0),
                Retiros = Convert.ToDecimal(reader[1]),
                Depositos = Convert.ToDecimal(reader[2])
            });
        }
        return lista;
    }

    public async Task<bool> InicializarCajeroAsync(string codigoCajero, Dictionary<int, int> desglose)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        using var comando = new SqlCommand("sp_InicializarCajero", conexion) { CommandType = CommandType.StoredProcedure };

        comando.Parameters.AddWithValue("@CodigoCajero", codigoCajero);
        comando.Parameters.AddWithValue("@IdAdmin", 1);
        comando.Parameters.AddWithValue("@Total", 10000.00m);

        // Mandamos los billetes del desglose que recibe el método
        comando.Parameters.AddWithValue("@B200", desglose.ContainsKey(200) ? desglose[200] : 0);
        comando.Parameters.AddWithValue("@B100", desglose.ContainsKey(100) ? desglose[100] : 0);
        comando.Parameters.AddWithValue("@B50", desglose.ContainsKey(50) ? desglose[50] : 0);
        comando.Parameters.AddWithValue("@B20", desglose.ContainsKey(20) ? desglose[20] : 0);
        comando.Parameters.AddWithValue("@B10", desglose.ContainsKey(10) ? desglose[10] : 0);
        comando.Parameters.AddWithValue("@B5", desglose.ContainsKey(5) ? desglose[5] : 0);
        comando.Parameters.AddWithValue("@B1", desglose.ContainsKey(1) ? desglose[1] : 0);

        var paramRespuesta = new SqlParameter("@CodigoRespuesta", SqlDbType.NVarChar, 5) { Direction = ParameterDirection.Output };
        var paramMensaje = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
        comando.Parameters.Add(paramRespuesta);
        comando.Parameters.Add(paramMensaje);

        await conexion.OpenAsync();
        await comando.ExecuteNonQueryAsync();

        return paramRespuesta.Value?.ToString() == "00";
    }

    public async Task<bool> AgregarEfectivoCajeroAsync(string codigoCajero, Dictionary<int, int> desglose)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        using var comando = new SqlCommand("sp_AgregarEfectivo", conexion) { CommandType = CommandType.StoredProcedure };

        comando.Parameters.AddWithValue("@CodigoCajero", codigoCajero);
        comando.Parameters.AddWithValue("@IdAdmin", 1);
        comando.Parameters.AddWithValue("@TotalAgregar", 5000.00m);

        comando.Parameters.AddWithValue("@B200", desglose.ContainsKey(200) ? desglose[200] : 0);
        comando.Parameters.AddWithValue("@B100", desglose.ContainsKey(100) ? desglose[100] : 0);
        comando.Parameters.AddWithValue("@B50", desglose.ContainsKey(50) ? desglose[50] : 0);
        comando.Parameters.AddWithValue("@B20", desglose.ContainsKey(20) ? desglose[20] : 0);
        comando.Parameters.AddWithValue("@B10", desglose.ContainsKey(10) ? desglose[10] : 0);
        comando.Parameters.AddWithValue("@B5", desglose.ContainsKey(5) ? desglose[5] : 0);
        comando.Parameters.AddWithValue("@B1", desglose.ContainsKey(1) ? desglose[1] : 0);

        var paramRespuesta = new SqlParameter("@CodigoRespuesta", SqlDbType.NVarChar, 5) { Direction = ParameterDirection.Output };
        var paramMensaje = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
        comando.Parameters.Add(paramRespuesta);
        comando.Parameters.Add(paramMensaje);

        await conexion.OpenAsync();
        await comando.ExecuteNonQueryAsync();

        return paramRespuesta.Value?.ToString() == "00";
    }

    public async Task<bool> CambiarEstadoCajeroAsync(string codigoCajero, string nuevoEstado)
    {
        using var conexion = new SqlConnection(_cadenaConexion);

        // Forzamos el texto exacto que pide la BD de Juan
        string estadoBD = nuevoEstado;
        if (nuevoEstado == "FueraDeServicio") estadoBD = "Fuera de servicio";
        if (nuevoEstado == "EnLinea") estadoBD = "En línea";
        if (nuevoEstado == "EfectivoBajo") estadoBD = "Efectivo bajo";

        using var comando = new SqlCommand("UPDATE Cajero SET Estado = @Estado WHERE CodigoCajero = @Codigo", conexion);
        comando.Parameters.AddWithValue("@Estado", estadoBD);
        comando.Parameters.AddWithValue("@Codigo", codigoCajero);

        await conexion.OpenAsync();
        int filas = await comando.ExecuteNonQueryAsync();
        return filas > 0;
    }

    public async Task<bool> GuardarCajeroAsync(string codigo, string nombre, string ubicacion, string ip)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        using var comando = new SqlCommand("IF EXISTS(SELECT 1 FROM Cajero WHERE CodigoCajero = @Codigo) UPDATE Cajero SET Nombre = @Nombre, Ubicacion = @Ubicacion, IP = @Ip WHERE CodigoCajero = @Codigo ELSE INSERT INTO Cajero (CodigoCajero, Nombre, Ubicacion, IP, Estado) VALUES (@Codigo, @Nombre, @Ubicacion, @Ip, 'Desconectado')", conexion);

        comando.Parameters.AddWithValue("@Codigo", codigo);
        comando.Parameters.AddWithValue("@Nombre", nombre);
        comando.Parameters.AddWithValue("@Ubicacion", ubicacion);
        comando.Parameters.AddWithValue("@Ip", ip);

        await conexion.OpenAsync();
        int filas = await comando.ExecuteNonQueryAsync();
        return filas > 0;
    }

    public async Task<List<Usuario>> ObtenerUsuariosAsync()
    {
        var lista = new List<Usuario>();
        using var conexion = new SqlConnection(_cadenaConexion);
        string query = @"
        SELECT u.IdUsuario, u.Nombre, t.NumeroTarjeta, u.Saldo, u.LimiteDiario, u.Estado,
               ISNULL((SELECT SUM(Monto) FROM Transaccion WHERE IdUsuario = u.IdUsuario AND Tipo = 'RETIRO' AND CodigoRespuesta = '00' AND CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE)), 0) AS RetiradoHoy
        FROM Usuario u
        LEFT JOIN Tarjeta t ON u.IdUsuario = t.IdUsuario AND t.Activa = 1";

        using var comando = new SqlCommand(query, conexion);
        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            string tarjeta = reader.IsDBNull(2) ? "Sin tarjeta" : reader.GetString(2);
            lista.Add(new Usuario
            {
                IdUsuario = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                TarjetaEnmascarada = tarjeta.Length >= 4 ? "**** " + tarjeta.Substring(tarjeta.Length - 4) : tarjeta,
                Saldo = reader.GetDecimal(3),
                LimiteDiario = reader.GetDecimal(4),
                Estado = reader.GetString(5),
                RetiradoHoy = reader.GetDecimal(6)
            });
        }
        return lista;
    }

    public async Task<bool> CrearUsuarioAsync(string nombre, string tarjeta, string pin, decimal saldo, decimal limiteDiario)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.OpenAsync();
        using var transaccion = conexion.BeginTransaction();
        try
        {
            string sqlUsuario = "INSERT INTO Usuario (Nombre, Saldo, LimiteDiario, Estado) OUTPUT INSERTED.IdUsuario VALUES (@Nom, @Saldo, @Limite, 'Activo')";
            using var cmdUser = new SqlCommand(sqlUsuario, conexion, transaccion);
            cmdUser.Parameters.AddWithValue("@Nom", nombre);
            cmdUser.Parameters.AddWithValue("@Saldo", saldo);
            cmdUser.Parameters.AddWithValue("@Limite", limiteDiario);
            int idUsuario = (int)await cmdUser.ExecuteScalarAsync();

            string salt = "salt123";
            string hash = Seguridad.Sha256Hex(pin + salt);

            string sqlTarjeta = "INSERT INTO Tarjeta (IdUsuario, NumeroTarjeta, PinHash, PinSalt, Activa) VALUES (@IdU, @Num, @Hash, @Salt, 1)";
            using var cmdTarjeta = new SqlCommand(sqlTarjeta, conexion, transaccion);
            cmdTarjeta.Parameters.AddWithValue("@IdU", idUsuario);
            cmdTarjeta.Parameters.AddWithValue("@Num", tarjeta);
            cmdTarjeta.Parameters.AddWithValue("@Hash", hash);
            cmdTarjeta.Parameters.AddWithValue("@Salt", salt);
            await cmdTarjeta.ExecuteNonQueryAsync();

            transaccion.Commit();
            return true;
        }
        catch
        {
            transaccion.Rollback();
            return false;
        }
    }

    public async Task<bool> ModificarUsuarioAsync(int idUsuario, string nombre, decimal saldo, decimal limiteDiario)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        using var comando = new SqlCommand("UPDATE Usuario SET Nombre = @Nom, Saldo = @Saldo, LimiteDiario = @Limite WHERE IdUsuario = @Id", conexion);
        comando.Parameters.AddWithValue("@Nom", nombre);
        comando.Parameters.AddWithValue("@Saldo", saldo);
        comando.Parameters.AddWithValue("@Limite", limiteDiario);
        comando.Parameters.AddWithValue("@Id", idUsuario);

        await conexion.OpenAsync();
        return await comando.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> CambiarTarjetaUsuarioAsync(int idUsuario, string nuevaTarjeta)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.OpenAsync();
        using var transaccion = conexion.BeginTransaction();

        try
        {
            using var cmdBaja = new SqlCommand("UPDATE Tarjeta SET Activa = 0, FechaBaja = GETDATE() WHERE IdUsuario = @Id AND Activa = 1", conexion, transaccion);
            cmdBaja.Parameters.AddWithValue("@Id", idUsuario);
            await cmdBaja.ExecuteNonQueryAsync();

            using var cmdInsert = new SqlCommand(@"
            INSERT INTO Tarjeta (IdUsuario, NumeroTarjeta, PinHash, PinSalt, Activa)
            SELECT @Id, @Nueva, PinHash, PinSalt, 1 FROM Tarjeta WHERE IdUsuario = @Id ORDER BY IdTarjeta DESC", conexion, transaccion);
            cmdInsert.Parameters.AddWithValue("@Id", idUsuario);
            cmdInsert.Parameters.AddWithValue("@Nueva", nuevaTarjeta);
            await cmdInsert.ExecuteNonQueryAsync();

            transaccion.Commit();
            return true;
        }
        catch
        {
            transaccion.Rollback();
            return false;
        }
    }

    public async Task<bool> CambiarEstadoUsuarioAsync(int idUsuario, string nuevoEstado)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        using var comando = new SqlCommand("UPDATE Usuario SET Estado = @Estado WHERE IdUsuario = @Id", conexion);
        comando.Parameters.AddWithValue("@Estado", nuevoEstado);
        comando.Parameters.AddWithValue("@Id", idUsuario);

        await conexion.OpenAsync();
        return await comando.ExecuteNonQueryAsync() > 0;
    }

    public async Task<List<Transaccion>> ObtenerTransaccionesAsync(string? codigoCajero = null, string? tipo = null)
    {
        var lista = new List<Transaccion>();
        using var conexion = new SqlConnection(_cadenaConexion);

        string query = @"
        SELECT t.IdTransaccion, t.Fecha, t.CodigoCajero, t.IdUsuario, 
               u.Nombre AS NombreUsuario, t.Tipo, t.Monto, t.CodigoRespuesta, t.MotivoRechazo,
               (SELECT TOP 1 NumeroTarjeta FROM Tarjeta WHERE IdUsuario = t.IdUsuario AND (Activa = 1 OR FechaBaja >= t.Fecha)) AS Tarjeta
        FROM Transaccion t
        INNER JOIN Usuario u ON t.IdUsuario = u.IdUsuario
        WHERE 1=1";

        if (!string.IsNullOrEmpty(codigoCajero)) query += " AND t.CodigoCajero = @Cajero";
        if (!string.IsNullOrEmpty(tipo)) query += " AND t.Tipo = @Tipo";
        query += " ORDER BY t.Fecha DESC";

        using var comando = new SqlCommand(query, conexion);
        if (!string.IsNullOrEmpty(codigoCajero)) comando.Parameters.AddWithValue("@Cajero", codigoCajero);
        if (!string.IsNullOrEmpty(tipo)) comando.Parameters.AddWithValue("@Tipo", tipo);

        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            string tarjeta = reader.IsDBNull(9) ? "" : reader.GetString(9);
            lista.Add(new Transaccion
            {
                IdTransaccion = reader.GetInt32(0),
                Fecha = reader.GetDateTime(1),
                CodigoCajero = reader.GetString(2),
                IdUsuario = reader.GetInt32(3),
                NombreUsuario = reader.GetString(4),
                Tipo = reader.GetString(5),
                Monto = Convert.ToDecimal(reader[6]),
                CodigoRespuesta = reader.GetString(7),
                MotivoRechazo = reader.IsDBNull(8) ? null : reader.GetString(8),
                TarjetaEnmascarada = tarjeta.Length >= 4 ? "**** " + tarjeta.Substring(tarjeta.Length - 4) : tarjeta
            });
        }
        return lista;
    }

    public async Task<List<Transaccion>> ObtenerDepositosAsync(string? codigoCajero = null)
    {
        var todas = await ObtenerTodasLasTransaccionesAsync(codigoCajero);
        return todas.Where(t => t.Tipo == "DEPOSITO").ToList();
    }

    public async Task<List<Transaccion>> ObtenerRetirosAsync(string? codigoCajero = null, bool? soloRechazados = null)
    {
        var todas = await ObtenerTodasLasTransaccionesAsync(codigoCajero);
        var retiros = todas.Where(t => t.Tipo == "RETIRO");

        if (soloRechazados.HasValue && soloRechazados.Value)
            retiros = retiros.Where(t => !t.Aprobada);

        return retiros.ToList();
    }

    public async Task<List<Transaccion>> ObtenerCambiosPinAsync()
    {
        var lista = new List<Transaccion>();
        using var conexion = new SqlConnection(_cadenaConexion);
        // Buscamos los cambios de PIN en la bitácora o en la tabla de CambioPin
        string query = @"
        SELECT c.IdCambio, c.Fecha, c.CodigoCajero, c.IdUsuario, u.Nombre, c.Resultado
        FROM CambioPin c
        INNER JOIN Usuario u ON c.IdUsuario = u.IdUsuario
        ORDER BY c.Fecha DESC";

        using var comando = new SqlCommand(query, conexion);
        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new Transaccion
            {
                IdTransaccion = reader.GetInt32(0),
                Fecha = reader.GetDateTime(1),
                CodigoCajero = reader.IsDBNull(2) ? "Admin" : reader.GetString(2),
                IdUsuario = reader.GetInt32(3),
                NombreUsuario = reader.GetString(4),
                Tipo = "CAMBIO_PIN",
                Monto = 0,
                CodigoRespuesta = reader.GetString(5) == "Exitosa" ? "00" : "96",
                TarjetaEnmascarada = ""
            });
        }
        return lista;
    }

    private async Task<List<Transaccion>> ObtenerTodasLasTransaccionesAsync(string? codigoCajero)
    {
        var lista = new List<Transaccion>();
        using var conexion = new SqlConnection(_cadenaConexion);

        string query = @"
        SELECT t.IdTransaccion, t.Fecha, t.CodigoCajero, t.IdUsuario, u.Nombre, t.Tipo, t.Monto, t.CodigoRespuesta, t.MotivoRechazo,
               (SELECT TOP 1 NumeroTarjeta FROM Tarjeta WHERE IdUsuario = t.IdUsuario AND (Activa = 1 OR FechaBaja >= t.Fecha)) AS Tarjeta
        FROM Transaccion t
        INNER JOIN Usuario u ON t.IdUsuario = u.IdUsuario
        WHERE 1=1";

        if (!string.IsNullOrEmpty(codigoCajero))
            query += " AND t.CodigoCajero = @Cajero";

        query += " ORDER BY t.Fecha DESC";

        using var comando = new SqlCommand(query, conexion);
        if (!string.IsNullOrEmpty(codigoCajero)) comando.Parameters.AddWithValue("@Cajero", codigoCajero);

        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            string tarjeta = reader.IsDBNull(9) ? "" : reader.GetString(9);
            lista.Add(new Transaccion
            {
                IdTransaccion = reader.GetInt32(0),
                Fecha = reader.GetDateTime(1),
                CodigoCajero = reader.GetString(2),
                IdUsuario = reader.GetInt32(3),
                NombreUsuario = reader.GetString(4),
                Tipo = reader.GetString(5),
                Monto = reader.GetDecimal(6),
                CodigoRespuesta = reader.GetString(7),
                MotivoRechazo = reader.IsDBNull(8) ? null : reader.GetString(8),
                TarjetaEnmascarada = tarjeta.Length >= 4 ? "**** " + tarjeta.Substring(tarjeta.Length - 4) : tarjeta
            });
        }
        return lista;
    }

    public async Task<string> RestablecerPinTemporalAsync(int idUsuario)
    {
        string pinTemporal = new Random().Next(1000, 9999).ToString();
        string salt = "salt123";
        string hash = Seguridad.Sha256Hex(pinTemporal + salt);

        using var conexion = new SqlConnection(_cadenaConexion);
        using var comando = new SqlCommand("UPDATE Tarjeta SET PinHash = @Hash WHERE IdUsuario = @Id AND Activa = 1", conexion);
        comando.Parameters.AddWithValue("@Hash", hash);
        comando.Parameters.AddWithValue("@Id", idUsuario);

        await conexion.OpenAsync();
        int filas = await comando.ExecuteNonQueryAsync();

        return filas > 0 ? pinTemporal : "";
    }

    public async Task<List<AlertaRed>> ObtenerAlertasAsync()
    {
        var lista = new List<AlertaRed>();
        using var conexion = new SqlConnection(_cadenaConexion);
        using var comando = new SqlCommand("SELECT IdAlerta, Fecha, CodigoCajero, Tipo, Descripcion, Atendida FROM Alerta ORDER BY Fecha DESC", conexion);

        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new AlertaRed
            {
                IdAlerta = reader.GetInt32(0),
                Fecha = reader.GetDateTime(1),
                CodigoCajero = reader.GetString(2),
                Tipo = reader.GetString(3),
                Descripcion = reader.GetString(4),
                Atendida = reader.GetBoolean(5)
            });
        }
        return lista;
    }

    public async Task<bool> AtenderAlertaAsync(int idAlerta)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        using var comando = new SqlCommand("UPDATE Alerta SET Atendida = 1, IdAdminAtiende = 1 WHERE IdAlerta = @Id", conexion);
        comando.Parameters.AddWithValue("@Id", idAlerta);

        await conexion.OpenAsync();
        return await comando.ExecuteNonQueryAsync() > 0;
    }

    public async Task<List<object>> ObtenerBitacoraAsync(string? categoria = null, DateTime? inicio = null, DateTime? fin = null)
    {
        var lista = new List<object>();
        using var conexion = new SqlConnection(_cadenaConexion);
        string query = "SELECT IdBitacora, Fecha, Origen, Accion, Detalle FROM Bitacora WHERE 1=1";

        if (inicio.HasValue) query += " AND Fecha >= @Inicio";
        if (fin.HasValue) query += " AND Fecha <= @Fin";
        query += " ORDER BY Fecha DESC";

        using var comando = new SqlCommand(query, conexion);
        if (inicio.HasValue) comando.Parameters.AddWithValue("@Inicio", inicio.Value);
        if (fin.HasValue) comando.Parameters.AddWithValue("@Fin", fin.Value);

        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new
            {
                Id = reader.GetInt32(0),
                Fecha = reader.GetDateTime(1).ToString("yyyy-MM-dd HH:mm:ss"),
                Origen = reader.GetString(2),
                Accion = reader.GetString(3),
                Detalle = reader.IsDBNull(4) ? "" : reader.GetString(4)
            });
        }
        return lista;
    }

    public async Task<Dictionary<string, string>> ObtenerConfiguracionAsync()
    {
        var config = new Dictionary<string, string>();
        using var conexion = new SqlConnection(_cadenaConexion);
        using var comando = new SqlCommand("SELECT Nombre, Valor FROM Parametro", conexion);

        await conexion.OpenAsync();
        using var reader = await comando.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            config[reader.GetString(0)] = reader.GetString(1);
        }
        return config;
    }

    public async Task<bool> GuardarConfiguracionAsync(string nombre, string valor)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        using var comando = new SqlCommand("UPDATE Parametro SET Valor = @Valor WHERE Nombre = @Nombre", conexion);
        comando.Parameters.AddWithValue("@Valor", valor);
        comando.Parameters.AddWithValue("@Nombre", nombre);

        await conexion.OpenAsync();
        int filas = await comando.ExecuteNonQueryAsync();
        return filas > 0;
    }

    public async Task<bool> DarDeBajaCajeroAsync(string codigoCajero)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        // En lugar de borrarlo físicamente, lo marcamos como inactivo/desconectado
        using var comando = new SqlCommand("UPDATE Cajero SET Estado = 'Desconectado', Inicializado = 0 WHERE CodigoCajero = @Codigo", conexion);
        comando.Parameters.AddWithValue("@Codigo", codigoCajero);

        await conexion.OpenAsync();
        return await comando.ExecuteNonQueryAsync() > 0;
    }
}