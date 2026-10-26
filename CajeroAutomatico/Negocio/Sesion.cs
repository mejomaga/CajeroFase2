using CajeroAutomatico.Entidades;

namespace CajeroAutomatico.Negocio
{
    // Guarda quién es el usuario/administrador que tiene la sesión abierta
    // en este momento, para que los Forms no tengan que pasárselo unos a
    // otros por el constructor.
    public static class Sesion
    {
        public static Usuario UsuarioActual { get; set; }

        public static void CerrarSesion()
        {
            UsuarioActual = null;
        }
    }
}
