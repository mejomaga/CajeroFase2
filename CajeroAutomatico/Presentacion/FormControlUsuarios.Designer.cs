namespace CajeroAutomatico.Presentacion
{
    partial class FormControlUsuarios
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
            this.lblTotalRetirado = new System.Windows.Forms.Label();
            this.lblPromedioDepositado = new System.Windows.Forms.Label();
            this.lstUsuariosCambioPin = new System.Windows.Forms.ListBox();
            this.lblUltimoUsuario = new System.Windows.Forms.Label();
            this.lblFechaUltimoAcceso = new System.Windows.Forms.Label();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.btnVolver = new System.Windows.Forms.Button();
            this.btnBoleta = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTotalRetirado
            // 
            this.lblTotalRetirado.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblTotalRetirado.AutoSize = true;
            this.lblTotalRetirado.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTotalRetirado.ForeColor = System.Drawing.Color.White;
            this.lblTotalRetirado.Location = new System.Drawing.Point(50, 70);
            this.lblTotalRetirado.Name = "lblTotalRetirado";
            this.lblTotalRetirado.Size = new System.Drawing.Size(350, 37);
            this.lblTotalRetirado.TabIndex = 0;
            this.lblTotalRetirado.Text = "Total retirado hoy (todos)";
            // 
            // lblPromedioDepositado
            // 
            this.lblPromedioDepositado.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblPromedioDepositado.AutoSize = true;
            this.lblPromedioDepositado.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.lblPromedioDepositado.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.lblPromedioDepositado.Location = new System.Drawing.Point(50, 130);
            this.lblPromedioDepositado.Name = "lblPromedioDepositado";
            this.lblPromedioDepositado.Size = new System.Drawing.Size(290, 32);
            this.lblPromedioDepositado.TabIndex = 1;
            this.lblPromedioDepositado.Text = "Promedio depositado hoy";
            // 
            // lstUsuariosCambioPin
            // 
            this.lstUsuariosCambioPin.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lstUsuariosCambioPin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lstUsuariosCambioPin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstUsuariosCambioPin.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lstUsuariosCambioPin.ForeColor = System.Drawing.Color.White;
            this.lstUsuariosCambioPin.FormattingEnabled = true;
            this.lstUsuariosCambioPin.ItemHeight = 25;
            this.lstUsuariosCambioPin.Location = new System.Drawing.Point(55, 360);
            this.lstUsuariosCambioPin.Name = "lstUsuariosCambioPin";
            this.lstUsuariosCambioPin.Size = new System.Drawing.Size(469, 277);
            this.lstUsuariosCambioPin.TabIndex = 2;
            // 
            // lblUltimoUsuario
            // 
            this.lblUltimoUsuario.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblUltimoUsuario.AutoSize = true;
            this.lblUltimoUsuario.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblUltimoUsuario.ForeColor = System.Drawing.Color.LightGray;
            this.lblUltimoUsuario.Location = new System.Drawing.Point(50, 190);
            this.lblUltimoUsuario.Name = "lblUltimoUsuario";
            this.lblUltimoUsuario.Size = new System.Drawing.Size(144, 28);
            this.lblUltimoUsuario.TabIndex = 3;
            this.lblUltimoUsuario.Text = "Último Usuario";
            // 
            // lblFechaUltimoAcceso
            // 
            this.lblFechaUltimoAcceso.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblFechaUltimoAcceso.AutoSize = true;
            this.lblFechaUltimoAcceso.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblFechaUltimoAcceso.ForeColor = System.Drawing.Color.DarkGray;
            this.lblFechaUltimoAcceso.Location = new System.Drawing.Point(50, 235);
            this.lblFechaUltimoAcceso.Name = "lblFechaUltimoAcceso";
            this.lblFechaUltimoAcceso.Size = new System.Drawing.Size(186, 25);
            this.lblFechaUltimoAcceso.TabIndex = 4;
            this.lblFechaUltimoAcceso.Text = "Fecha Último Acceso";
            // 
            // lblMensaje
            // 
            this.lblMensaje.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblMensaje.AutoSize = true;
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblMensaje.ForeColor = System.Drawing.Color.Gold;
            this.lblMensaje.Location = new System.Drawing.Point(50, 300);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Size = new System.Drawing.Size(91, 28);
            this.lblMensaje.TabIndex = 6;
            this.lblMensaje.Text = "Mensaje";
            // 
            // btnVolver
            // 
            this.btnVolver.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnVolver.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(165)))), ((int)(((byte)(166)))));
            this.btnVolver.FlatAppearance.BorderSize = 0;
            this.btnVolver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVolver.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnVolver.ForeColor = System.Drawing.Color.White;
            this.btnVolver.Location = new System.Drawing.Point(777, 587);
            this.btnVolver.Name = "btnVolver";
            this.btnVolver.Size = new System.Drawing.Size(180, 50);
            this.btnVolver.TabIndex = 7;
            this.btnVolver.Text = "Volver";
            this.btnVolver.UseVisualStyleBackColor = false;
            this.btnVolver.Click += new System.EventHandler(this.btnVolver_Click);
            // 
            // btnBoleta
            // 
            this.btnBoleta.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnBoleta.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnBoleta.FlatAppearance.BorderSize = 0;
            this.btnBoleta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBoleta.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnBoleta.ForeColor = System.Drawing.Color.White;
            this.btnBoleta.Location = new System.Drawing.Point(577, 587);
            this.btnBoleta.Name = "btnBoleta";
            this.btnBoleta.Size = new System.Drawing.Size(180, 50);
            this.btnBoleta.TabIndex = 9;
            this.btnBoleta.Text = "Voucher";
            this.btnBoleta.UseVisualStyleBackColor = false;
            this.btnBoleta.Click += new System.EventHandler(this.btnBoleta_Click);
            // 
            // FormControlUsuarios
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(40)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.btnBoleta);
            this.Controls.Add(this.btnVolver);
            this.Controls.Add(this.lblMensaje);
            this.Controls.Add(this.lblFechaUltimoAcceso);
            this.Controls.Add(this.lblUltimoUsuario);
            this.Controls.Add(this.lstUsuariosCambioPin);
            this.Controls.Add(this.lblPromedioDepositado);
            this.Controls.Add(this.lblTotalRetirado);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormControlUsuarios";
            this.Text = "Cajero Automático - Julian Antonio Monterroso González_202342200";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTotalRetirado;
        private System.Windows.Forms.Label lblPromedioDepositado;
        private System.Windows.Forms.ListBox lstUsuariosCambioPin;
        private System.Windows.Forms.Label lblUltimoUsuario;
        private System.Windows.Forms.Label lblFechaUltimoAcceso;
        private System.Windows.Forms.Label lblMensaje;
        private System.Windows.Forms.Button btnVolver;
        private System.Windows.Forms.Button btnBoleta;
    }
}