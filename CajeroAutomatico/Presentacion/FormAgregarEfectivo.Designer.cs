namespace CajeroAutomatico.Presentacion
{
    partial class FormAgregarEfectivo
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
            this.lblInstrucciones = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.numeric200 = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numeric100 = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numeric50 = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numeric20 = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.numeric10 = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.numeric5 = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.numeric1 = new System.Windows.Forms.NumericUpDown();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.btnVolver = new System.Windows.Forms.Button();
            this.btn1 = new System.Windows.Forms.Button();
            this.btn2 = new System.Windows.Forms.Button();
            this.btn3 = new System.Windows.Forms.Button();
            this.btn4 = new System.Windows.Forms.Button();
            this.btn5 = new System.Windows.Forms.Button();
            this.btn6 = new System.Windows.Forms.Button();
            this.btn7 = new System.Windows.Forms.Button();
            this.btn8 = new System.Windows.Forms.Button();
            this.btn9 = new System.Windows.Forms.Button();
            this.btn0 = new System.Windows.Forms.Button();
            this.btnBorrar = new System.Windows.Forms.Button();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.lbl200Disponible = new System.Windows.Forms.Label();
            this.lbl1Disponible = new System.Windows.Forms.Label();
            this.lbl5Disponible = new System.Windows.Forms.Label();
            this.lbl10Disponible = new System.Windows.Forms.Label();
            this.lbl20Disponible = new System.Windows.Forms.Label();
            this.lbl50Disponible = new System.Windows.Forms.Label();
            this.lbl100Disponible = new System.Windows.Forms.Label();
            this.lblTotalActual = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numeric200)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric100)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric50)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric20)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric10)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblInstrucciones
            // 
            this.lblInstrucciones.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblInstrucciones.AutoSize = true;
            this.lblInstrucciones.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstrucciones.ForeColor = System.Drawing.Color.White;
            this.lblInstrucciones.Location = new System.Drawing.Point(40, 25);
            this.lblInstrucciones.Name = "lblInstrucciones";
            this.lblInstrucciones.Size = new System.Drawing.Size(581, 56);
            this.lblInstrucciones.TabIndex = 18;
            this.lblInstrucciones.Text = "Indique cuántos billetes de cada denominación va a agregar\r\n(máximo Q 30,000.00 p" +
    "or lote)";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label1.ForeColor = System.Drawing.Color.LightGray;
            this.label1.Location = new System.Drawing.Point(40, 85);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(159, 23);
            this.label1.TabIndex = 20;
            this.label1.Text = "Billetes de Q 200.00";
            // 
            // numeric200
            // 
            this.numeric200.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.numeric200.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numeric200.Location = new System.Drawing.Point(220, 82);
            this.numeric200.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numeric200.Name = "numeric200";
            this.numeric200.Size = new System.Drawing.Size(120, 32);
            this.numeric200.TabIndex = 36;
            this.numeric200.ValueChanged += new System.EventHandler(this.numeric200_ValueChanged);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label2.ForeColor = System.Drawing.Color.LightGray;
            this.label2.Location = new System.Drawing.Point(40, 120);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(159, 23);
            this.label2.TabIndex = 22;
            this.label2.Text = "Billetes de Q 100.00";
            // 
            // numeric100
            // 
            this.numeric100.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.numeric100.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numeric100.Location = new System.Drawing.Point(220, 117);
            this.numeric100.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numeric100.Name = "numeric100";
            this.numeric100.Size = new System.Drawing.Size(120, 32);
            this.numeric100.TabIndex = 37;
            this.numeric100.ValueChanged += new System.EventHandler(this.numeric100_ValueChanged);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label3.ForeColor = System.Drawing.Color.LightGray;
            this.label3.Location = new System.Drawing.Point(40, 155);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(150, 23);
            this.label3.TabIndex = 24;
            this.label3.Text = "Billetes de Q 50.00";
            // 
            // numeric50
            // 
            this.numeric50.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.numeric50.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numeric50.Location = new System.Drawing.Point(220, 152);
            this.numeric50.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numeric50.Name = "numeric50";
            this.numeric50.Size = new System.Drawing.Size(120, 32);
            this.numeric50.TabIndex = 38;
            this.numeric50.ValueChanged += new System.EventHandler(this.numeric50_ValueChanged);
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label4.ForeColor = System.Drawing.Color.LightGray;
            this.label4.Location = new System.Drawing.Point(40, 190);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(150, 23);
            this.label4.TabIndex = 26;
            this.label4.Text = "Billetes de Q 20.00";
            // 
            // numeric20
            // 
            this.numeric20.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.numeric20.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numeric20.Location = new System.Drawing.Point(220, 187);
            this.numeric20.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numeric20.Name = "numeric20";
            this.numeric20.Size = new System.Drawing.Size(120, 32);
            this.numeric20.TabIndex = 39;
            this.numeric20.ValueChanged += new System.EventHandler(this.numeric20_ValueChanged);
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label5.ForeColor = System.Drawing.Color.LightGray;
            this.label5.Location = new System.Drawing.Point(40, 225);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(150, 23);
            this.label5.TabIndex = 28;
            this.label5.Text = "Billetes de Q 10.00";
            // 
            // numeric10
            // 
            this.numeric10.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.numeric10.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numeric10.Location = new System.Drawing.Point(220, 222);
            this.numeric10.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numeric10.Name = "numeric10";
            this.numeric10.Size = new System.Drawing.Size(120, 32);
            this.numeric10.TabIndex = 40;
            this.numeric10.ValueChanged += new System.EventHandler(this.numeric10_ValueChanged);
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label6.ForeColor = System.Drawing.Color.LightGray;
            this.label6.Location = new System.Drawing.Point(40, 260);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(141, 23);
            this.label6.TabIndex = 30;
            this.label6.Text = "Billetes de Q 5.00";
            // 
            // numeric5
            // 
            this.numeric5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.numeric5.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numeric5.Location = new System.Drawing.Point(220, 257);
            this.numeric5.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numeric5.Name = "numeric5";
            this.numeric5.Size = new System.Drawing.Size(120, 32);
            this.numeric5.TabIndex = 41;
            this.numeric5.ValueChanged += new System.EventHandler(this.numeric5_ValueChanged);
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label7.ForeColor = System.Drawing.Color.LightGray;
            this.label7.Location = new System.Drawing.Point(40, 295);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(141, 23);
            this.label7.TabIndex = 32;
            this.label7.Text = "Billetes de Q 1.00";
            // 
            // numeric1
            // 
            this.numeric1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.numeric1.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numeric1.Location = new System.Drawing.Point(220, 292);
            this.numeric1.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numeric1.Name = "numeric1";
            this.numeric1.Size = new System.Drawing.Size(120, 32);
            this.numeric1.TabIndex = 42;
            this.numeric1.ValueChanged += new System.EventHandler(this.numeric1_ValueChanged);
            // 
            // lblTotal
            // 
            this.lblTotal.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotal.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.lblTotal.Location = new System.Drawing.Point(40, 350);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(178, 37);
            this.lblTotal.TabIndex = 34;
            this.lblTotal.Text = "Total: Q 0.00";
            // 
            // lblMensaje
            // 
            this.lblMensaje.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblMensaje.AutoSize = true;
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMensaje.ForeColor = System.Drawing.Color.Gold;
            this.lblMensaje.Location = new System.Drawing.Point(40, 395);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Size = new System.Drawing.Size(98, 28);
            this.lblMensaje.TabIndex = 35;
            this.lblMensaje.Text = "Resultado";
            // 
            // btnVolver
            // 
            this.btnVolver.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnVolver.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(165)))), ((int)(((byte)(166)))));
            this.btnVolver.FlatAppearance.BorderSize = 0;
            this.btnVolver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVolver.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVolver.ForeColor = System.Drawing.Color.White;
            this.btnVolver.Location = new System.Drawing.Point(850, 40);
            this.btnVolver.Name = "btnVolver";
            this.btnVolver.Size = new System.Drawing.Size(120, 45);
            this.btnVolver.TabIndex = 43;
            this.btnVolver.Text = "Volver";
            this.btnVolver.UseVisualStyleBackColor = false;
            this.btnVolver.Click += new System.EventHandler(this.btnVolver_Click);
            // 
            // btn1
            // 
            this.btn1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btn1.FlatAppearance.BorderSize = 0;
            this.btn1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn1.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn1.ForeColor = System.Drawing.Color.White;
            this.btn1.Location = new System.Drawing.Point(620, 114);
            this.btn1.Name = "btn1";
            this.btn1.Size = new System.Drawing.Size(90, 70);
            this.btn1.TabIndex = 65;
            this.btn1.Text = "1";
            this.btn1.UseVisualStyleBackColor = false;
            this.btn1.Click += new System.EventHandler(this.btn1_Click);
            // 
            // btn2
            // 
            this.btn2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btn2.FlatAppearance.BorderSize = 0;
            this.btn2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn2.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn2.ForeColor = System.Drawing.Color.White;
            this.btn2.Location = new System.Drawing.Point(720, 114);
            this.btn2.Name = "btn2";
            this.btn2.Size = new System.Drawing.Size(90, 70);
            this.btn2.TabIndex = 66;
            this.btn2.Text = "2";
            this.btn2.UseVisualStyleBackColor = false;
            this.btn2.Click += new System.EventHandler(this.btn2_Click);
            // 
            // btn3
            // 
            this.btn3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btn3.FlatAppearance.BorderSize = 0;
            this.btn3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn3.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn3.ForeColor = System.Drawing.Color.White;
            this.btn3.Location = new System.Drawing.Point(820, 114);
            this.btn3.Name = "btn3";
            this.btn3.Size = new System.Drawing.Size(90, 70);
            this.btn3.TabIndex = 67;
            this.btn3.Text = "3";
            this.btn3.UseVisualStyleBackColor = false;
            this.btn3.Click += new System.EventHandler(this.btn3_Click);
            // 
            // btn4
            // 
            this.btn4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btn4.FlatAppearance.BorderSize = 0;
            this.btn4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn4.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn4.ForeColor = System.Drawing.Color.White;
            this.btn4.Location = new System.Drawing.Point(620, 194);
            this.btn4.Name = "btn4";
            this.btn4.Size = new System.Drawing.Size(90, 70);
            this.btn4.TabIndex = 68;
            this.btn4.Text = "4";
            this.btn4.UseVisualStyleBackColor = false;
            this.btn4.Click += new System.EventHandler(this.btn4_Click);
            // 
            // btn5
            // 
            this.btn5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btn5.FlatAppearance.BorderSize = 0;
            this.btn5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn5.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn5.ForeColor = System.Drawing.Color.White;
            this.btn5.Location = new System.Drawing.Point(720, 194);
            this.btn5.Name = "btn5";
            this.btn5.Size = new System.Drawing.Size(90, 70);
            this.btn5.TabIndex = 69;
            this.btn5.Text = "5";
            this.btn5.UseVisualStyleBackColor = false;
            this.btn5.Click += new System.EventHandler(this.btn5_Click);
            // 
            // btn6
            // 
            this.btn6.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btn6.FlatAppearance.BorderSize = 0;
            this.btn6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn6.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn6.ForeColor = System.Drawing.Color.White;
            this.btn6.Location = new System.Drawing.Point(820, 194);
            this.btn6.Name = "btn6";
            this.btn6.Size = new System.Drawing.Size(90, 70);
            this.btn6.TabIndex = 70;
            this.btn6.Text = "6";
            this.btn6.UseVisualStyleBackColor = false;
            this.btn6.Click += new System.EventHandler(this.btn6_Click);
            // 
            // btn7
            // 
            this.btn7.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btn7.FlatAppearance.BorderSize = 0;
            this.btn7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn7.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn7.ForeColor = System.Drawing.Color.White;
            this.btn7.Location = new System.Drawing.Point(620, 274);
            this.btn7.Name = "btn7";
            this.btn7.Size = new System.Drawing.Size(90, 70);
            this.btn7.TabIndex = 71;
            this.btn7.Text = "7";
            this.btn7.UseVisualStyleBackColor = false;
            this.btn7.Click += new System.EventHandler(this.btn7_Click);
            // 
            // btn8
            // 
            this.btn8.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btn8.FlatAppearance.BorderSize = 0;
            this.btn8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn8.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn8.ForeColor = System.Drawing.Color.White;
            this.btn8.Location = new System.Drawing.Point(720, 274);
            this.btn8.Name = "btn8";
            this.btn8.Size = new System.Drawing.Size(90, 70);
            this.btn8.TabIndex = 72;
            this.btn8.Text = "8";
            this.btn8.UseVisualStyleBackColor = false;
            this.btn8.Click += new System.EventHandler(this.btn8_Click);
            // 
            // btn9
            // 
            this.btn9.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btn9.FlatAppearance.BorderSize = 0;
            this.btn9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn9.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn9.ForeColor = System.Drawing.Color.White;
            this.btn9.Location = new System.Drawing.Point(820, 274);
            this.btn9.Name = "btn9";
            this.btn9.Size = new System.Drawing.Size(90, 70);
            this.btn9.TabIndex = 73;
            this.btn9.Text = "9";
            this.btn9.UseVisualStyleBackColor = false;
            this.btn9.Click += new System.EventHandler(this.btn9_Click);
            // 
            // btn0
            // 
            this.btn0.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn0.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btn0.FlatAppearance.BorderSize = 0;
            this.btn0.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn0.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn0.ForeColor = System.Drawing.Color.White;
            this.btn0.Location = new System.Drawing.Point(620, 354);
            this.btn0.Name = "btn0";
            this.btn0.Size = new System.Drawing.Size(90, 70);
            this.btn0.TabIndex = 74;
            this.btn0.Text = "0";
            this.btn0.UseVisualStyleBackColor = false;
            this.btn0.Click += new System.EventHandler(this.btn0_Click);
            // 
            // btnBorrar
            // 
            this.btnBorrar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnBorrar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnBorrar.FlatAppearance.BorderSize = 0;
            this.btnBorrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBorrar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBorrar.ForeColor = System.Drawing.Color.White;
            this.btnBorrar.Location = new System.Drawing.Point(720, 354);
            this.btnBorrar.Name = "btnBorrar";
            this.btnBorrar.Size = new System.Drawing.Size(90, 70);
            this.btnBorrar.TabIndex = 75;
            this.btnBorrar.Text = "Borrar";
            this.btnBorrar.UseVisualStyleBackColor = false;
            this.btnBorrar.Click += new System.EventHandler(this.btnBorrar_Click_1);
            // 
            // btnAgregar
            // 
            this.btnAgregar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnAgregar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnAgregar.FlatAppearance.BorderSize = 0;
            this.btnAgregar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAgregar.ForeColor = System.Drawing.Color.White;
            this.btnAgregar.Location = new System.Drawing.Point(820, 354);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(115, 70);
            this.btnAgregar.TabIndex = 33;
            this.btnAgregar.Text = "Agregar";
            this.btnAgregar.UseVisualStyleBackColor = false;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            // 
            // lbl200Disponible
            // 
            this.lbl200Disponible.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl200Disponible.AutoSize = true;
            this.lbl200Disponible.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl200Disponible.ForeColor = System.Drawing.Color.LightGray;
            this.lbl200Disponible.Location = new System.Drawing.Point(357, 88);
            this.lbl200Disponible.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl200Disponible.Name = "lbl200Disponible";
            this.lbl200Disponible.Size = new System.Drawing.Size(21, 20);
            this.lbl200Disponible.TabIndex = 77;
            this.lbl200Disponible.Text = "\"\"";
            // 
            // lbl1Disponible
            // 
            this.lbl1Disponible.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl1Disponible.AutoSize = true;
            this.lbl1Disponible.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl1Disponible.ForeColor = System.Drawing.Color.LightGray;
            this.lbl1Disponible.Location = new System.Drawing.Point(357, 298);
            this.lbl1Disponible.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl1Disponible.Name = "lbl1Disponible";
            this.lbl1Disponible.Size = new System.Drawing.Size(21, 20);
            this.lbl1Disponible.TabIndex = 88;
            this.lbl1Disponible.Text = "\"\"";
            // 
            // lbl5Disponible
            // 
            this.lbl5Disponible.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl5Disponible.AutoSize = true;
            this.lbl5Disponible.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl5Disponible.ForeColor = System.Drawing.Color.LightGray;
            this.lbl5Disponible.Location = new System.Drawing.Point(357, 263);
            this.lbl5Disponible.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl5Disponible.Name = "lbl5Disponible";
            this.lbl5Disponible.Size = new System.Drawing.Size(21, 20);
            this.lbl5Disponible.TabIndex = 87;
            this.lbl5Disponible.Text = "\"\"";
            // 
            // lbl10Disponible
            // 
            this.lbl10Disponible.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl10Disponible.AutoSize = true;
            this.lbl10Disponible.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl10Disponible.ForeColor = System.Drawing.Color.LightGray;
            this.lbl10Disponible.Location = new System.Drawing.Point(357, 228);
            this.lbl10Disponible.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl10Disponible.Name = "lbl10Disponible";
            this.lbl10Disponible.Size = new System.Drawing.Size(21, 20);
            this.lbl10Disponible.TabIndex = 86;
            this.lbl10Disponible.Text = "\"\"";
            // 
            // lbl20Disponible
            // 
            this.lbl20Disponible.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl20Disponible.AutoSize = true;
            this.lbl20Disponible.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl20Disponible.ForeColor = System.Drawing.Color.LightGray;
            this.lbl20Disponible.Location = new System.Drawing.Point(357, 195);
            this.lbl20Disponible.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl20Disponible.Name = "lbl20Disponible";
            this.lbl20Disponible.Size = new System.Drawing.Size(21, 20);
            this.lbl20Disponible.TabIndex = 85;
            this.lbl20Disponible.Text = "\"\"";
            // 
            // lbl50Disponible
            // 
            this.lbl50Disponible.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl50Disponible.AutoSize = true;
            this.lbl50Disponible.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl50Disponible.ForeColor = System.Drawing.Color.LightGray;
            this.lbl50Disponible.Location = new System.Drawing.Point(357, 158);
            this.lbl50Disponible.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl50Disponible.Name = "lbl50Disponible";
            this.lbl50Disponible.Size = new System.Drawing.Size(21, 20);
            this.lbl50Disponible.TabIndex = 84;
            this.lbl50Disponible.Text = "\"\"";
            // 
            // lbl100Disponible
            // 
            this.lbl100Disponible.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl100Disponible.AutoSize = true;
            this.lbl100Disponible.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl100Disponible.ForeColor = System.Drawing.Color.LightGray;
            this.lbl100Disponible.Location = new System.Drawing.Point(357, 123);
            this.lbl100Disponible.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl100Disponible.Name = "lbl100Disponible";
            this.lbl100Disponible.Size = new System.Drawing.Size(21, 20);
            this.lbl100Disponible.TabIndex = 83;
            this.lbl100Disponible.Text = "\"\"";
            // 
            // lblTotalActual
            // 
            this.lblTotalActual.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblTotalActual.AutoSize = true;
            this.lblTotalActual.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalActual.ForeColor = System.Drawing.Color.LightGray;
            this.lblTotalActual.Location = new System.Drawing.Point(357, 334);
            this.lblTotalActual.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTotalActual.Name = "lblTotalActual";
            this.lblTotalActual.Size = new System.Drawing.Size(21, 20);
            this.lblTotalActual.TabIndex = 89;
            this.lblTotalActual.Text = "\"\"";
            // 
            // FormAgregarEfectivo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(40)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.lblTotalActual);
            this.Controls.Add(this.lbl1Disponible);
            this.Controls.Add(this.lbl5Disponible);
            this.Controls.Add(this.lbl10Disponible);
            this.Controls.Add(this.lbl20Disponible);
            this.Controls.Add(this.lbl50Disponible);
            this.Controls.Add(this.lbl100Disponible);
            this.Controls.Add(this.lbl200Disponible);
            this.Controls.Add(this.btnBorrar);
            this.Controls.Add(this.btn0);
            this.Controls.Add(this.btn9);
            this.Controls.Add(this.btn8);
            this.Controls.Add(this.btn7);
            this.Controls.Add(this.btn6);
            this.Controls.Add(this.btn5);
            this.Controls.Add(this.btn4);
            this.Controls.Add(this.btn3);
            this.Controls.Add(this.btn2);
            this.Controls.Add(this.btn1);
            this.Controls.Add(this.btnVolver);
            this.Controls.Add(this.numeric1);
            this.Controls.Add(this.numeric5);
            this.Controls.Add(this.numeric10);
            this.Controls.Add(this.numeric20);
            this.Controls.Add(this.numeric50);
            this.Controls.Add(this.numeric100);
            this.Controls.Add(this.numeric200);
            this.Controls.Add(this.lblMensaje);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.btnAgregar);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblInstrucciones);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormAgregarEfectivo";
            this.Text = "Cajero Automático - Julian Antonio Monterroso González_202342200";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.numeric200)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric100)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric50)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric20)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric10)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMensaje;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Button btnAgregar;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblInstrucciones;
        private System.Windows.Forms.NumericUpDown numeric1;
        private System.Windows.Forms.NumericUpDown numeric5;
        private System.Windows.Forms.NumericUpDown numeric10;
        private System.Windows.Forms.NumericUpDown numeric20;
        private System.Windows.Forms.NumericUpDown numeric50;
        private System.Windows.Forms.NumericUpDown numeric100;
        private System.Windows.Forms.NumericUpDown numeric200;
        private System.Windows.Forms.Button btnVolver;
        private System.Windows.Forms.Button btnBorrar;
        private System.Windows.Forms.Button btn0;
        private System.Windows.Forms.Button btn9;
        private System.Windows.Forms.Button btn8;
        private System.Windows.Forms.Button btn7;
        private System.Windows.Forms.Button btn6;
        private System.Windows.Forms.Button btn5;
        private System.Windows.Forms.Button btn4;
        private System.Windows.Forms.Button btn3;
        private System.Windows.Forms.Button btn2;
        private System.Windows.Forms.Button btn1;
        private System.Windows.Forms.Label lbl200Disponible;
        private System.Windows.Forms.Label lbl1Disponible;
        private System.Windows.Forms.Label lbl5Disponible;
        private System.Windows.Forms.Label lbl10Disponible;
        private System.Windows.Forms.Label lbl20Disponible;
        private System.Windows.Forms.Label lbl50Disponible;
        private System.Windows.Forms.Label lbl100Disponible;
        private System.Windows.Forms.Label lblTotalActual;
    }
}