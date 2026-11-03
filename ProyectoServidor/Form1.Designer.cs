namespace ProyectoServidor
{
    partial class Form1
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
            this.lblEstado = new System.Windows.Forms.Label();
            this.btnEncender = new System.Windows.Forms.Button();
            this.btnApagar = new System.Windows.Forms.Button();
            this.lstCajeros = new System.Windows.Forms.ListBox();
            this.txtConsola = new System.Windows.Forms.TextBox();
            this.btnProbarBD = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblEstado
            // 
            this.lblEstado.AutoSize = true;
            this.lblEstado.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEstado.ForeColor = System.Drawing.Color.IndianRed;
            this.lblEstado.Location = new System.Drawing.Point(47, 15);
            this.lblEstado.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(237, 24);
            this.lblEstado.TabIndex = 79;
            this.lblEstado.Text = "Servidor Central - Apagado";
            // 
            // btnEncender
            // 
            this.btnEncender.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEncender.Location = new System.Drawing.Point(51, 56);
            this.btnEncender.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnEncender.Name = "btnEncender";
            this.btnEncender.Size = new System.Drawing.Size(163, 45);
            this.btnEncender.TabIndex = 80;
            this.btnEncender.Text = "Encender Servidor";
            this.btnEncender.UseVisualStyleBackColor = true;
            this.btnEncender.Click += new System.EventHandler(this.btnEncender_Click);
            // 
            // btnApagar
            // 
            this.btnApagar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnApagar.Location = new System.Drawing.Point(51, 123);
            this.btnApagar.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnApagar.Name = "btnApagar";
            this.btnApagar.Size = new System.Drawing.Size(163, 45);
            this.btnApagar.TabIndex = 81;
            this.btnApagar.Text = "Apagar Servidor";
            this.btnApagar.UseVisualStyleBackColor = true;
            this.btnApagar.Click += new System.EventHandler(this.btnApagar_Click);
            // 
            // lstCajeros
            // 
            this.lstCajeros.FormattingEnabled = true;
            this.lstCajeros.Location = new System.Drawing.Point(889, 56);
            this.lstCajeros.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.lstCajeros.Name = "lstCajeros";
            this.lstCajeros.Size = new System.Drawing.Size(257, 290);
            this.lstCajeros.TabIndex = 82;
            // 
            // txtConsola
            // 
            this.txtConsola.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConsola.Location = new System.Drawing.Point(338, 56);
            this.txtConsola.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtConsola.Multiline = true;
            this.txtConsola.Name = "txtConsola";
            this.txtConsola.ReadOnly = true;
            this.txtConsola.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtConsola.Size = new System.Drawing.Size(225, 288);
            this.txtConsola.TabIndex = 83;
            // 
            // btnProbarBD
            // 
            this.btnProbarBD.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnProbarBD.Location = new System.Drawing.Point(51, 299);
            this.btnProbarBD.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnProbarBD.Name = "btnProbarBD";
            this.btnProbarBD.Size = new System.Drawing.Size(163, 45);
            this.btnProbarBD.TabIndex = 84;
            this.btnProbarBD.Text = "Probar BD";
            this.btnProbarBD.UseVisualStyleBackColor = true;
            this.btnProbarBD.Click += new System.EventHandler(this.btnProbarBD_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1176, 479);
            this.Controls.Add(this.btnProbarBD);
            this.Controls.Add(this.txtConsola);
            this.Controls.Add(this.lstCajeros);
            this.Controls.Add(this.btnApagar);
            this.Controls.Add(this.btnEncender);
            this.Controls.Add(this.lblEstado);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.Button btnEncender;
        private System.Windows.Forms.Button btnApagar;
        private System.Windows.Forms.ListBox lstCajeros;
        private System.Windows.Forms.TextBox txtConsola;
        private System.Windows.Forms.Button btnProbarBD;
    }
}

