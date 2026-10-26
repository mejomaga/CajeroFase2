using CajeroAutomatico.Entidades;
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
    public partial class FormModificarTarjeta : Form
    {
        private Usuario usuarioEncontrado;
        private string _uidNuevo = "";

        // Rastrear cuál TextBox tiene el foco táctil
        private TextBox _cajaActiva;

        // Variables de respaldo para la impresión del comprobante
        private string _tarjetaAnterior;
        private string _tarjetaNueva;
        private string _nombreCliente;
        public FormModificarTarjeta()
        {
            InitializeComponent();

            _cajaActiva = txtTarjetaActual;

            AsignarEventosFoco();
            AsignarEventosTeclado();

            this.Load += FormModificarTarjeta_Load;
            this.FormClosing += FormModificarTarjeta_FormClosing;
        }

        private void AsignarEventosFoco()
        {
            txtTarjetaActual.Enter += (s, e) => _cajaActiva = txtTarjetaActual;
            txtTarjetaNueva.Enter += (s, e) => _cajaActiva = txtTarjetaNueva;
        }

        private void AsignarEventosTeclado()
        {
            btn0.Click += BotonNumero_Click;
            btn1.Click += BotonNumero_Click;
            btn2.Click += BotonNumero_Click;
            btn3.Click += BotonNumero_Click;
            btn4.Click += BotonNumero_Click;
            btn5.Click += BotonNumero_Click;
            btn6.Click += BotonNumero_Click;
            btn7.Click += BotonNumero_Click;
            btn8.Click += BotonNumero_Click;
            btn9.Click += BotonNumero_Click;
        }

        private void BotonNumero_Click(object sender, EventArgs e)
        {
            if (_cajaActiva == null || !_cajaActiva.Enabled) return;

            Button boton = (Button)sender;

            // Limitar longitud estándar de tarjeta (16 dígitos)
            if (_cajaActiva.Text.Length < 16)
            {
                _cajaActiva.Text += boton.Text;
            }
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            if (_cajaActiva != null && _cajaActiva.Enabled && _cajaActiva.Text.Length > 0)
            {
                _cajaActiva.Text = _cajaActiva.Text.Substring(0, _cajaActiva.Text.Length - 1);
            }
        }

        private void FormModificarTarjeta_Load(object sender, EventArgs e)
        {
            Program.Servicio.TarjetaDetectada += TarjetaDetectada;

            txtTarjetaNueva.Enabled = false;
            btnGuardar.Enabled = false;
            btnVincularNueva.Enabled = false;
            lblEstadoVinculacion.Text = "";
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            usuarioEncontrado = Program.Servicio.BuscarPorTarjeta(txtTarjetaActual.Text.Trim());

            if (usuarioEncontrado == null)
            {
                MessageBox.Show("No se encontró ningún usuario con esa tarjeta.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblNombreUsuario.Text = "Usuario: No encontrado";
                txtTarjetaNueva.Enabled = false;
                btnGuardar.Enabled = false;
                btnVincularNueva.Enabled = false;
            }
            else
            {
                lblNombreUsuario.Text = $"Usuario encontrado: {usuarioEncontrado.Nombre}";
                txtTarjetaNueva.Enabled = true;
                btnVincularNueva.Enabled = true;
                btnGuardar.Enabled = true;

                txtTarjetaNueva.Focus();
                _cajaActiva = txtTarjetaNueva;
            }
        }



        private void TarjetaDetectada(string uid)
        {
            // Si el formulario ya no existe, ignorar
            if (!this.IsHandleCreated || this.IsDisposed) return;

            this.Invoke(new Action(() =>
            {
                if (btnVincularNueva.Enabled) return;
                _uidNuevo = uid;
                lblEstadoVinculacion.ForeColor = Color.Green;
                lblEstadoVinculacion.Text = "Tarjeta vinculada: " + uid;
            }));
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string nuevaTarjeta = txtTarjetaNueva.Text.Trim();

            if (string.IsNullOrEmpty(nuevaTarjeta) || nuevaTarjeta.Length != 16)
            {
                lblMensaje.ForeColor = Color.DarkRed;
                lblMensaje.Text = "El número de tarjeta nueva debe tener 16 dígitos.";
                return;
            }

            if (string.IsNullOrWhiteSpace(_uidNuevo))
            {
                lblMensaje.ForeColor = Color.DarkRed;
                lblMensaje.Text = "Debe vincular la tarjeta física nueva antes de guardar.";
                return;
            }

            var resultado = Program.Servicio.ModificarTarjetaCompleta(
                usuarioEncontrado,
                nuevaTarjeta,
                _uidNuevo
            );

            lblMensaje.ForeColor = resultado.Exito ? Color.DarkGreen : Color.DarkRed;
            lblMensaje.Text = resultado.Mensaje;

            if (resultado.Exito)
            {
                txtTarjetaActual.Clear();
                txtTarjetaNueva.Clear();
                txtTarjetaNueva.Enabled = false;
                btnGuardar.Enabled = false;
                btnVincularNueva.Enabled = false;
                lblNombreUsuario.Text = "";
                lblEstadoVinculacion.Text = "";
                usuarioEncontrado = null;
                _uidNuevo = "";
                _cajaActiva = txtTarjetaActual;

                MessageBox.Show(resultado.Mensaje, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Program.Navegar(this, new FormMenuAdmin());
            }
            else
            {
                MessageBox.Show(resultado.Mensaje, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            Program.Navegar(this, new FormMenuAdmin());
        }

        private void FormModificarTarjeta_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Servicio.TarjetaDetectada -= TarjetaDetectada;
            Program.Servicio.DetenerLector();
        }

        private void btnVincularNueva_Click(object sender, EventArgs e)
        {
            _uidNuevo = "";
            lblEstadoVinculacion.ForeColor = Color.Blue;
            lblEstadoVinculacion.Text = "Acerca la tarjeta nueva al lector...";
            btnVincularNueva.Enabled = false;

            Program.Servicio.IniciarLector();
        }
    }
}
