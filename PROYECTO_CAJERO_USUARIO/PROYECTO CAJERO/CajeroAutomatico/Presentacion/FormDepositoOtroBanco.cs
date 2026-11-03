using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using CajeroAutomatico.Comunicacion;
using CajeroAutomatico.Negocio;

namespace CajeroAutomatico.Presentacion
{
    public class FormDepositoOtroBanco : Form
    {
        private ComboBox cboBancoDestino;
        private TextBox txtCuentaDestino;
        private TextBox txtMonto;
        private TextBox txtConcepto;
        private Label lblTitularVerificado;
        private Label lblMensaje;
        private Button btnVerificarCuenta;
        private Button btnTransferir;
        private Button btnCancelar;

        private TextBox _campoTactilActivo;
        private bool _cuentaVerificada = false;
        private string _tokenPendienteConciliar = "";

        public FormDepositoOtroBanco()
        {
            ConstruirInterfazTactil();
            this.Load += FormDepositoOtroBanco_Load;
        }

        private async void FormDepositoOtroBanco_Load(object sender, EventArgs e)
        {
            try
            {
                VozService.Hablar("Seleccione el banco externo, verifique la cuenta destino e ingrese el monto a transferir");
            }
            catch { }

            await CargarBancosAutorizadosAsync();
        }

        private async Task CargarBancosAutorizadosAsync()
        {
            lblMensaje.Text = "Consultando bancos autorizados en el servidor...";
            var bancos = await Task.Run(() => Program.Servidor.ObtenerBancosAutorizados());

            cboBancoDestino.DataSource = bancos;
            if (bancos.Count > 0)
            {
                cboBancoDestino.SelectedIndex = 0;
                lblMensaje.ForeColor = Color.LightGreen;
                lblMensaje.Text = "Toque 'Cuenta Destino', ingrese el número y presione 'Verificar Cuenta'.";
            }
            else
            {
                lblMensaje.ForeColor = Color.Orange;
                lblMensaje.Text = "No hay bancos externos autorizados. Autorícelos primero en el Servidor Central.";
            }
        }

        private void BotonTeclado_Click(object sender, EventArgs e)
        {
            if (_campoTactilActivo == null)
                _campoTactilActivo = txtCuentaDestino;

            string tecla = ((Button)sender).Text;

            if (tecla == ".")
            {
                if (_campoTactilActivo == txtMonto && !txtMonto.Text.Contains("."))
                {
                    txtMonto.Text = string.IsNullOrEmpty(txtMonto.Text) ? "0." : txtMonto.Text + ".";
                }
                return;
            }

            if (_campoTactilActivo == txtCuentaDestino)
            {
                if (txtCuentaDestino.Text.Length < 24)
                    txtCuentaDestino.Text += tecla;
            }
            else if (_campoTactilActivo == txtMonto)
            {
                // Permitir máximo 2 decimales (requisito guía NETGEAR)
                int idxPunto = txtMonto.Text.IndexOf('.');
                if (idxPunto >= 0 && txtMonto.Text.Length - idxPunto > 2)
                    return;

                if (txtMonto.Text == "0")
                    txtMonto.Text = tecla;
                else if (txtMonto.Text.Length < 10)
                    txtMonto.Text += tecla;
            }
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            if (_campoTactilActivo == null) return;
            if (_campoTactilActivo.Text.Length > 0)
            {
                _campoTactilActivo.Text = _campoTactilActivo.Text.Substring(0, _campoTactilActivo.Text.Length - 1);
            }
        }

        private async void btnVerificarCuenta_Click(object sender, EventArgs e)
        {
            var banco = cboBancoDestino.SelectedItem as BancoExternoDto;
            if (banco == null)
            {
                MostrarMensaje("Seleccione primero un banco autorizado.", true);
                return;
            }

            string cuenta = txtCuentaDestino.Text.Trim();
            if (string.IsNullOrEmpty(cuenta))
            {
                MostrarMensaje("Ingrese el número de cuenta destino.", true);
                return;
            }

            btnVerificarCuenta.Enabled = false;
            lblTitularVerificado.Text = "Verificando con " + banco.NombreBanco + "...";
            lblTitularVerificado.ForeColor = Color.Khaki;

            try
            {
                var res = await Task.Run(() => Program.Servidor.VerificarCuentaInterbancaria(banco.CodigoBanco, cuenta));
                if (res.Exito && res.Booleano("Existe"))
                {
                    string titular = res.Texto("NombreTitular");
                    decimal cupo = res.Decimal("CupoDisponible");
                    _cuentaVerificada = true;
                    lblTitularVerificado.ForeColor = Color.MediumSpringGreen;
                    lblTitularVerificado.Text = $"✔ Titular: {titular} (Cupo disp.: Q.{cupo:N2})";
                    MostrarMensaje("Cuenta verificada. Toque el campo Monto e ingrese la cantidad a transferir.", false);
                    _campoTactilActivo = txtMonto;
                    txtMonto.Focus();
                    VozService.Hablar($"Cuenta verificada a nombre de {titular}");
                }
                else
                {
                    _cuentaVerificada = false;
                    string cod = res.Texto("Codigo", "14");
                    lblTitularVerificado.ForeColor = Color.Salmon;
                    lblTitularVerificado.Text = $"✘ Cuenta no válida (Código {cod})";
                    MostrarMensaje(res.Mensaje ?? "La cuenta no existe en el banco destino.", true);
                }
            }
            finally
            {
                btnVerificarCuenta.Enabled = true;
            }
        }

