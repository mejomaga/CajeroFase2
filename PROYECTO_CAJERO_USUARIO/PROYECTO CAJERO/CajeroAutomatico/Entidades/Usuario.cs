namespace CajeroAutomatico.Entidades
{
    // Representación MÍNIMA de un usuario en el cliente, solo para mostrar
    // en pantalla (nombre, tarjeta). El cliente NUNCA guarda PIN, saldo,
    // huella ni límites: esos datos viven únicamente en el servidor y se
    // consultan bajo demanda (ver ServidorClient / Sesion).
    public class Usuario
    {
        public string NumeroTarjeta { get; set; }
        public string Nombre { get; set; }
        public bool Bloqueado { get; set; }
        public bool RequiereHuella { get; set; }
    }
}
