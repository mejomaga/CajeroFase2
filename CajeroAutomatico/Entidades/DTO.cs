using System;
using System.Collections.Generic;

namespace CajeroAutomatico.Entidades
{
    // Resultado de "Pantalla Consulta de Usuario"
    public class ConsultaUsuarioDTO
    {
        public string Nombre { get; set; }
        public decimal MontoMaximoDisponible { get; set; }
        public decimal CantidadRetiradaHastaElMomento { get; set; }
        public decimal SaldoActual { get; set; }
        public DateTime UltimoAcceso { get; set; }
        public bool bloqueado { get; set; } // Indica si el usuario está bloqueado
    }

    // Resultado de "Pantalla Control de usuarios"
    public class ControlUsuariosDTO
    {
        public decimal TotalRetiradoTodos { get; set; }
        public decimal PromedioDepositado { get; set; }
        public List<string> UsuariosQueCambiaronPin { get; set; } = new List<string>();
        public string UltimoUsuarioAcceso { get; set; }
        public DateTime FechaUltimoAcceso { get; set; }
    }

    // Resultado de operaciones que pueden fallar por varias razones distintas
    // (saldo, límite, denominaciones); permite mostrar el mensaje correcto en el Form.
    public class ResultadoOperacion
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }

        public static ResultadoOperacion Ok(string mensaje = "") => new ResultadoOperacion { Exito = true, Mensaje = mensaje };
        public static ResultadoOperacion Error(string mensaje) => new ResultadoOperacion { Exito = false, Mensaje = mensaje };
    }
}