        private async void btnTransferir_Click(object sender, EventArgs e)
        {
            var banco = cboBancoDestino.SelectedItem as BancoExternoDto;
            if (banco == null)
            {
                MostrarMensaje("Seleccione un banco destino autorizado.", true);
                return;
            }

            string cuenta = txtCuentaDestino.Text.Trim();
            if (string.IsNullOrEmpty(cuenta) || !_cuentaVerificada)
            {
                MostrarMensaje("Debe verificar la cuenta destino antes de transferir.", true);
                return;
            }

            if (!decimal.TryParse(txtMonto.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto) || monto <= 0)
            {
                MostrarMensaje("Ingrese un monto válido mayor a Q.0.00", true);
                return;
            }

            btnTransferir.Enabled = false;
            btnVerificarCuenta.Enabled = false;
            btnCancelar.Enabled = false;
            MostrarMensaje($"Enviando transferencia de Q.{monto:N2} a {banco.NombreBanco}...", false);

            try
            {
                string concepto = string.IsNullOrWhiteSpace(txtConcepto.Text) ? "Transferencia CajeroRed" : txtConcepto.Text.Trim();
                var res = await Task.Run(() => Program.Servidor.TransferirInterbancario(
                    banco.CodigoBanco, cuenta, monto, concepto, _tokenPendienteConciliar));

                string codigo = res.Texto("Codigo", res.Exito ? "00" : "96");
                string tokenTrf = res.Texto("TokenTransferencia");
                string titular = res.Texto("NombreDestino");

                if (res.Exito && codigo == "00")
                {
                    _tokenPendienteConciliar = "";
                    VozService.Hablar("Transferencia interbancaria acreditada con éxito");
                    MessageBox.Show(
                        $"¡Transferencia Interbancaria Aprobada!\n\n" +
                        $"Banco Destino: {banco.NombreBanco} ({banco.CodigoBanco})\n" +
                        $"Cuenta Destino: {cuenta}\n" +
                        $"Titular: {titular}\n" +
                        $"Monto: Q.{monto:N2}\n" +
                        $"Token Transferencia: {tokenTrf}",
                        "Comprobante IB_TRANSFERIR",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    Program.Navegar(this, new FormMenuUsuario());
                    return;
                }

                // Si quedó en código 96 o estado incierto, conservamos el token para conciliar sin duplicar envío
                if (codigo == "96" && !string.IsNullOrEmpty(tokenTrf))
                {
                    _tokenPendienteConciliar = tokenTrf;
                }
                else
                {
                    _tokenPendienteConciliar = "";
                }

                MostrarMensaje(res.Mensaje ?? $"No se pudo completar la transferencia (Código {codigo}).", true);
                MessageBox.Show(res.Mensaje ?? "Operación no confirmada.", "Transferencia Interbancaria",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btnTransferir.Enabled = true;
                btnVerificarCuenta.Enabled = true;
                btnCancelar.Enabled = true;
            }
        }

        private void MostrarMensaje(string msg, bool esError)
        {
            lblMensaje.ForeColor = esError ? Color.Salmon : Color.LightGreen;
            lblMensaje.Text = msg;
        }

        private void ConstruirInterfazTactil()
        {
            this.Text = "Transferir Dinero a Otro Banco (NETGEAR)";
            this.ClientSize = new Size(768, 624);
            this.BackColor = Color.FromArgb(28, 40, 51);
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            var lblTitulo = new Label
            {
                Text = "Transferir Dinero a Otro Banco",
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 25),
                AutoSize = true
            };
            this.Controls.Add(lblTitulo);

            // Panel Izquierdo: Formulario
            var panelIzq = new Panel
            {
                Location = new Point(30, 80),
                Size = new Size(410, 490),
                BackColor = Color.FromArgb(44, 62, 80)
            };

            panelIzq.Controls.Add(new Label
            {
                Text = "1. Banco Destino Autorizado:",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 18),
                AutoSize = true
            });

            cboBancoDestino = new ComboBox
            {
                Location = new Point(20, 46),
                Size = new Size(370, 34),
                Font = new Font("Segoe UI", 12f),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboBancoDestino.SelectedIndexChanged += (s, e) =>
            {
                _cuentaVerificada = false;
                _tokenPendienteConciliar = "";
                lblTitularVerificado.Text = "Cuenta pendiente de verificar";
                lblTitularVerificado.ForeColor = Color.LightGray;
            };
            panelIzq.Controls.Add(cboBancoDestino);

            panelIzq.Controls.Add(new Label
            {
                Text = "2. Número de Cuenta / Tarjeta Destino:",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 95),
                AutoSize = true
            });

            txtCuentaDestino = new TextBox
            {
                Location = new Point(20, 123),
                Size = new Size(235, 34),
                Font = new Font("Consolas", 14f, FontStyle.Bold)
            };
            txtCuentaDestino.Enter += (s, e) => _campoTactilActivo = txtCuentaDestino;
            txtCuentaDestino.Click += (s, e) => _campoTactilActivo = txtCuentaDestino;
            txtCuentaDestino.TextChanged += (s, e) =>
            {
                _cuentaVerificada = false;
                _tokenPendienteConciliar = "";
                lblTitularVerificado.Text = "Cuenta pendiente de verificar";
                lblTitularVerificado.ForeColor = Color.LightGray;
            };
            panelIzq.Controls.Add(txtCuentaDestino);

            btnVerificarCuenta = new Button
            {
                Text = "Verificar",
                Location = new Point(265, 121),
                Size = new Size(125, 34),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold)
            };
            btnVerificarCuenta.Click += btnVerificarCuenta_Click;
            panelIzq.Controls.Add(btnVerificarCuenta);

