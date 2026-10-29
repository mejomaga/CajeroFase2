namespace CajeroAutomatico.Presentacion
{
    partial class FormVerificacion
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
            this.btnPin = new System.Windows.Forms.Button();
            this.btnHuella = new System.Windows.Forms.Button();
            this.lblUsuario = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(0, 150);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1024, 50);
            this.label1.TabIndex = 0;
            this.label1.Text = "Seleccione un método de verificación";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnPin
            // 
            this.btnPin.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnPin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btnPin.FlatAppearance.BorderSize = 0;
            this.btnPin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPin.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPin.ForeColor = System.Drawing.Color.White;
            this.btnPin.Location = new System.Drawing.Point(407, 290);
            this.btnPin.Name = "btnPin";
            this.btnPin.Size = new System.Drawing.Size(250, 120);
            this.btnPin.TabIndex = 1;
            this.btnPin.Text = "PIN";
            this.btnPin.UseVisualStyleBackColor = false;
            this.btnPin.Click += new System.EventHandler(this.btnPin_Click);
            // 
            // btnHuella
            // 
            this.btnHuella.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnHuella.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btnHuella.FlatAppearance.BorderSize = 0;
            this.btnHuella.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHuella.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHuella.ForeColor = System.Drawing.Color.White;
            this.btnHuella.Location = new System.Drawing.Point(407, 447);
            this.btnHuella.Name = "btnHuella";
            this.btnHuella.Size = new System.Drawing.Size(250, 120);
            this.btnHuella.TabIndex = 2;
            this.btnHuella.Text = "Huella Digital";
            this.btnHuella.UseVisualStyleBackColor = false;
            this.btnHuella.Click += new System.EventHandler(this.btnHuella_Click);
            // 
            // lblUsuario
            // 
            this.lblUsuario.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblUsuario.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsuario.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.lblUsuario.Location = new System.Drawing.Point(0, 210);
            this.lblUsuario.Name = "lblUsuario";
            this.lblUsuario.Size = new System.Drawing.Size(1024, 30);
            this.lblUsuario.TabIndex = 3;
            this.lblUsuario.Text = "usuario";
            this.lblUsuario.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblUsuario.Visible = false;
            // 
            // FormVerificacion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(40)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.lblUsuario);
            this.Controls.Add(this.btnHuella);
            this.Controls.Add(this.btnPin);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormVerificacion";
            this.Text = "Cajero Automático - Julian Antonio Monterroso González_202342200";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FormVerificacion_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnPin;
        private System.Windows.Forms.Button btnHuella;
        private System.Windows.Forms.Label lblUsuario;
    }
}