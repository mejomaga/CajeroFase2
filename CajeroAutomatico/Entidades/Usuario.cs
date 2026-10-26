using System;
using System.Collections.Generic;

namespace CajeroAutomatico.Entidades
{
    // Representa tanto a un usuario normal como al administrador (ver Tipo).
    public class Usuario
    {
        public string NumeroTarjeta { get; set; }
        public string Nombre { get; set; }
        public string Pin { get; set; }
        public decimal Saldo { get; set; }
        public decimal LimiteDiario { get; set; }
        public decimal RetiradoHoy { get; set; }
        public DateTime FechaRetiroHoy { get; set; }
        public DateTime UltimoAcceso { get; set; }
        public int IntentosFallidos { get; set; }
        public bool Bloqueado { get; set; }
        public TipoUsuario Tipo { get; set; }
        public bool CambioPin { get; set; }

        // Guarda números de tarjeta anteriores para que, aunque el usuario
        // cambie de tarjeta, el límite diario se siga respetando.
        public List<string> HistorialTarjetas { get; set; } = new List<string>();

        public string HuellaDigital { get; set; } // Nueva propiedad para la huella digital
        public int HuellaSlot { get; set; } = -1;  // -1 = sin huella asignada
        public string UID { get; set; } // Nueva propiedad para el UID
        public decimal DisponibleHoy()
        {
            if (FechaRetiroHoy.Date != DateTime.Today) return LimiteDiario;
            return LimiteDiario - RetiradoHoy;
        }
    }
}