            lblTitularVerificado = new Label
            {
                Text = "Cuenta pendiente de verificar",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.LightGray,
                Location = new Point(20, 165),
                Size = new Size(370, 26)
            };
            panelIzq.Controls.Add(lblTitularVerificado);

            panelIzq.Controls.Add(new Label
            {
                Text = "3. Monto a Transferir (Q.):",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 205),
                AutoSize = true
            });

            txtMonto = new TextBox
            {
                Location = new Point(20, 233),
                Size = new Size(370, 34),
                Font = new Font("Consolas", 15f, FontStyle.Bold),
                Text = ""
            };
            txtMonto.Enter += (s, e) => _campoTactilActivo = txtMonto;
            txtMonto.Click += (s, e) => _campoTactilActivo = txtMonto;
            txtMonto.TextChanged += (s, e) => _tokenPendienteConciliar = "";
            panelIzq.Controls.Add(txtMonto);

            panelIzq.Controls.Add(new Label
            {
                Text = "4. Concepto:",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 282),
                AutoSize = true
            });

            txtConcepto = new TextBox
            {
                Location = new Point(20, 310),
                Size = new Size(370, 32),
                Font = new Font("Segoe UI", 11.5f),
                Text = "Transferencia interbancaria"
            };
            panelIzq.Controls.Add(txtConcepto);

            lblMensaje = new Label
            {
                Text = "Seleccione el banco y verifique la cuenta antes de enviar.",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.LightGray,
                Location = new Point(20, 360),
                Size = new Size(370, 110)
            };
            panelIzq.Controls.Add(lblMensaje);

            this.Controls.Add(panelIzq);

            // Panel Derecho: Teclado Numérico Táctil y Botones
            var panelDer = new Panel
            {
                Location = new Point(460, 80),
                Size = new Size(280, 490),
                BackColor = Color.FromArgb(44, 62, 80)
            };

            string[] teclas = { "1", "2", "3", "4", "5", "6", "7", "8", "9", ".", "0" };
            int startX = 22, startY = 20, w = 72, h = 58, gap = 10;

            for (int i = 0; i < teclas.Length; i++)
            {
                int fila = i / 3;
                int col = i % 3;
                var btn = new Button
                {
                    Text = teclas[i],
                    Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(startX + col * (w + gap), startY + fila * (h + gap)),
                    Size = new Size(w, h)
                };
                btn.Click += BotonTeclado_Click;
                panelDer.Controls.Add(btn);
            }

            var btnBorrar = new Button
            {
                Text = "⌫",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                BackColor = Color.FromArgb(211, 84, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(startX + 2 * (w + gap), startY + 3 * (h + gap)),
                Size = new Size(w, h)
            };
            btnBorrar.Click += btnBorrar_Click;
            panelDer.Controls.Add(btnBorrar);

            btnTransferir = new Button
            {
                Text = "Transferir Dinero",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(22, 315),
                Size = new Size(236, 52)
            };
            btnTransferir.Click += btnTransferir_Click;
            panelDer.Controls.Add(btnTransferir);

            btnCancelar = new Button
            {
                Text = "Regresar al Menú",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(22, 382),
                Size = new Size(236, 46)
            };
            btnCancelar.Click += (s, e) => Program.Navegar(this, new FormMenuUsuario());
            panelDer.Controls.Add(btnCancelar);

            this.Controls.Add(panelDer);
            _campoTactilActivo = txtCuentaDestino;
        }
    }
}
