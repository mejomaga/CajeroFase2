using CajeroAutomatico.Presentacion;
using CajeroAutomatico.Datos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CajeroAutomatico
{
    public partial class FormCargando : Form
    {
        public FormCargando()
        {
            InitializeComponent();

            lblEstado.Text = "Cargando, por favor espere...";

            BarraProceso.Value = 0;
            BarraProceso.Maximum = 100;

            TimerCarga = new System.Windows.Forms.Timer
            {
                Interval = 50
            };
            TimerCarga.Tick += async (s, e) => await TimerCarga_TickAsync(s, e);
            Load += (s, e) => TimerCarga.Start();
        }

        private async Task TimerCarga_TickAsync(object sender, EventArgs e)
        {
         
            if (BarraProceso.Value < BarraProceso.Maximum)
            {
                BarraProceso.Value += 2; 
            }
            else
            {
            
                TimerCarga.Stop();

                bool archivosExiste = Program.Servicio.VerificarConexionArchivos();
               // bool arduinoArchivo = Program.Servicio.VerificarConexionArduino();

                if (archivosExiste /*&& arduinoArchivo*/)
                {
                    lblEstado2.Text = "Conexión establecida con éxito.";
                    this.Hide();

                    FormEstadoCajero pantallaExito = new FormEstadoCajero();
                    pantallaExito.FormClosed += (s, args) => this.Close();

                    pantallaExito.Show();
                }
                else
                {
                    lblEstado2.Text = "Error al establecer la conexión. Por favor, verifique los archivos y el Arduino.";
                    await Task.Delay(3000);

                    this.Hide();

                    FormErrorConexion pantallaError = new FormErrorConexion("Error al establecer la conexión. Por favor, verifique los archivos y el Arduino.");
                    pantallaError.Show();

                    this.Close();
                }
            }
        }
    }
}
