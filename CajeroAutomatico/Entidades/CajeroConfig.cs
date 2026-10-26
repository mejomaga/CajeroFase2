using System.Collections.Generic;
using System.Linq;
using System;

namespace CajeroAutomatico.Entidades
{
    public class CajeroConfig
    {
        public EstadoCajero Estado { get; set; } = EstadoCajero.Inactivo;
        public bool Inicializado { get; set; } = false;
        public DateTime FechaInicializacion { get; set; }

        // denominación -> cantidad de billetes disponibles en el cajero
        public Dictionary<int, int> Denominaciones { get; set; } = new Dictionary<int, int>
        {
            { 200, 0 }, { 100, 0 }, { 50, 0 }, { 20, 0 }, { 10, 0 }, { 5, 0 }, { 1, 0 }
        };

        public decimal Total() => Denominaciones.Sum(kv => (decimal)kv.Key * kv.Value);
    }
}
