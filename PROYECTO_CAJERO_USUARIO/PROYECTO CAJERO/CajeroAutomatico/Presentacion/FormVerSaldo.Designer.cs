namespace CajeroAutomatico.Presentacion
{
    partial class FormVerSaldo
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
            this.lblSaldoActual = new System.Windows.Forms.Label();
            this.lblDisponibleHoy = new System.Windows.Forms.Label();
            this.btnSalir = new System.Windows.Forms.Button();
            this.btnVoucher = new System.Windows.Forms.Button();
            this.lblLimiteUsuario = new System.Windows.Forms.Label();
            this.lblLimiteSistema = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblSaldoActual
            // 
            this.lblSaldoActual.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblSaldoActual.AutoSize = true;
            this.lblSaldoActual.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSaldoActual.ForeColor = System.Drawing.Color.White;
            this.lblSaldoActual.Location = new System.Drawing.Point(112, 162);
            this.lblSaldoActual.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblSaldoActual.Name = "lblSaldoActual";
            this.lblSaldoActual.Size = new System.Drawing.Size(164, 32);
            this.lblSaldoActual.TabIndex = 0;
            this.lblSaldoActual.Text = "Saldo Actual:";
            // 
            // lblDisponibleHoy
            // 
            this.lblDisponibleHoy.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblDisponibleHoy.AutoSize = true;
            this.lblDisponibleHoy.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblDisponibleHoy.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.lblDisponibleHoy.Location = new System.Drawing.Point(112, 215);
            this.lblDisponibleHoy.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDisponibleHoy.Name = "lblDisponibleHoy";
            this.lblDisponibleHoy.Size = new System.Drawing.Size(238, 41);
            this.lblDisponibleHoy.TabIndex = 1;
            this.lblDisponibleHoy.Text = "Disponible hoy:";
            // 
            // btnSalir
            // 
            this.btnSalir.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnSalir.FlatAppearance.BorderSize = 0;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.Location = new System.Drawing.Point(603, 455);
            this.btnSalir.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(105, 41);
            this.btnSalir.TabIndex = 3;
            this.btnSalir.Text = "Salir";
            this.btnSalir.UseVisualStyleBackColor = false;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // btnVoucher
            // 
            this.btnVoucher.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnVoucher.FlatAppearance.BorderSize = 0;
            this.btnVoucher.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVoucher.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVoucher.ForeColor = System.Drawing.Color.White;
            this.btnVoucher.Location = new System.Drawing.Point(483, 455);
            this.btnVoucher.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnVoucher.Name = "btnVoucher";
            this.btnVoucher.Size = new System.Drawing.Size(105, 41);
            this.btnVoucher.TabIndex = 4;
            this.btnVoucher.Text = "Imprimir Voucher";
            this.btnVoucher.UseVisualStyleBackColor = false;
            this.btnVoucher.Click += new System.EventHandler(this.btnVoucher_Click);
            // 
            // lblLimiteUsuario
            // 
            this.lblLimiteUsuario.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblLimiteUsuario.AutoSize = true;
            this.lblLimiteUsuario.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLimiteUsuario.ForeColor = System.Drawing.Color.White;
            this.lblLimiteUsuario.Location = new System.Drawing.Point(113, 130);
            this.lblLimiteUsuario.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblLimiteUsuario.Name = "lblLimiteUsuario";
            this.lblLimiteUsuario.Size = new System.Drawing.Size(228, 32);
            this.lblLimiteUsuario.TabIndex = 5;
            this.lblLimiteUsuario.Text = "Limite del Usuario:";
            // 
            // lblLimiteSistema
            // 
            this.lblLimiteSistema.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblLimiteSistema.AutoSize = true;
            this.lblLimiteSistema.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLimiteSistema.ForeColor = System.Drawing.Color.White;
            this.lblLimiteSistema.Location = new System.Drawing.Point(113, 98);
            this.lblLimiteSistema.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblLimiteSistema.Name = "lblLimiteSistema";
            this.lblLimiteSistema.Size = new System.Drawing.Size(228, 32);
            this.lblLimiteSistema.TabIndex = 6;
            this.lblLimiteSistema.Text = "Limite del Sistema:";
            // 
            // FormVerSaldo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(40)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(768, 624);
            this.Controls.Add(this.lblLimiteSistema);
            this.Controls.Add(this.lblLimiteUsuario);
            this.Controls.Add(this.btnVoucher);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.lblDisponibleHoy);
            this.Controls.Add(this.lblSaldoActual);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "FormVerSaldo";
            this.Text = "Cajero Automático - Julian Antonio Monterroso González_202342200";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSaldoActual;
        private System.Windows.Forms.Label lblDisponibleHoy;
        private System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.Button btnVoucher;
        private System.Windows.Forms.Label lblLimiteUsuario;
        private System.Windows.Forms.Label lblLimiteSistema;
    }
}