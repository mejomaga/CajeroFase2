using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProyectoServidor.Modelos
{
    // Esta clase base representa cualquier mensaje que llegue del cajero de Luis
    public class MensajeRed
    {
        [JsonPropertyName("tipo")]
        public string Tipo { get; set; }

        [JsonPropertyName("idSolicitud")]
        public string IdSolicitud { get; set; }

        [JsonPropertyName("cajero")]
        public string Cajero { get; set; }

        [JsonPropertyName("sesion")]
        public string Sesion { get; set; }
    }

    // Esta clase hereda de MensajeRed, pero agrega los campos específicos para RETIRO y DEPOSITO
    public class MensajeTransaccion : MensajeRed
    {
        [JsonPropertyName("monto")]
        public decimal Monto { get; set; }

        [JsonPropertyName("desglose")]
        public Dictionary<string, int> Desglose { get; set; }
    }
}
