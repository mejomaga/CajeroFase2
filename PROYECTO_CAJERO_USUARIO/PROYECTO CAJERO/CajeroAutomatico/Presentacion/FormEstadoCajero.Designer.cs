namespace CajeroAutomatico.Presentacion
{
    partial class FormEstadoCajero
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
            this.lblInstruccion = new System.Windows.Forms.Label();
            this.lblEstadoActual = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblInstruccion
            // 
            this.lblInstruccion.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblInstruccion.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstruccion.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.lblInstruccion.Location = new System.Drawing.Point(0, 350);
            this.lblInstruccion.Name = "lblInstruccion";
            this.lblInstruccion.Size = new System.Drawing.Size(1024, 40);
            this.lblInstruccion.TabIndex = 3;
            this.lblInstruccion.Text = "Toque la pantalla para continuar";
            this.lblInstruccion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblEstadoActual
            // 
            this.lblEstadoActual.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblEstadoActual.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
            this.lblEstadoActual.ForeColor = System.Drawing.Color.White;
            this.lblEstadoActual.Location = new System.Drawing.Point(0, 260);
            this.lblEstadoActual.Name = "lblEstadoActual";
            this.lblEstadoActual.Size = new System.Drawing.Size(1024, 60);
            this.lblEstadoActual.TabIndex = 2;
            this.lblEstadoActual.Text = "Estado Cajero";
            this.lblEstadoActual.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormEstadoCajero
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(40)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.lblInstruccion);
            this.Controls.Add(this.lblEstadoActual);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormEstadoCajero";
            this.Text = "Cajero Automático - Julian Antonio Monterroso González_202342200";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Click += new System.EventHandler(this.FormEstadoCajero_Click);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblInstruccion;
        private System.Windows.Forms.Label lblEstadoActual;
    }
}