namespace CajeroAutomatico
{
    partial class FormCargando
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblEstado = new System.Windows.Forms.Label();
            this.TimerCarga = new System.Windows.Forms.Timer(this.components);
            this.BarraProceso = new System.Windows.Forms.ProgressBar();
            this.lblEstado2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblEstado
            // 
            this.lblEstado.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblEstado.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEstado.ForeColor = System.Drawing.Color.White;
            this.lblEstado.Location = new System.Drawing.Point(0, 195);
            this.lblEstado.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(768, 41);
            this.lblEstado.TabIndex = 0;
            this.lblEstado.Text = "Iniciando Cajero...";
            this.lblEstado.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblEstado.Click += new System.EventHandler(this.lblEstado_Click);
            // 
            // BarraProceso
            // 
            this.BarraProceso.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.BarraProceso.Location = new System.Drawing.Point(196, 260);
            this.BarraProceso.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.BarraProceso.Name = "BarraProceso";
            this.BarraProceso.Size = new System.Drawing.Size(375, 24);
            this.BarraProceso.TabIndex = 1;
            // 
            // lblEstado2
            // 
            this.lblEstado2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblEstado2.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEstado2.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.lblEstado2.Location = new System.Drawing.Point(0, 305);
            this.lblEstado2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblEstado2.Name = "lblEstado2";
            this.lblEstado2.Size = new System.Drawing.Size(768, 24);
            this.lblEstado2.TabIndex = 2;
            this.lblEstado2.Text = "Cargando componentes...";
            this.lblEstado2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormCargando
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(40)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(768, 624);
            this.Controls.Add(this.lblEstado2);
            this.Controls.Add(this.BarraProceso);
            this.Controls.Add(this.lblEstado);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "FormCargando";
            this.Text = "Cajero Automático - Julian Antonio Monterroso González_202342200";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.Timer TimerCarga;
        private System.Windows.Forms.ProgressBar BarraProceso;
        private System.Windows.Forms.Label lblEstado2;
    }
}