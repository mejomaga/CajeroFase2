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
            bool correcto = Program.Servicio.VerificarHuellaDigital(usuario, txtHuella.Text);

            if (correcto)
            {
                Sesion.UsuarioActual = usuario;

                Form siguiente = usuario.Tipo == TipoUsuario.Administrador
                    ? (Form)new FormMenuAdmin()
                    : (Form)new FormMenuUsuario();

                Program.Navegar(this, siguiente);
                return;
            }

            if (usuario.Bloqueado)
            {
                MessageBox.Show("Tarjeta bloqueada por demasiados intentos fallidos.\nContacte al administrador.",
                    "Tarjeta bloqueada", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                Program.Navegar(this, new FormInsertarTarjeta());
                return;
            }

            MessageBox.Show("Huella digital incorrecta. Intente nuevamente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            txtHuella.Clear();
            txtHuella.Focus();

            
        }

        private void FormHuellaDigital_Load(object sender, EventArgs e)
        {
            VozService.Hablar("Acérque su huella al lector.");

            _procesando = false;

            // Suscribirse al evento de huella
            Program.Servicio.HuellaDetectada += HuellaDetectada;

            // Activar el modo "esperando huella"
            Program.Servicio.IniciarLector();
            Program.Servicio.ModoEsperarHuella(true);
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
            _procesando = true;

            // Slot -1 significa "huella no reconocida"
            if (slot == -1)
            {
                MessageBox.Show("Huella no reconocida. Intente nuevamente.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _procesando = false;
                return;
            }

            // Verificar que el slot coincide con el del usuario
            if (usuario.HuellaSlot != slot)
            {
                MessageBox.Show("Esa huella no corresponde a este usuario.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _procesando = false;
                return;
            }

            // Huella válida
            Sesion.UsuarioActual = usuario;

            Form siguiente = usuario.Tipo == TipoUsuario.Administrador
                ? (Form)new FormMenuAdmin()
                : (Form)new FormMenuUsuario();

            Program.Navegar(this, siguiente);
        }



        private void btnRegresar_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormVerificacion(usuario));
        }

        private void FormHuellaDigital_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Servicio.HuellaDetectada -= HuellaDetectada;
            Program.Servicio.ModoEsperarHuella(false);
            Program.Servicio.DetenerLector();
        }
    }
 
}
