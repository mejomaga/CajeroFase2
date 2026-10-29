using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CajeroAutomatico.Presentacion
{
    public partial class FormVoucher : Form
    {
        public FormVoucher()
        {
            InitializeComponent();
            timerVoucher.Interval = 5000; // 3 segundos
        }

        private void timerVoucher_Tick(object sender, EventArgs e)
        {
            timerVoucher.Stop();
            timerVoucher.Enabled = false;

            timerVoucher.Tick -= timerVoucher_Tick; 

            timerVoucher.Dispose();

            Program.Navegar(this, new FormFinProceso());
            this.Close();
        }

        private void FormVoucher_Load(object sender, EventArgs e)
        {
            timerVoucher.Start();
        }
    }
}