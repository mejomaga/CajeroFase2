using System;
using System.Windows.Forms;
using CajeroAutomatico.Presentacion;
using CajeroAutomatico.Comunicacion;
using CajeroAutomatico.Negocio;

namespace CajeroAutomatico
{
    internal static class Program
    {
        // Único cliente TCP del cajero: todo lo que antes hacía CajeroService
        // (leer/escribir usuarios, validar PIN, mover saldo, etc.) ahora
        // vive en el servidor y se pide a través de este objeto.
        public static ServidorClient Servidor { get; private set; }

        // El lector de tarjetas/huellas (Arduino) solo CAPTURA datos del
        // hardware; no decide nada ni guarda estado de negocio, así que se
        // mantiene local. Lo que hace con esos datos (validarlos) siempre
        // pasa por Servidor.
        public static IArduinoService Lector { get; private set; }

        // Contexto de aplicación para que Windows nunca cierre el hilo de la app
        private static ApplicationContext _contexto;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Servidor = new ServidorClient();
            Lector = new ArduinoService();

            // Iniciamos el contexto con la primera pantalla
            var formInicial = new FormCargando();
            _contexto = new ApplicationContext(formInicial);

            Application.Run(_contexto);
        }

        public static void Navegar(Form actual, Form siguiente)
        {
            if (siguiente == null) return;

            // 1. Clonar geometría exacta
            if (actual != null && !actual.IsDisposed)
            {
                siguiente.StartPosition = FormStartPosition.Manual;
                siguiente.Location = actual.Location;
                siguiente.Size = actual.Size;
                siguiente.WindowState = actual.WindowState;
                siguiente.FormBorderStyle = actual.FormBorderStyle;
                siguiente.TopMost = actual.TopMost;
            }

            // 2. Transferir la ventana principal del hilo al nuevo formulario
            _contexto.MainForm = siguiente;

            // 3. Mostrar el siguiente formulario encima
            siguiente.Show();
            siguiente.BringToFront();
            siguiente.Refresh(); // Fuerza el renderizado completo de todos los controles

            // 4. Retirar el anterior de forma limpia sin tocar el ciclo del hilo
            if (actual != null && !actual.IsDisposed)
            {
                actual.Hide();
                actual.Dispose();
            }
        }
    }
}