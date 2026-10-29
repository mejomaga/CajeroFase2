using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CajeroAutomatico.Presentacion
{
    // Los formularios que piden un desglose de billetes (Inicializar cajero,
    // Agregar efectivo, Retiro, Depósito) repiten el mismo patrón de
    // 7 pares Label+NumericUpDown (uno por denominación), así que ese
    // pedazo de interfaz se arma aquí una sola vez.
    internal class DenominacionesHelper
    {
        public static readonly int[] Denominaciones = { 200, 100, 50, 20, 10, 5, 1 };
        public static Dictionary<int, NumericUpDown> CrearControles(Control contenedor, int x, int yInicio, System.EventHandler alCambiar)
        {
            var controles = new Dictionary<int, NumericUpDown>();
            int y = yInicio;
            foreach (var d in Denominaciones)
            {
                var lbl = new Label { Text = $"Billetes de Q.{d}:", Location = new Point(x, y), Size = new Size(110, 25) };
                var num = new NumericUpDown { Minimum = 0, Maximum = 9999, Location = new Point(x + 120, y), Size = new Size(80, 25) };
                num.ValueChanged += alCambiar;
                contenedor.Controls.Add(lbl);
                contenedor.Controls.Add(num);
                controles[d] = num;
                y += 32;
            }
            return controles;
        }

        public static Dictionary<int, int> ObtenerDesglose(Dictionary<int, NumericUpDown> controles) =>
            controles.ToDictionary(kv => kv.Key, kv => (int)kv.Value.Value);

        public static decimal CalcularTotal(Dictionary<int, NumericUpDown> controles) =>
            controles.Sum(kv => (decimal)kv.Key * (int)kv.Value.Value);

        public static void Limpiar(Dictionary<int, NumericUpDown> controles)
        {
            foreach (var num in controles.Values) num.Value = 0;
        }
    }
}
