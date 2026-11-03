using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CajeroAutomatico.Negocio;

namespace CajeroAutomatico.Presentacion
{
    public static class VigilanteEstado
    {
        private static Timer _timer;
        private static bool _consultando;

        public static void Iniciar()
        {
            if (_timer != null) return;

            _timer = new Timer { Interval = 3000 };
            _timer.Tick += async (s, e) => await RevisarAsync();
            _timer.Start();
        }

        private static async Task RevisarAsync()
        {
            if (_consultando) return;
            _consultando = true;

            try
            {
                var respuesta = await Task.Run(() => Program.Servidor.ObtenerEstadoCajero());

                if (respuesta == null || !respuesta.Exito || respuesta.Datos == null ||
                    !respuesta.Datos.ContainsKey("Estado"))
                    return;

                string estado = respuesta.Datos["Estado"]?.ToString()?.Trim() ?? "";
                if (string.Equals(estado, "Activo", StringComparison.OrdinalIgnoreCase))
                    return;

                // Cajero inactivo. No interrumpimos si ya está en la pantalla de estado
                // ni si hay un cuadro de diálogo abierto (el formulario queda deshabilitado).
                var actual = Program.FormularioActual;
                if (actual == null || actual.IsDisposed || actual is FormEstadoCajero || !actual.Enabled)
                    return;

                await Task.Run(() => Program.Servidor.CerrarSesion());
                Sesion.UsuarioActual = null;

                // Revisamos otra vez: el usuario pudo cambiar de pantalla durante la espera.
                actual = Program.FormularioActual;
                if (actual == null || actual.IsDisposed || actual is FormEstadoCajero || !actual.Enabled)
                    return;

                Program.Navegar(actual, new FormEstadoCajero());
            }
            catch { }
            finally
            {
                _consultando = false;
            }
        }
    }
}