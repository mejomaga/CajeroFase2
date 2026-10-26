namespace CajeroAutomatico.Presentacion
{
    partial class FormCrearUsuario
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.numSaldoInicial = new System.Windows.Forms.NumericUpDown();
            this.numLimiteDiario = new System.Windows.Forms.NumericUpDown();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.txtHuella = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnVolver = new System.Windows.Forms.Button();
            this.txtNumeroTarjeta = new System.Windows.Forms.TextBox();
            this.txtPin = new System.Windows.Forms.TextBox();
            this.btnTeclado = new System.Windows.Forms.Button();
            this.btnVincularTarjeta = new System.Windows.Forms.Button();
            this.lblEstadoVinculacion = new System.Windows.Forms.Label();
            this.btnRegistrarHuella = new System.Windows.Forms.Button();
            this.lblEstadoHuella = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numSaldoInicial)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLimiteDiario)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(50, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(323, 50);
            this.label1.TabIndex = 0;
            this.label1.Text = "Registrar Usuario";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.label2.ForeColor = System.Drawing.Color.LightGray;
            this.label2.Location = new System.Drawing.Point(50, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 25);
            this.label2.TabIndex = 1;
            this.label2.Text = "Nombre:";
            // 
            // txtNombre
            // 
            this.txtNombre.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtNombre.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.txtNombre.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNombre.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtNombre.ForeColor = System.Drawing.Color.White;
            this.txtNombre.Location = new System.Drawing.Point(50, 115);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(300, 34);
            this.txtNombre.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.label3.ForeColor = System.Drawing.Color.LightGray;
            this.label3.Location = new System.Drawing.Point(50, 160);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(172, 25);
            this.label3.TabIndex = 3;
            this.label3.Text = "Número de Tarjeta:";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.label4.ForeColor = System.Drawing.Color.LightGray;
            this.label4.Location = new System.Drawing.Point(50, 230);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 25);
            this.label4.TabIndex = 5;
            this.label4.Text = "PIN:";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.label5.ForeColor = System.Drawing.Color.LightGray;
            this.label5.Location = new System.Drawing.Point(50, 300);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(118, 25);
            this.label5.TabIndex = 7;
            this.label5.Text = "Saldo Inicial:";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.label6.ForeColor = System.Drawing.Color.LightGray;
            this.label6.Location = new System.Drawing.Point(50, 370);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(123, 25);
            this.label6.TabIndex = 9;
            this.label6.Text = "Límite Diario:";
            // 
            // numSaldoInicial
            // 
            this.numSaldoInicial.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.numSaldoInicial.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.numSaldoInicial.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.numSaldoInicial.ForeColor = System.Drawing.Color.White;
            this.numSaldoInicial.Location = new System.Drawing.Point(50, 325);
            this.numSaldoInicial.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numSaldoInicial.Name = "numSaldoInicial";
            this.numSaldoInicial.Size = new System.Drawing.Size(200, 34);
            this.numSaldoInicial.TabIndex = 13;
            // 
            // numLimiteDiario
            // 
            this.numLimiteDiario.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.numLimiteDiario.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.numLimiteDiario.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.numLimiteDiario.ForeColor = System.Drawing.Color.White;
            this.numLimiteDiario.Location = new System.Drawing.Point(50, 395);
            this.numLimiteDiario.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numLimiteDiario.Name = "numLimiteDiario";
            this.numLimiteDiario.Size = new System.Drawing.Size(200, 34);
            this.numLimiteDiario.TabIndex = 14;
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(750, 200);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(200, 50);
            this.btnGuardar.TabIndex = 15;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // lblMensaje
            // 
            this.lblMensaje.AutoSize = true;
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblMensaje.ForeColor = System.Drawing.Color.Gold;
            this.lblMensaje.Location = new System.Drawing.Point(50, 540);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Size = new System.Drawing.Size(106, 28);
            this.lblMensaje.TabIndex = 16;
            this.lblMensaje.Text = "Resultado";
            // 
            // txtHuella
            // 
            this.txtHuella.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtHuella.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.txtHuella.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHuella.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtHuella.ForeColor = System.Drawing.Color.White;
            this.txtHuella.Location = new System.Drawing.Point(50, 465);
            this.txtHuella.Name = "txtHuella";
            this.txtHuella.Size = new System.Drawing.Size(200, 34);
            this.txtHuella.TabIndex = 17;
            this.txtHuella.Visible = false;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.label7.ForeColor = System.Drawing.Color.LightGray;
            this.label7.Location = new System.Drawing.Point(50, 440);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(130, 25);
            this.label7.TabIndex = 18;
            this.label7.Text = "Huella Digital:";
            // 
            // btnVolver
            // 
            this.btnVolver.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnVolver.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(165)))), ((int)(((byte)(166)))));
            this.btnVolver.FlatAppearance.BorderSize = 0;
            this.btnVolver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVolver.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnVolver.ForeColor = System.Drawing.Color.White;
            this.btnVolver.Location = new System.Drawing.Point(750, 350);
            this.btnVolver.Name = "btnVolver";
            this.btnVolver.Size = new System.Drawing.Size(200, 50);
            this.btnVolver.TabIndex = 19;
            this.btnVolver.Text = "Volver";
            this.btnVolver.UseVisualStyleBackColor = false;
            this.btnVolver.Click += new System.EventHandler(this.btnVolver_Click);
            // 
            // txtNumeroTarjeta
            // 
            this.txtNumeroTarjeta.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtNumeroTarjeta.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.txtNumeroTarjeta.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNumeroTarjeta.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtNumeroTarjeta.ForeColor = System.Drawing.Color.White;
            this.txtNumeroTarjeta.Location = new System.Drawing.Point(50, 185);
            this.txtNumeroTarjeta.MaxLength = 16;
            this.txtNumeroTarjeta.Name = "txtNumeroTarjeta";
            this.txtNumeroTarjeta.Size = new System.Drawing.Size(300, 34);
            this.txtNumeroTarjeta.TabIndex = 20;
            // 
            // txtPin
            // 
            this.txtPin.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtPin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.txtPin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPin.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtPin.ForeColor = System.Drawing.Color.White;
            this.txtPin.Location = new System.Drawing.Point(50, 255);
            this.txtPin.MaxLength = 4;
            this.txtPin.Name = "txtPin";
            this.txtPin.PasswordChar = '●';
            this.txtPin.Size = new System.Drawing.Size(150, 34);
            this.txtPin.TabIndex = 21;
            // 
            // btnTeclado
            // 
            this.btnTeclado.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnTeclado.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnTeclado.FlatAppearance.BorderSize = 0;
            this.btnTeclado.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTeclado.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnTeclado.ForeColor = System.Drawing.Color.White;
            this.btnTeclado.Location = new System.Drawing.Point(750, 275);
            this.btnTeclado.Name = "btnTeclado";
            this.btnTeclado.Size = new System.Drawing.Size(200, 50);
            this.btnTeclado.TabIndex = 22;
            this.btnTeclado.Text = "Abrir Teclado";
            this.btnTeclado.UseVisualStyleBackColor = false;
            this.btnTeclado.Click += new System.EventHandler(this.btnTeclado_Click);
            // 
            // btnVincularTarjeta
            // 
            this.btnVincularTarjeta.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnVincularTarjeta.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnVincularTarjeta.FlatAppearance.BorderSize = 0;
            this.btnVincularTarjeta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVincularTarjeta.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnVincularTarjeta.ForeColor = System.Drawing.Color.White;
            this.btnVincularTarjeta.Location = new System.Drawing.Point(365, 183);
            this.btnVincularTarjeta.Name = "btnVincularTarjeta";
            this.btnVincularTarjeta.Size = new System.Drawing.Size(140, 32);
            this.btnVincularTarjeta.TabIndex = 23;
            this.btnVincularTarjeta.Text = "Vincular Tarjeta";
            this.btnVincularTarjeta.UseVisualStyleBackColor = false;
            this.btnVincularTarjeta.Click += new System.EventHandler(this.btnVincularTarjeta_Click);
            // 
            // lblEstadoVinculacion
            // 
            this.lblEstadoVinculacion.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblEstadoVinculacion.AutoSize = true;
            this.lblEstadoVinculacion.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblEstadoVinculacion.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.lblEstadoVinculacion.Location = new System.Drawing.Point(520, 190);
            this.lblEstadoVinculacion.Name = "lblEstadoVinculacion";
            this.lblEstadoVinculacion.Size = new System.Drawing.Size(61, 23);
            this.lblEstadoVinculacion.TabIndex = 24;
            this.lblEstadoVinculacion.Text = "Estado";
            // 
            // btnRegistrarHuella
            // 
            this.btnRegistrarHuella.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnRegistrarHuella.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnRegistrarHuella.FlatAppearance.BorderSize = 0;
            this.btnRegistrarHuella.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegistrarHuella.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRegistrarHuella.ForeColor = System.Drawing.Color.White;
            this.btnRegistrarHuella.Location = new System.Drawing.Point(50, 465);
            this.btnRegistrarHuella.Name = "btnRegistrarHuella";
            this.btnRegistrarHuella.Size = new System.Drawing.Size(200, 38);
            this.btnRegistrarHuella.TabIndex = 25;
            this.btnRegistrarHuella.Text = "Registrar Huella";
            this.btnRegistrarHuella.UseVisualStyleBackColor = false;
            this.btnRegistrarHuella.Click += new System.EventHandler(this.btnRegistrarHuella_Click);
            // 
            // lblEstadoHuella
            // 
            this.lblEstadoHuella.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblEstadoHuella.AutoSize = true;
            this.lblEstadoHuella.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblEstadoHuella.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.lblEstadoHuella.Location = new System.Drawing.Point(265, 474);
            this.lblEstadoHuella.Name = "lblEstadoHuella";
            this.lblEstadoHuella.Size = new System.Drawing.Size(61, 23);
            this.lblEstadoHuella.TabIndex = 26;
            this.lblEstadoHuella.Text = "Estado";
            // 
            // FormCrearUsuario
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(40)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.lblEstadoHuella);
            this.Controls.Add(this.btnRegistrarHuella);
            this.Controls.Add(this.lblEstadoVinculacion);
            this.Controls.Add(this.btnVincularTarjeta);
            this.Controls.Add(this.btnTeclado);
            this.Controls.Add(this.txtPin);
            this.Controls.Add(this.txtNumeroTarjeta);
            this.Controls.Add(this.btnVolver);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtHuella);
            this.Controls.Add(this.lblMensaje);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.numLimiteDiario);
            this.Controls.Add(this.numSaldoInicial);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormCrearUsuario";
            this.Text = "Cajero Automático - Julian Antonio Monterroso González_202342200";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCrearUsuario_FormClosing);
            this.Load += new System.EventHandler(this.FormCrearUsuario_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numSaldoInicial)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLimiteDiario)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numSaldoInicial;
        private System.Windows.Forms.NumericUpDown numLimiteDiario;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Label lblMensaje;
        private System.Windows.Forms.TextBox txtHuella;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnVolver;
        private System.Windows.Forms.TextBox txtNumeroTarjeta;
        private System.Windows.Forms.TextBox txtPin;
        private System.Windows.Forms.Button btnTeclado;
        private System.Windows.Forms.Button btnVincularTarjeta;
        private System.Windows.Forms.Label lblEstadoVinculacion;
        private System.Windows.Forms.Button btnRegistrarHuella;
        private System.Windows.Forms.Label lblEstadoHuella;
    }
}