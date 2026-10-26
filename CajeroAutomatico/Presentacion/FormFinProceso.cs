using CajeroAutomatico.Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CajeroAutomatico.Presentacion
{
    public partial class FormFinProceso : Form
    {
        public FormFinProceso()
        {
            InitializeComponent();

            // Limpiamos cualquier enlace previo del diseñador para evitar doble ejecución
            TimerCierre.Tick -= TimerCierre_Tick;

            TimerCierre.Interval = 5000; // 5 segundos
            TimerCierre.Tick += TimerCierre_Tick;
        }

        private void FinCajero_Load(object sender, EventArgs e)
        {
            TimerCierre.Stop();
            TimerCierre.Start();

            VozService.Hablar("Operación finalizada. Gracias por utilizar nuestro cajero automático.");
        }

        private void TimerCierre_Tick(object sender, EventArgs e)
        {
            // Apagamos el reloj de inmediato para que ninguna otra pulsación entre aquí
            TimerCierre.Stop();
            TimerCierre.Enabled = false;
            TimerCierre.Tick -= TimerCierre_Tick;

            // Navegamos de forma segura a la pantalla inicial del cajero
            Program.Navegar(this, new FormEstadoCajero());
            this.Close();
        }
    }
}
