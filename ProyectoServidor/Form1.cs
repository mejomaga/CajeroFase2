using System;
using System.Drawing;
using System.Windows.Forms;
using ProyectoServidor.Comunicaciones;

namespace ProyectoServidor
{
    public partial class Form1 : Form
    {
        private ServidorCentral _servidor;

        // Controles adicionales para Gestión Interbancaria NETGEAR (UDP 5005 + TCP 5000)
        private GroupBox grpInterbancario;
        private TextBox txtMiCodigo;
        private TextBox txtMiNombre;
        private TextBox txtExtCodigo;
        private TextBox txtExtNombre;
        private TextBox txtExtIp;
        private TextBox txtExtPuerto;
        private CheckBox chkExtAutorizado;
        private TextBox txtCuentaPrueba;
        private Button btnEscanearUdp;
        private Button btnGuardarBanco;
        private Button btnPingBanco;
        private Button btnVerificarCuenta;
        private Button btnGuardarMiBanco;

        public Form1()
        {
            InitializeComponent();
            _servidor = new ServidorCentral();
            _servidor.OnMensajeConsola += AgregarMensajeConsola;

            ConstruirPanelInterbancario();
            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _servidor.Iniciar(5000);
            lblEstado.Text = "Servidor Central - ESCUCHANDO (TCP 5000 / UDP 5005)";
            lblEstado.ForeColor = Color.Green;
            btnEncender.Enabled = false;
            btnApagar.Enabled = true;

            CargarDatosInterbancariosEnPantalla();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _servidor?.Detener();
        }

        private void CargarDatosInterbancariosEnPantalla()
        {
            try
            {
                var propio = _servidor.BaseDatos.ObtenerEstadoInterbancarioPropio();
                txtMiCodigo.Text = propio.codigoBanco;
                txtMiNombre.Text = propio.nombreBanco;

                RefrescarListaBancos();
            }
            catch { }
        }

        private void RefrescarListaBancos()
        {
            try
            {
                lstCajeros.Items.Clear();
                var bancos = _servidor.BaseDatos.ObtenerBancosRed(soloAutorizados: false);
                foreach (var b in bancos)
                {
                    lstCajeros.Items.Add(b);
                }
                if (lstCajeros.Items.Count > 0 && lstCajeros.SelectedIndex < 0)
                    lstCajeros.SelectedIndex = 0;
            }
            catch { }
        }

