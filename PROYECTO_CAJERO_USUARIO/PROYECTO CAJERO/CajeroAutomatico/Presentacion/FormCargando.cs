using CajeroAutomatico.Presentacion;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

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

                // Verificamos conexión mediante el cliente central del cajero
                bool conectadoAlServidor = Program.Servidor.VerificarConexion();

                if (conectadoAlServidor)
                {
                    lblEstado2.Text = "Conexión con el servidor establecida con éxito.";
                    this.Hide();

                    FormEstadoCajero pantallaExito = new FormEstadoCajero();
                    pantallaExito.FormClosed += (s, args) => this.Close();

                    pantallaExito.Show();
                }
                else
                {
                    lblEstado2.Text = "No se pudo conectar con el servidor. Verifique la red y vuelva a intentarlo.";
                    await Task.Delay(3000);

                    this.Hide();

                    FormErrorConexion pantallaError = new FormErrorConexion("No se pudo conectar con el servidor. Verifique la red y vuelva a intentarlo.");
                    pantallaError.Show();

                    this.Close();
                }
            }
        }

        private void lblEstado_Click(object sender, EventArgs e)
        {

        }
    }
}
