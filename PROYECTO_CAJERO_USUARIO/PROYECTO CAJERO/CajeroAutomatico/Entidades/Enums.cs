namespace CajeroAutomatico.Entidades
{
    // El rol de Administrador ya no existe del lado del cliente: toda
    // la administración (crear usuarios, inicializar el cajero, agregar
    // efectivo, bloquear tarjetas, reportes, etc.) vive exclusivamente
    // en el servidor.
    public enum EstadoCajero
    {
        Activo,
        Inactivo
    }

    public enum TipoTransaccion
    {
        Retiro,
        Deposito
    }
}
