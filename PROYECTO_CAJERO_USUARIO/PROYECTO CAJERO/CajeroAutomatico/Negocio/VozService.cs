using System;
using System.Linq;
using System.Speech.Synthesis;

namespace CajeroAutomatico.Negocio
{
    public static class VozService
    {
        private static readonly SpeechSynthesizer sintetizador;

        static VozService()
        {
            sintetizador = new SpeechSynthesizer();
            sintetizador.Volume = 100; // Volumen al máximo (0 a 100)
            sintetizador.Rate = 0;     // Velocidad natural (-10 a 10)

            // Intentar seleccionar una voz en español instalada en Windows (ej. Helena, Sabina)
            var vozEspanol = sintetizador.GetInstalledVoices()
                .FirstOrDefault(v => v.Enabled && v.VoiceInfo.Culture.TwoLetterISOLanguageName.Equals("es", StringComparison.OrdinalIgnoreCase));

            if (vozEspanol != null)
            {
                sintetizador.SelectVoice(vozEspanol.VoiceInfo.Name);
            }
        }

        public static void Hablar(string mensaje)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(mensaje)) return;

                // Cancela cualquier locución previa que todavía esté sonando
                sintetizador.SpeakAsyncCancelAll();

                // Habla de forma asíncrona para no congelar los Forms ni la UI
                sintetizador.SpeakAsync(mensaje);
            }
            catch
            {
                // Si la terminal no tiene tarjeta de sonido conectada, continúa sin error
            }
        }

        public static void Detener()
        {
            try
            {
                sintetizador.SpeakAsyncCancelAll();
            }
            catch { }
        }
    }
}