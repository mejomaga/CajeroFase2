using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CajeroRed.Dashboard.Modelos;

namespace CajeroRed.Dashboard.Servicios;

public interface IServicioDatos
{
    string Descripcion { get; }

    Task<Administrador?> IniciarSesionAdminAsync(string usuario, string contrasenaHash);
    Task<ResumenDia> ObtenerResumenAsync();
    Task<List<Cajero>> ObtenerCajerosAsync();
    Task<List<PuntoHora>> ObtenerSerieHorariaAsync();

    // Etapa 3.1: Gestión de Cajeros
    Task<bool> InicializarCajeroAsync(string codigoCajero, Dictionary<int, int> desglose);
    Task<bool> AgregarEfectivoCajeroAsync(string codigoCajero, Dictionary<int, int> desglose);
    Task<bool> CambiarEstadoCajeroAsync(string codigoCajero, string nuevoEstado);
    Task<bool> GuardarCajeroAsync(string codigo, string nombre, string ubicacion, string ip);
    /// <summary>Da de baja lógica a un cajero para conservar el historial de la red.</summary>
    Task<bool> DarDeBajaCajeroAsync(string codigoCajero);

    // Etapa 3.2: Gestión de Usuarios
    Task<List<Usuario>> ObtenerUsuariosAsync();
    Task<bool> CrearUsuarioAsync(string nombre, string tarjeta, string pin, decimal saldo, decimal limiteDiario);
    Task<bool> ModificarUsuarioAsync(int idUsuario, string nombre, decimal saldo, decimal limiteDiario);
    Task<bool> CambiarTarjetaUsuarioAsync(int idUsuario, string nuevaTarjeta);
    Task<bool> CambiarEstadoUsuarioAsync(int idUsuario, string nuevoEstado);

    // Etapa 4: Transacciones y Operaciones
    Task<List<Transaccion>> ObtenerTransaccionesAsync(string? codigoCajero = null, string? tipo = null);
    Task<List<Transaccion>> ObtenerDepositosAsync(string? codigoCajero = null);
    Task<List<Transaccion>> ObtenerRetirosAsync(string? codigoCajero = null, bool? soloRechazados = null);
    Task<List<Transaccion>> ObtenerCambiosPinAsync();
    Task<string> RestablecerPinTemporalAsync(int idUsuario);

    // Etapa 5: Alertas, Bitácora y Configuración
    Task<List<AlertaRed>> ObtenerAlertasAsync();
    Task<bool> AtenderAlertaAsync(int idAlerta);

    // Corregido a List<object> para evitar errores de tipo no encontrado
    Task<List<object>> ObtenerBitacoraAsync(string? categoria = null, DateTime? inicio = null, DateTime? fin = null);

    Task<Dictionary<string, string>> ObtenerConfiguracionAsync();
    Task<bool> GuardarConfiguracionAsync(string nombre, string valor);
}