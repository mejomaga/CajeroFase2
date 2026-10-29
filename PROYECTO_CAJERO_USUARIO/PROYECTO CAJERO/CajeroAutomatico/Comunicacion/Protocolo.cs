using System;
using System.Collections.Generic;

namespace CajeroAutomatico.Comunicacion
{
    // ---------------------------------------------------------------------
    // Protocolo de comunicación Cliente (cajero) <-> Servidor central.
    //
    // Transporte: TCP (TcpClient / NetworkStream).
    // Formato:    1 objeto JSON por mensaje, terminado en '\n' (newline-delimited).
    //             El cliente abre la conexión, envía una Solicitud y espera
    //             exactamente una Respuesta antes de enviar la siguiente.
    //
    // El servidor es la ÚNICA autoridad sobre usuarios, saldos, PIN, huellas,
    // límites diarios, denominaciones de efectivo y transacciones. El cliente
    // nunca decide si una operación es válida: solo arma la Solicitud, la
    // envía y muestra lo que el servidor responde.
    // ---------------------------------------------------------------------

    public static class Operaciones
    {
        public const string RegistrarCajero = "REGISTRAR_CAJERO";
        public const string Heartbeat = "HEARTBEAT";
        public const string Login = "LOGIN";
        public const string ValidarToken = "VALIDAR_TOKEN";
        public const string ConsultarSaldo = "CONSULTA_SALDO";
        public const string Retirar = "RETIRO";
        public const string Depositar = "DEPOSITO";
        public const string CambiarPin = "CAMBIO_PIN";
        public const string UltimasTransacciones = "ULTIMAS_TRANSACCIONES";
        public const string CerrarSesion = "LOGOUT";
        public const string EstadoCajero = "ESTADO_CAJERO";
    }

    // Mensaje que viaja del cliente hacia el servidor.
    public class SolicitudRed
    {
        public string Operacion { get; set; }
        public Dictionary<string, object> Datos { get; set; } = new Dictionary<string, object>();
    }

    // Mensaje que viaja del servidor hacia el cliente.
    public class RespuestaRed
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = "";
        public Dictionary<string, object> Datos { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        // Se usa cuando ni siquiera se pudo hablar con el servidor
        // (sin red, servidor caído, timeout, etc.), para que el Form
        // muestre el mismo tipo de mensaje que un rechazo normal.
        public static RespuestaRed ErrorConexion(string mensaje) =>
            new RespuestaRed { Exito = false, Mensaje = mensaje };

        public static RespuestaRed DesdeDiccionario(Dictionary<string, object> dict)
        {
            if (dict == null) return ErrorConexion("Respuesta vacía del servidor.");

            var respuesta = new RespuestaRed
            {
                Exito = (dict.ContainsKey("Exito") && Convert.ToBoolean(dict["Exito"]))
                     || (dict.ContainsKey("exito") && Convert.ToBoolean(dict["exito"])),
                Mensaje = dict.ContainsKey("Mensaje") ? dict["Mensaje"]?.ToString() 
                        : (dict.ContainsKey("mensaje") ? dict["mensaje"]?.ToString() : "")
            };

            if (dict.ContainsKey("Datos") && dict["Datos"] is Dictionary<string, object> datos)
                respuesta.Datos = new Dictionary<string, object>(datos, StringComparer.OrdinalIgnoreCase);
            else if (dict.ContainsKey("datos") && dict["datos"] is Dictionary<string, object> datosMin)
                respuesta.Datos = new Dictionary<string, object>(datosMin, StringComparer.OrdinalIgnoreCase);

            return respuesta;
        }

        // ---------- Helpers para leer "Datos" sin pelear con tipos de JSON ----------
        public string Texto(string clave, string porDefecto = "")
        {
            if (!Datos.ContainsKey(clave) || Datos[clave] == null) return porDefecto;
            if (Datos[clave] is System.Text.Json.JsonElement elem)
                return elem.ToString();
            return Datos[clave].ToString();
        }

        public bool Booleano(string clave, bool porDefecto = false)
        {
            if (!Datos.ContainsKey(clave) || Datos[clave] == null) return porDefecto;
            if (Datos[clave] is System.Text.Json.JsonElement elem)
            {
                if (elem.ValueKind == System.Text.Json.JsonValueKind.True) return true;
                if (elem.ValueKind == System.Text.Json.JsonValueKind.False) return false;
                if (bool.TryParse(elem.ToString(), out bool bVal)) return bVal;
                return porDefecto;
            }
            try
            {
                return Convert.ToBoolean(Datos[clave].ToString());
            }
            catch
            {
                return porDefecto;
            }
        }

        public int Entero(string clave, int porDefecto = 0)
        {
            if (!Datos.ContainsKey(clave) || Datos[clave] == null) return porDefecto;
            if (Datos[clave] is System.Text.Json.JsonElement elem)
            {
                if (elem.TryGetInt32(out int iVal)) return iVal;
                if (int.TryParse(elem.ToString(), out int pVal)) return pVal;
                return porDefecto;
            }
            return int.TryParse(Datos[clave].ToString(), out int parsed) ? parsed : porDefecto;
        }

        public decimal Decimal(string clave, decimal porDefecto = 0m)
        {
            if (!Datos.ContainsKey(clave) || Datos[clave] == null) return porDefecto;
            if (Datos[clave] is System.Text.Json.JsonElement elem)
            {
                if (elem.TryGetDecimal(out decimal dVal)) return dVal;
                if (decimal.TryParse(elem.ToString(), out decimal pVal)) return pVal;
                return porDefecto;
            }
            return decimal.TryParse(Datos[clave].ToString(), out decimal parsed) ? parsed : porDefecto;
        }
    }
}
