using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CajeroAutomatico.Entidades;
using CajeroAutomatico.Negocio;

namespace CajeroAutomatico.Presentacion
{
    public partial class FormHuellaDigital : Form
    {
        private Usuario usuario;
        private bool _procesando = false;

        public FormHuellaDigital(Usuario usuario)
        {
            InitializeComponent();
            this.usuario = usuario;
        }

        private void btnContinuar_Click(object sender, EventArgs e)
        {
           Program.Navegar(this, new FormPIN(usuario));
        }

        private void FormHuellaDigital_Load(object sender, EventArgs e)
        {
            VozService.Hablar("Acérque su huella al lector.");

            _procesando = false;

            // Suscribirse al evento de huella del lector físico. El Arduino
            // solo CAPTURA la huella; quien decide si es válida sigue
            // siendo el servidor (ver ProcesarHuella).
            Program.Lector.HuellaDetectada += HuellaDetectada;

            Program.Lector.Iniciar();
            Program.Lector.ModoEsperarHuella(true);
        }

        private void HuellaDetectada(int slot)
        {
            // Si el formulario ya no está vivo, ignorar
            if (!this.IsHandleCreated || this.IsDisposed) return;

            if (_procesando) return;

            this.Invoke(new Action(() =>
            {
                ProcesarHuella(slot);
            }));
        }

        private void ProcesarHuella(int slot)
        {
            
        }

        private void btnRegresar_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormVerificacion(usuario));
        }

        private void FormHuellaDigital_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Lector.HuellaDetectada -= HuellaDetectada;
            Program.Lector.ModoEsperarHuella(false);
            Program.Lector.Detener();
        }
    }
 
}