        private void ConstruirPanelInterbancario()
        {
            this.Text = "Servidor Central CajeroRed + Interbancario NETGEAR (TCP 5000 / UDP 5005)";
            this.ClientSize = new Size(1160, 680);

            txtConsola.Location = new Point(310, 69);
            txtConsola.Size = new Size(440, 356);

            var lblListaBancos = new Label
            {
                Text = "Bancos de la Red (Descubiertos / Registrados):",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(768, 45),
                AutoSize = true
            };
            this.Controls.Add(lblListaBancos);

            lstCajeros.SelectedIndexChanged += (s, e) =>
            {
                if (lstCajeros.SelectedItem is BancoRedInfo b)
                {
                    txtExtCodigo.Text = b.CodigoBanco;
                    txtExtNombre.Text = b.NombreBanco;
                    txtExtIp.Text = b.IP;
                    txtExtPuerto.Text = b.PuertoTcp.ToString();
                    chkExtAutorizado.Checked = b.Autorizado;
                }
            };

            grpInterbancario = new GroupBox
            {
                Text = "Red Interbancaria NETGEAR 2.2 (UDP 5005 Discovery + TCP 5000 IB_*)",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(30, 440),
                Size = new Size(1095, 220)
            };

            // 1. Identidad de nuestro banco
            grpInterbancario.Controls.Add(new Label { Text = "Mi Código:", Location = new Point(15, 30), AutoSize = true, Font = new Font("Segoe UI", 9f) });
            txtMiCodigo = new TextBox { Location = new Point(95, 27), Size = new Size(85, 24), Text = "BANC03", Font = new Font("Segoe UI", 9f) };
            grpInterbancario.Controls.Add(txtMiCodigo);

            grpInterbancario.Controls.Add(new Label { Text = "Mi Nombre:", Location = new Point(190, 30), AutoSize = true, Font = new Font("Segoe UI", 9f) });
            txtMiNombre = new TextBox { Location = new Point(275, 27), Size = new Size(130, 24), Text = "CAJERORED", Font = new Font("Segoe UI", 9f) };
            grpInterbancario.Controls.Add(txtMiNombre);

            btnGuardarMiBanco = new Button
            {
                Text = "Guardar Mi Banco",
                Location = new Point(415, 25),
                Size = new Size(135, 28),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            btnGuardarMiBanco.Click += (s, e) =>
            {
                _servidor.BaseDatos.GuardarIdentidadBancoPropio(txtMiCodigo.Text, txtMiNombre.Text, 25000.00m);
                AgregarMensajeConsola($"[CONFIG] Identidad propia actualizada: {txtMiCodigo.Text} ({txtMiNombre.Text})");
            };
            grpInterbancario.Controls.Add(btnGuardarMiBanco);

            btnEscanearUdp = new Button
            {
                Text = "Escanear Bancos (UDP 5005)",
                Location = new Point(565, 24),
                Size = new Size(210, 30),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btnEscanearUdp.Click += async (s, e) =>
            {
                btnEscanearUdp.Enabled = false;
                try
                {
                    await _servidor.EscanearBancosEnRedAsync();
                    RefrescarListaBancos();
                }
                finally
                {
                    btnEscanearUdp.Enabled = true;
                }
            };
            grpInterbancario.Controls.Add(btnEscanearUdp);

            // 2. Registro / Autorización de Banco Externo (ej. BANC02 - NETGEAR)
            grpInterbancario.Controls.Add(new Label { Text = "Cód. Externo:", Location = new Point(15, 78), AutoSize = true, Font = new Font("Segoe UI", 9f) });
            txtExtCodigo = new TextBox { Location = new Point(105, 75), Size = new Size(80, 24), Text = "BANC02", Font = new Font("Segoe UI", 9f) };
            grpInterbancario.Controls.Add(txtExtCodigo);

            grpInterbancario.Controls.Add(new Label { Text = "Nombre:", Location = new Point(195, 78), AutoSize = true, Font = new Font("Segoe UI", 9f) });
            txtExtNombre = new TextBox { Location = new Point(260, 75), Size = new Size(130, 24), Text = "NETGEAR", Font = new Font("Segoe UI", 9f) };
            grpInterbancario.Controls.Add(txtExtNombre);

            grpInterbancario.Controls.Add(new Label { Text = "IPv4 LAN:", Location = new Point(400, 78), AutoSize = true, Font = new Font("Segoe UI", 9f) });
            txtExtIp = new TextBox { Location = new Point(470, 75), Size = new Size(130, 24), Text = "192.168.0.34", Font = new Font("Segoe UI", 9f) };
            grpInterbancario.Controls.Add(txtExtIp);

            grpInterbancario.Controls.Add(new Label { Text = "TCP:", Location = new Point(610, 78), AutoSize = true, Font = new Font("Segoe UI", 9f) });
            txtExtPuerto = new TextBox { Location = new Point(650, 75), Size = new Size(60, 24), Text = "5000", Font = new Font("Segoe UI", 9f) };
            grpInterbancario.Controls.Add(txtExtPuerto);

            chkExtAutorizado = new CheckBox
            {
                Text = "Autorizado para transferir",
                Location = new Point(725, 76),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            grpInterbancario.Controls.Add(chkExtAutorizado);

            btnGuardarBanco = new Button
            {
                Text = "Registrar / Autorizar",
                Location = new Point(915, 72),
                Size = new Size(165, 30),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btnGuardarBanco.Click += (s, e) =>
            {
                int puerto = int.TryParse(txtExtPuerto.Text, out int p) ? p : 5000;
                _servidor.BaseDatos.GuardarOAutorizarBancoRed(
                    txtExtCodigo.Text, txtExtNombre.Text, txtExtIp.Text, puerto, 25000.00m, chkExtAutorizado.Checked, true);
                AgregarMensajeConsola($"[BANCO RED] Guardado {txtExtCodigo.Text} ({txtExtNombre.Text}) @ {txtExtIp.Text}:{puerto} | Autorizado={chkExtAutorizado.Checked}");
                RefrescarListaBancos();
            };
            grpInterbancario.Controls.Add(btnGuardarBanco);

            // 3. Diagnóstico rápido: IB_PING e IB_VERIFICAR_CUENTA
            btnPingBanco = new Button
            {
                Text = "Probar IB_PING",
                Location = new Point(15, 130),
                Size = new Size(160, 34),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btnPingBanco.Click += async (s, e) =>
            {
                var res = await _servidor.ProbarPingBancoAsync(txtExtCodigo.Text);
                MessageBox.Show(res.mensaje, res.exito ? "IB_PING Exitoso" : "IB_PING Falló",
                    MessageBoxButtons.OK, res.exito ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            };
            grpInterbancario.Controls.Add(btnPingBanco);

            grpInterbancario.Controls.Add(new Label { Text = "Cuenta Destino a Verificar:", Location = new Point(195, 138), AutoSize = true, Font = new Font("Segoe UI", 9f) });
            txtCuentaPrueba = new TextBox { Location = new Point(365, 135), Size = new Size(180, 24), Text = "30000001", Font = new Font("Segoe UI", 9f) };
            grpInterbancario.Controls.Add(txtCuentaPrueba);

            btnVerificarCuenta = new Button
            {
                Text = "Probar IB_VERIFICAR_CUENTA",
                Location = new Point(560, 130),
                Size = new Size(230, 34),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btnVerificarCuenta.Click += async (s, e) =>
            {
                var res = await _servidor.VerificarCuentaExternaDesdeServidorAsync(txtExtCodigo.Text, txtCuentaPrueba.Text);
                MessageBox.Show(
                    $"Código: {res.codigo}\nExiste: {res.exito}\nTitular: {res.nombreTitular}\nCupo Banco: Q.{res.cupoDisponible:N2}\nMensaje: {res.mensaje}",
                    "Resultado IB_VERIFICAR_CUENTA",
                    MessageBoxButtons.OK,
                    res.exito ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            };
            grpInterbancario.Controls.Add(btnVerificarCuenta);

            this.Controls.Add(grpInterbancario);
        }

        private void btnEncender_Click(object sender, EventArgs e)
        {
            _servidor.Iniciar(5000);

            lblEstado.Text = "Servidor Central - ESCUCHANDO (TCP 5000 / UDP 5005)";
            lblEstado.ForeColor = Color.Green;
            btnEncender.Enabled = false;
            btnApagar.Enabled = true;
            CargarDatosInterbancariosEnPantalla();
        }

        private void btnApagar_Click(object sender, EventArgs e)
        {
            _servidor.Detener();

            lblEstado.Text = "Servidor Central - APAGADO";
            lblEstado.ForeColor = Color.Red;
            btnEncender.Enabled = true;
            btnApagar.Enabled = false;
        }

        private void AgregarMensajeConsola(string mensaje)
        {
            if (txtConsola.InvokeRequired)
            {
                txtConsola.Invoke(new Action(() => AgregarMensajeConsola(mensaje)));
                return;
            }

            txtConsola.AppendText($"[{DateTime.Now:HH:mm:ss}] {mensaje}{Environment.NewLine}");
            txtConsola.SelectionStart = txtConsola.Text.Length;
            txtConsola.ScrollToCaret();
        }

        private void btnProbarBD_Click(object sender, EventArgs e)
        {
            var conexionBD = new ProyectoServidor.Datos.ConexionServidor();

            if (conexionBD.ProbarConexion(out string msj))
            {
                conexionBD.AsegurarEsquemaInterbancario();
                CargarDatosInterbancariosEnPantalla();
                MessageBox.Show(msj + "\nTablas interbancarias NETGEAR listas.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(msj, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
