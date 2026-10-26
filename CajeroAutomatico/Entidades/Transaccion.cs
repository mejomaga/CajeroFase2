using System;
using System.Collections.Generic;

namespace CajeroAutomatico.Entidades
{
    public class Transaccion
    {
        public string NumeroTarjeta { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        public TipoTransaccion Tipo { get; set; }
        public decimal Monto { get; set; }

        // denominación -> cantidad de billetes usados en la transacción
        public Dictionary<int, int> Desglose { get; set; } = new Dictionary<int, int>();
    }
}
