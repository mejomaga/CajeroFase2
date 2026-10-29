using CajeroAutomatico.Entidades;

namespace CajeroAutomatico.Negocio
{
    // Guarda quién tiene la tarjeta en mano (solo datos de pantalla) y el
    // token de sesión que entregó el servidor al validar PIN/huella. Ya NO
    // guarda saldo, límite ni PIN: esos datos se piden al servidor cada vez
    // que una pantalla los necesita.
    public static class Sesion
    {
        public static Usuario UsuarioActual { get; set; }

        public static void CerrarSesion()
        {
            Program.Servidor.CerrarSesion();
            UsuarioActual = null;
        }
    }
}
