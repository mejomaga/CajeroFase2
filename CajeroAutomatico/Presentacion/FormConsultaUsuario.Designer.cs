namespace CajeroAutomatico.Presentacion
{
    partial class FormConsultaUsuario
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnBuscar = new System.Windows.Forms.Button();
            this.txtNumeroTarjeta = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblNombre = new System.Windows.Forms.Label();
            this.lblMontoMaximoDisponible = new System.Windows.Forms.Label();
            this.lblCantidadRetiradaHastaElMomento = new System.Windows.Forms.Label();
            this.lblSaldoActual = new System.Windows.Forms.Label();
            this.lblUltimoAcceso = new System.Windows.Forms.Label();
            this.btnVolver = new System.Windows.Forms.Button();
            this.lblEstadoCuenta = new System.Windows.Forms.Label();
            this.btnCambiarEstado = new System.Windows.Forms.Button();
            this.btnVoucher = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnBuscar
            // 
            this.btnBuscar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnBuscar.FlatAppearance.BorderSize = 0;
            this.btnBuscar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuscar.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnBuscar.ForeColor = System.Drawing.Color.White;
            this.btnBuscar.Location = new System.Drawing.Point(310, 72);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(120, 35);
            this.btnBuscar.TabIndex = 13;
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.UseVisualStyleBackColor = false;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
            // 
            // txtNumeroTarjeta
            // 
            this.txtNumeroTarjeta.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtNumeroTarjeta.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.txtNumeroTarjeta.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNumeroTarjeta.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.txtNumeroTarjeta.ForeColor = System.Drawing.Color.White;
            this.txtNumeroTarjeta.Location = new System.Drawing.Point(50, 75);
            this.txtNumeroTarjeta.MaxLength = 16;
            this.txtNumeroTarjeta.Name = "txtNumeroTarjeta";
            this.txtNumeroTarjeta.Size = new System.Drawing.Size(240, 34);
            this.txtNumeroTarjeta.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.label1.ForeColor = System.Drawing.Color.LightGray;
            this.label1.Location = new System.Drawing.Point(50, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(172, 25);
            this.label1.TabIndex = 11;
            this.label1.Text = "Número de Tarjeta:";
            // 
            // lblNombre
            // 
            this.lblNombre.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblNombre.AutoSize = true;
            this.lblNombre.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblNombre.ForeColor = System.Drawing.Color.White;
            this.lblNombre.Location = new System.Drawing.Point(50, 150);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(123, 37);
            this.lblNombre.TabIndex = 14;
            this.lblNombre.Text = "Nombre";
            // 
            // lblMontoMaximoDisponible
            // 
            this.lblMontoMaximoDisponible.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblMontoMaximoDisponible.AutoSize = true;
            this.lblMontoMaximoDisponible.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblMontoMaximoDisponible.ForeColor = System.Drawing.Color.LightGray;
            this.lblMontoMaximoDisponible.Location = new System.Drawing.Point(50, 205);
            this.lblMontoMaximoDisponible.Name = "lblMontoMaximoDisponible";
            this.lblMontoMaximoDisponible.Size = new System.Drawing.Size(251, 28);
            this.lblMontoMaximoDisponible.TabIndex = 15;
            this.lblMontoMaximoDisponible.Text = "Monto Máximo Disponible:";
            // 
            // lblCantidadRetiradaHastaElMomento
            // 
            this.lblCantidadRetiradaHastaElMomento.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblCantidadRetiradaHastaElMomento.AutoSize = true;
            this.lblCantidadRetiradaHastaElMomento.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblCantidadRetiradaHastaElMomento.ForeColor = System.Drawing.Color.LightGray;
            this.lblCantidadRetiradaHastaElMomento.Location = new System.Drawing.Point(50, 245);
            this.lblCantidadRetiradaHastaElMomento.Name = "lblCantidadRetiradaHastaElMomento";
            this.lblCantidadRetiradaHastaElMomento.Size = new System.Drawing.Size(338, 28);
            this.lblCantidadRetiradaHastaElMomento.TabIndex = 16;
            this.lblCantidadRetiradaHastaElMomento.Text = "Cantidad Retirada Hasta El Momento:";
            // 
            // lblSaldoActual
            // 
            this.lblSaldoActual.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblSaldoActual.AutoSize = true;
            this.lblSaldoActual.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblSaldoActual.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.lblSaldoActual.Location = new System.Drawing.Point(50, 295);
            this.lblSaldoActual.Name = "lblSaldoActual";
            this.lblSaldoActual.Size = new System.Drawing.Size(178, 37);
            this.lblSaldoActual.TabIndex = 17;
            this.lblSaldoActual.Text = "Saldo Actual";
            // 
            // lblUltimoAcceso
            // 
            this.lblUltimoAcceso.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblUltimoAcceso.AutoSize = true;
            this.lblUltimoAcceso.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblUltimoAcceso.ForeColor = System.Drawing.Color.DarkGray;
            this.lblUltimoAcceso.Location = new System.Drawing.Point(50, 360);
            this.lblUltimoAcceso.Name = "lblUltimoAcceso";
            this.lblUltimoAcceso.Size = new System.Drawing.Size(132, 25);
            this.lblUltimoAcceso.TabIndex = 18;
            this.lblUltimoAcceso.Text = "Último Acceso";
            // 
            // btnVolver
            // 
            this.btnVolver.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnVolver.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(165)))), ((int)(((byte)(166)))));
            this.btnVolver.FlatAppearance.BorderSize = 0;
            this.btnVolver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVolver.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnVolver.ForeColor = System.Drawing.Color.White;
            this.btnVolver.Location = new System.Drawing.Point(700, 350);
            this.btnVolver.Name = "btnVolver";
            this.btnVolver.Size = new System.Drawing.Size(220, 50);
            this.btnVolver.TabIndex = 19;
            this.btnVolver.Text = "Volver";
            this.btnVolver.UseVisualStyleBackColor = false;
            this.btnVolver.Click += new System.EventHandler(this.btnVolver_Click);
            // 
            // lblEstadoCuenta
            // 
            this.lblEstadoCuenta.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblEstadoCuenta.AutoSize = true;
            this.lblEstadoCuenta.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblEstadoCuenta.ForeColor = System.Drawing.Color.Gold;
            this.lblEstadoCuenta.Location = new System.Drawing.Point(50, 400);
            this.lblEstadoCuenta.Name = "lblEstadoCuenta";
            this.lblEstadoCuenta.Size = new System.Drawing.Size(177, 32);
            this.lblEstadoCuenta.TabIndex = 20;
            this.lblEstadoCuenta.Text = "Estado Cuenta";
            // 
            // btnCambiarEstado
            // 
            this.btnCambiarEstado.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnCambiarEstado.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.btnCambiarEstado.FlatAppearance.BorderSize = 0;
            this.btnCambiarEstado.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCambiarEstado.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCambiarEstado.ForeColor = System.Drawing.Color.White;
            this.btnCambiarEstado.Location = new System.Drawing.Point(700, 200);
            this.btnCambiarEstado.Name = "btnCambiarEstado";
            this.btnCambiarEstado.Size = new System.Drawing.Size(220, 50);
            this.btnCambiarEstado.TabIndex = 21;
            this.btnCambiarEstado.Text = "Cambiar Estado";
            this.btnCambiarEstado.UseVisualStyleBackColor = false;
            this.btnCambiarEstado.Click += new System.EventHandler(this.btnCambiarEstado_Click);
            // 
            // btnVoucher
            // 
            this.btnVoucher.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnVoucher.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnVoucher.FlatAppearance.BorderSize = 0;
            this.btnVoucher.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVoucher.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnVoucher.ForeColor = System.Drawing.Color.White;
            this.btnVoucher.Location = new System.Drawing.Point(700, 275);
            this.btnVoucher.Name = "btnVoucher";
            this.btnVoucher.Size = new System.Drawing.Size(220, 50);
            this.btnVoucher.TabIndex = 22;
            this.btnVoucher.Text = "Imprimir Voucher";
            this.btnVoucher.UseVisualStyleBackColor = false;
            this.btnVoucher.Click += new System.EventHandler(this.btnVoucher_Click);
            // 
            // FormConsultaUsuario
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(40)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.btnVoucher);
            this.Controls.Add(this.btnCambiarEstado);
            this.Controls.Add(this.lblEstadoCuenta);
            this.Controls.Add(this.btnVolver);
            this.Controls.Add(this.lblUltimoAcceso);
            this.Controls.Add(this.lblSaldoActual);
            this.Controls.Add(this.lblCantidadRetiradaHastaElMomento);
            this.Controls.Add(this.lblMontoMaximoDisponible);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.txtNumeroTarjeta);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormConsultaUsuario";
            this.Text = "Cajero Automático - Julian Antonio Monterroso González_202342200";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FormConsultaUsuario_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.TextBox txtNumeroTarjeta;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.Label lblMontoMaximoDisponible;
        private System.Windows.Forms.Label lblCantidadRetiradaHastaElMomento;
        private System.Windows.Forms.Label lblSaldoActual;
        private System.Windows.Forms.Label lblUltimoAcceso;
        private System.Windows.Forms.Button btnVolver;
        private System.Windows.Forms.Label lblEstadoCuenta;
        private System.Windows.Forms.Button btnCambiarEstado;
        private System.Windows.Forms.Button btnVoucher;
    }
}