using CajeroRed.Dashboard.UI;

namespace CajeroRed.Dashboard;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Ciclo: login -> ventana principal -> (si cierra sesión) otra vez login.
        while (true)
        {
            using var login = new LoginForm();
            if (login.ShowDialog() != DialogResult.OK)
                break;

            using var principal = new MainForm();
            Application.Run(principal);

            if (!principal.VolverALogin)
                break;
        }
    }
}
