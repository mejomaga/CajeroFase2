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
            this.lblLimiteMaximo = new System.Windows.Forms.Label();
            this.lblDisponibleHoy = new System.Windows.Forms.Label();
            this.btnSalir = new System.Windows.Forms.Button();
            this.btnVoucher = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblLimiteMaximo
            // 
            this.lblLimiteMaximo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblLimiteMaximo.AutoSize = true;
            this.lblLimiteMaximo.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLimiteMaximo.ForeColor = System.Drawing.Color.White;
            this.lblLimiteMaximo.Location = new System.Drawing.Point(150, 200);
            this.lblLimiteMaximo.Name = "lblLimiteMaximo";
            this.lblLimiteMaximo.Size = new System.Drawing.Size(236, 41);
            this.lblLimiteMaximo.TabIndex = 0;
            this.lblLimiteMaximo.Text = "Límite máximo:";
            // 
            // lblDisponibleHoy
            // 
            this.lblDisponibleHoy.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblDisponibleHoy.AutoSize = true;
            this.lblDisponibleHoy.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblDisponibleHoy.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.lblDisponibleHoy.Location = new System.Drawing.Point(150, 270);
            this.lblDisponibleHoy.Name = "lblDisponibleHoy";
            this.lblDisponibleHoy.Size = new System.Drawing.Size(294, 50);
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
            this.btnSalir.Location = new System.Drawing.Point(804, 560);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(140, 50);
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
            this.btnVoucher.Location = new System.Drawing.Point(644, 560);
            this.btnVoucher.Name = "btnVoucher";
            this.btnVoucher.Size = new System.Drawing.Size(140, 50);
            this.btnVoucher.TabIndex = 4;
            this.btnVoucher.Text = "Imprimir Voucher";
            this.btnVoucher.UseVisualStyleBackColor = false;
            this.btnVoucher.Click += new System.EventHandler(this.btnVoucher_Click);
            // 
            // FormVerSaldo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(40)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.btnVoucher);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.lblDisponibleHoy);
            this.Controls.Add(this.lblLimiteMaximo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormVerSaldo";
            this.Text = "Cajero Automático - Julian Antonio Monterroso González_202342200";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblLimiteMaximo;
        private System.Windows.Forms.Label lblDisponibleHoy;
        private System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.Button btnVoucher;
    }
}