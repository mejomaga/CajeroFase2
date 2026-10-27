using System;
using System.Collections.Concurrent;

namespace ProyectoServidor.Negocio
{
    // Clase que representa un token generado
    public class DatosToken
    {
        public string Codigo { get; set; }
        public string UsuarioId { get; set; }
        public string CajeroId { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool FueUsado { get; set; }
    }

    public class GestorToken
    {
        // Diccionario en memoria para almacenar los tokens temporales de forma concurrente
        private readonly ConcurrentDictionary<string, DatosToken> _tokensActivos;
        private readonly Random _generadorAleatorio;

        public GestorToken()
        {
            _tokensActivos = new ConcurrentDictionary<string, DatosToken>();
            _generadorAleatorio = new Random();
        }

        // Método para generar un nuevo token de 6 dígitos, válido por 2 minutos
        public string GenerarToken(string usuarioId, string cajeroId)
        {
            // 1. Genera un número aleatorio de 6 dígitos (000000 a 999999)
            string nuevoCodigo = _generadorAleatorio.Next(0, 1000000).ToString("D6");

            // 2. Crea el objeto con la regla de negocio: expiración en 2 minutos exactos
            var nuevoToken = new DatosToken
            {
                Codigo = nuevoCodigo,
                UsuarioId = usuarioId,
                CajeroId = cajeroId,
                FechaExpiracion = DateTime.Now.AddMinutes(2),
                FueUsado = false
            };

            // 3. Guarda el token en el diccionario, usando el código como llave
            _tokensActivos.AddOrUpdate(nuevoCodigo, nuevoToken, (key, oldValue) => nuevoToken);

            // Simulación visual en la consola del servidor (como lo pide la regla S-06)
            Console.WriteLine($"\n========================================");
            Console.WriteLine($"[SIMULADOR SMS] Token del usuario {usuarioId}");
            Console.WriteLine($"Tu código de seguridad es: {nuevoCodigo}");
            Console.WriteLine($"Válido por 2 minutos en el cajero {cajeroId}");
            Console.WriteLine($"========================================\n");

            return nuevoCodigo;
        }

        // Método para validar si un token ingresado por el cajero es correcto y no ha expirado
        public bool ValidarToken(string codigo, string usuarioId, string cajeroId)
        {
            // Busca si el código existe en la memoria
            if (_tokensActivos.TryGetValue(codigo, out DatosToken tokenAlmacenado))
            {
                // Validación 1: ¿Ya se usó? (Regla de un solo uso)
                if (tokenAlmacenado.FueUsado)
                {
                    Console.WriteLine($"[TOKEN RECHAZADO] El código {codigo} ya fue utilizado.");
                    return false;
                }

                // Validación 2: ¿Ya pasaron los 2 minutos? (Regla de tiempo)
                if (DateTime.Now > tokenAlmacenado.FechaExpiracion)
                {
                    Console.WriteLine($"[TOKEN RECHAZADO] El código {codigo} expiró a las {tokenAlmacenado.FechaExpiracion:HH:mm:ss}.");
                    return false;
                }

                // Validación 3: ¿Pertenece al usuario y cajero que lo solicitó?
                if (tokenAlmacenado.UsuarioId != usuarioId || tokenAlmacenado.CajeroId != cajeroId)
                {
                    Console.WriteLine($"[TOKEN RECHAZADO] El código {codigo} no pertenece a esta sesión.");
                    return false;
                }

                // ¡Si pasa todas las pruebas, marcamos el token como usado!
                tokenAlmacenado.FueUsado = true;
                return true;
            }

            // Si el código no existe en el diccionario
            return false;
        }
    }
}