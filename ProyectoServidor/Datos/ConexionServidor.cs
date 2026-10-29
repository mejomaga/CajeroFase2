using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace ProyectoServidor.Datos
{
    public class ConexionServidor
    {
        // Cadena de conexión a la Base de Datos
        public string CadenaConexion => _cadenaConexion;
        private readonly string _cadenaConexion =
            System.Configuration.ConfigurationManager.ConnectionStrings["BD_CajeroRed"]?.ConnectionString
            ?? System.Configuration.ConfigurationManager.AppSettings["CadenaConexion"]
            ?? @"Server=.\SQLEXPRESS;Database=BD_CajeroRed;Trusted_Connection=True;TrustServerCertificate=True;";



        public static string CalcularSha256(string texto)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(texto));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }


        public bool ValidarCredenciales(string numeroTarjeta, string pinPlano,
    out int idUsuario, out int idTarjeta, out string codigoRespuesta, out string mensajeError)
        {
            idUsuario = 0;
            idTarjeta = 0;
            codigoRespuesta = "96";
            mensajeError = string.Empty;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    using (SqlCommand comando = new SqlCommand("sp_ValidarCredenciales", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.AddWithValue("@NumeroTarjeta", numeroTarjeta);

                        // Soporte hibrido: detecta si la BD en este equipo espera @PinHash o @PinPlano
                        bool usaPinHash = false;
                        using (SqlCommand cmdCheck = new SqlCommand("SELECT COUNT(1) FROM sys.parameters WHERE object_id = OBJECT_ID('sp_ValidarCredenciales') AND name = '@PinHash'", conexion))
                        {
                            usaPinHash = Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0;
                        }
                        if (usaPinHash)
                        {
                            comando.Parameters.AddWithValue("@PinHash", CalcularSha256(pinPlano));
                        }
                        else
                        {
                            comando.Parameters.AddWithValue("@PinPlano", pinPlano);
                        }

                        var pIdUsuario = new SqlParameter("@IdUsuarioOut", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        var pIdTarjeta = new SqlParameter("@IdTarjetaOut", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        var pCodigo = new SqlParameter("@CodigoRespuesta", SqlDbType.NVarChar, 5) { Direction = ParameterDirection.Output };

                        comando.Parameters.Add(pIdUsuario);
                        comando.Parameters.Add(pIdTarjeta);
                        comando.Parameters.Add(pCodigo);

                        comando.ExecuteNonQuery();

                        idUsuario = pIdUsuario.Value != DBNull.Value ? (int)pIdUsuario.Value : 0;
                        idTarjeta = pIdTarjeta.Value != DBNull.Value ? (int)pIdTarjeta.Value : 0;
                        codigoRespuesta = pCodigo.Value?.ToString() ?? "96";

                        if (codigoRespuesta == "00")
                            return true;

                        mensajeError = MapearCodigoError(codigoRespuesta);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Esto atrapará cualquier error y lo mostrará sin cerrar el socket a la fuerza
                mensajeError = $"Error BD: {ex.Message}";
                Console.WriteLine("EXCEPCIÓN EN VALIDAR CREDENCIALES: " + ex.ToString());
                return false;
            }
        }


        public bool RegistrarCajero(string codigoCajero, string nombre, string ip, out string codigoRespuesta, out string mensajeError)
        {
            codigoRespuesta = "96";
            mensajeError = string.Empty;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    using (SqlCommand comando = new SqlCommand("sp_RegistrarCajero", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@CodigoCajero", codigoCajero);
                        comando.Parameters.AddWithValue("@Nombre", nombre);
                        comando.Parameters.AddWithValue("@IP", ip);

                        var pCodigo = new SqlParameter("@CodigoRespuesta", SqlDbType.NVarChar, 5) { Direction = ParameterDirection.Output };
                        comando.Parameters.Add(pCodigo);

                        comando.ExecuteNonQuery();

                        codigoRespuesta = pCodigo.Value?.ToString() ?? "96";
                        if (codigoRespuesta == "00") return true;

                        mensajeError = MapearCodigoError(codigoRespuesta);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                mensajeError = $"Error BD: {ex.Message}";
                return false;
            }
        }

    
        public bool GenerarToken(int idUsuario, string codigoCajero, out string tokenGenerado, out string mensajeError)
        {
            tokenGenerado = string.Empty;
            mensajeError = string.Empty;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    using (SqlCommand comando = new SqlCommand("sp_GenerarToken", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@IdUsuario", idUsuario);
                        comando.Parameters.AddWithValue("@CodigoCajero", codigoCajero);

                        var pToken = new SqlParameter("@TokenGenerado", SqlDbType.NVarChar, 6) { Direction = ParameterDirection.Output };
                        comando.Parameters.Add(pToken);

                        comando.ExecuteNonQuery();

                        tokenGenerado = pToken.Value?.ToString() ?? string.Empty;
                        return !string.IsNullOrEmpty(tokenGenerado);
                    }
                }
            }
            catch (Exception ex)
            {
                mensajeError = $"Error BD: {ex.Message}";
                return false;
            }
        }

        
        public bool ValidarToken(int idUsuario, string codigoCajero, string tokenIngresado, out string codigoRespuesta, out string mensajeError)
        {
            codigoRespuesta = "96";
            mensajeError = string.Empty;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    using (SqlCommand comando = new SqlCommand("sp_ValidarToken", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@IdUsuario", idUsuario);
                        comando.Parameters.AddWithValue("@CodigoCajero", codigoCajero);
                        comando.Parameters.AddWithValue("@TokenIngresado", tokenIngresado);

                        var pCodigo = new SqlParameter("@CodigoRespuesta", SqlDbType.NVarChar, 5) { Direction = ParameterDirection.Output };
                        var pMensaje = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                        comando.Parameters.Add(pCodigo);
                        comando.Parameters.Add(pMensaje);

                        comando.ExecuteNonQuery();

                        codigoRespuesta = pCodigo.Value?.ToString() ?? "96";
                        if (codigoRespuesta == "00") return true;

                        mensajeError = pMensaje.Value?.ToString() ?? MapearCodigoError(codigoRespuesta);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                mensajeError = $"Error BD: {ex.Message}";
                return false;
            }
        }

      
        public bool ProcesarRetiro(int idUsuario, int idTarjeta, string codigoCajero, decimal monto,
            Dictionary<int, int> desglose, out int idTransaccion, out string codigoRespuesta, out string mensajeError)
        {
            idTransaccion = 0;
            codigoRespuesta = "96";
            mensajeError = string.Empty;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    using (SqlCommand comando = new SqlCommand("sp_Retiro", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@IdUsuario", idUsuario);
                        comando.Parameters.AddWithValue("@IdTarjeta", idTarjeta);
                        comando.Parameters.AddWithValue("@CodigoCajero", codigoCajero);
                        comando.Parameters.AddWithValue("@Monto", monto);

                        // Desglose: si el diccionario no tiene la llave, se manda 0
                        comando.Parameters.AddWithValue("@B200", desglose.ContainsKey(200) ? desglose[200] : 0);
                        comando.Parameters.AddWithValue("@B100", desglose.ContainsKey(100) ? desglose[100] : 0);
                        comando.Parameters.AddWithValue("@B50", desglose.ContainsKey(50) ? desglose[50] : 0);
                        comando.Parameters.AddWithValue("@B20", desglose.ContainsKey(20) ? desglose[20] : 0);
                        comando.Parameters.AddWithValue("@B10", desglose.ContainsKey(10) ? desglose[10] : 0);
                        comando.Parameters.AddWithValue("@B5", desglose.ContainsKey(5) ? desglose[5] : 0);
                        comando.Parameters.AddWithValue("@B1", desglose.ContainsKey(1) ? desglose[1] : 0);

                        var pCodigo = new SqlParameter("@CodigoRespuesta", SqlDbType.NVarChar, 5) { Direction = ParameterDirection.Output };
                        var pMensaje = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                        var pIdTx = new SqlParameter("@IdTransaccionOut", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        comando.Parameters.Add(pCodigo);
                        comando.Parameters.Add(pMensaje);
                        comando.Parameters.Add(pIdTx);

                        comando.ExecuteNonQuery();

                        codigoRespuesta = pCodigo.Value?.ToString() ?? "96";
                        mensajeError = pMensaje.Value?.ToString() ?? string.Empty;
                        idTransaccion = pIdTx.Value != DBNull.Value ? (int)pIdTx.Value : 0;

                        return codigoRespuesta == "00";
                    }
                }
            }
            catch (Exception ex)
            {
                mensajeError = $"Error BD: {ex.Message}";
                return false;
            }
        }

        public bool ProcesarDeposito(int idUsuario, int idTarjeta, string codigoCajero, decimal monto,
            Dictionary<int, int> desglose, out int idTransaccion, out string codigoRespuesta, out string mensajeError)
        {
            idTransaccion = 0;
            codigoRespuesta = "96";
            mensajeError = string.Empty;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    using (SqlCommand comando = new SqlCommand("sp_Deposito", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@IdUsuario", idUsuario);
                        comando.Parameters.AddWithValue("@IdTarjeta", idTarjeta);
                        comando.Parameters.AddWithValue("@CodigoCajero", codigoCajero);
                        comando.Parameters.AddWithValue("@Monto", monto);

                        comando.Parameters.AddWithValue("@B200", desglose.ContainsKey(200) ? desglose[200] : 0);
                        comando.Parameters.AddWithValue("@B100", desglose.ContainsKey(100) ? desglose[100] : 0);
                        comando.Parameters.AddWithValue("@B50", desglose.ContainsKey(50) ? desglose[50] : 0);
                        comando.Parameters.AddWithValue("@B20", desglose.ContainsKey(20) ? desglose[20] : 0);
                        comando.Parameters.AddWithValue("@B10", desglose.ContainsKey(10) ? desglose[10] : 0);
                        comando.Parameters.AddWithValue("@B5", desglose.ContainsKey(5) ? desglose[5] : 0);
                        comando.Parameters.AddWithValue("@B1", desglose.ContainsKey(1) ? desglose[1] : 0);

                        var pCodigo = new SqlParameter("@CodigoRespuesta", SqlDbType.NVarChar, 5) { Direction = ParameterDirection.Output };
                        var pMensaje = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                        var pIdTx = new SqlParameter("@IdTransaccionOut", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        comando.Parameters.Add(pCodigo);
                        comando.Parameters.Add(pMensaje);
                        comando.Parameters.Add(pIdTx);

                        comando.ExecuteNonQuery();

                        codigoRespuesta = pCodigo.Value?.ToString() ?? "96";
                        mensajeError = pMensaje.Value?.ToString() ?? string.Empty;
                        idTransaccion = pIdTx.Value != DBNull.Value ? (int)pIdTx.Value : 0;

                        return codigoRespuesta == "00";
                    }
                }
            }
            catch (Exception ex)
            {
                mensajeError = $"Error BD: {ex.Message}";
                return false;
            }
        }

 
        public bool CambiarPin(int idUsuario, string codigoCajero, string origen, string nuevoPinPlano,
            out string codigoRespuesta, out string mensajeError)
        {
            codigoRespuesta = "96";
            mensajeError = string.Empty;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    using (SqlCommand comando = new SqlCommand("sp_CambiarPin", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@IdUsuario", idUsuario);
                        comando.Parameters.AddWithValue("@CodigoCajero", (object)codigoCajero ?? DBNull.Value);
                        comando.Parameters.AddWithValue("@Origen", origen);
                        comando.Parameters.AddWithValue("@NuevoPinHash", CalcularSha256(nuevoPinPlano + "salt123"));
                        comando.Parameters.AddWithValue("@NuevoPinSalt", "salt123"); 

                        var pCodigo = new SqlParameter("@CodigoRespuesta", SqlDbType.NVarChar, 5) { Direction = ParameterDirection.Output };
                        var pMensaje = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                        comando.Parameters.Add(pCodigo);
                        comando.Parameters.Add(pMensaje);

                        comando.ExecuteNonQuery();

                        codigoRespuesta = pCodigo.Value?.ToString() ?? "96";
                        mensajeError = pMensaje.Value?.ToString() ?? string.Empty;
                        return codigoRespuesta == "00";
                    }
                }
            }
            catch (Exception ex)
            {
                mensajeError = $"Error BD: {ex.Message}";
                return false;
            }
        }

        public bool ConsultarSaldo(int idUsuario, out decimal saldo, out decimal limiteDiario, out decimal disponibleHoy, out string mensajeError)
        {
            saldo = 0; limiteDiario = 0; disponibleHoy = 0;
            mensajeError = string.Empty;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    using (SqlCommand comando = new SqlCommand("sp_ConsultaUsuario", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@IdUsuario", idUsuario);

                        using (SqlDataReader reader = comando.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                saldo = reader.GetDecimal(0);
                                limiteDiario = reader.GetDecimal(1);
                                disponibleHoy = reader.GetDecimal(2);
                                return true;
                            }
                        }
                    }
                }
                mensajeError = "Usuario no encontrado";
                return false;
            }
            catch (Exception ex)
            {
                mensajeError = $"Error BD: {ex.Message}";
                return false;
            }
        }


        public bool InicializarCajero(string codigoCajero, int idAdmin, decimal total,
            Dictionary<int, int> desglose, out string codigoRespuesta, out string mensajeError)
        {
            codigoRespuesta = "96";
            mensajeError = string.Empty;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    using (SqlCommand comando = new SqlCommand("sp_InicializarCajero", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@CodigoCajero", codigoCajero);
                        comando.Parameters.AddWithValue("@IdAdmin", idAdmin);
                        comando.Parameters.AddWithValue("@Total", total);
                        comando.Parameters.AddWithValue("@B200", desglose.ContainsKey(200) ? desglose[200] : 0);
                        comando.Parameters.AddWithValue("@B100", desglose.ContainsKey(100) ? desglose[100] : 0);
                        comando.Parameters.AddWithValue("@B50", desglose.ContainsKey(50) ? desglose[50] : 0);
                        comando.Parameters.AddWithValue("@B20", desglose.ContainsKey(20) ? desglose[20] : 0);
                        comando.Parameters.AddWithValue("@B10", desglose.ContainsKey(10) ? desglose[10] : 0);
                        comando.Parameters.AddWithValue("@B5", desglose.ContainsKey(5) ? desglose[5] : 0);
                        comando.Parameters.AddWithValue("@B1", desglose.ContainsKey(1) ? desglose[1] : 0);

                        var pCodigo = new SqlParameter("@CodigoRespuesta", SqlDbType.NVarChar, 5) { Direction = ParameterDirection.Output };
                        var pMensaje = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                        comando.Parameters.Add(pCodigo);
                        comando.Parameters.Add(pMensaje);

                        comando.ExecuteNonQuery();

                        codigoRespuesta = pCodigo.Value?.ToString() ?? "96";
                        mensajeError = pMensaje.Value?.ToString() ?? string.Empty;
                        return codigoRespuesta == "00";
                    }
                }
            }
            catch (Exception ex)
            {
                mensajeError = $"Error BD: {ex.Message}";
                return false;
            }
        }


        public bool MarcarCajeroDesconectado(string codigoCajero)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    using (SqlCommand comando = new SqlCommand(
                        "UPDATE Cajero SET Estado = 'Desconectado' WHERE CodigoCajero = @Cod AND Estado = 'En línea'", conexion))
                    {
                        comando.Parameters.AddWithValue("@Cod", codigoCajero);
                        return comando.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch { return false; }
        }
        public bool ActualizarHeartbeat(string codigoCajero)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    // Cambiamos 'Conectado' o 'Activo' por el valor exacto que exige la BD: 'En línea'
                    using (SqlCommand comando = new SqlCommand(
                        "UPDATE Cajero SET UltimoHeartbeat = GETDATE(), Estado = 'En línea' WHERE CodigoCajero = @Codigo", conexion))
                    {
                        comando.Parameters.AddWithValue("@Codigo", codigoCajero);
                        return comando.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch { return false; }
        }


        public List<Dictionary<string, object>> ObtenerUltimasTransacciones(int idUsuario, int cantidad = 5)
        {
            var lista = new List<Dictionary<string, object>>();
            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    string sql = @"
                        SELECT TOP (@Cantidad) 
                            t.IdTransaccion, t.Tipo, t.Monto, t.Fecha, t.CodigoCajero, t.CodigoRespuesta
                        FROM Transaccion t
                        WHERE t.IdUsuario = @IdUsuario
                        ORDER BY t.Fecha DESC";

                    using (SqlCommand comando = new SqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@Cantidad", cantidad);
                        comando.Parameters.AddWithValue("@IdUsuario", idUsuario);

                        using (SqlDataReader reader = comando.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var fila = new Dictionary<string, object>
                                {
                                    { "idTransaccion", reader.GetInt32(0) },
                                    { "tipo", reader.GetString(1) },
                                    { "monto", reader.GetDecimal(2) },
                                    { "fecha", reader.GetDateTime(3).ToString("yyyy-MM-ddTHH:mm:ss") },
                                    { "cajero", reader.GetString(4) },
                                    { "codigo", reader.GetString(5) }
                                };
                                lista.Add(fila);
                            }
                        }
                    }
                }
            }
            catch { }
            return lista;
        }

        private string MapearCodigoError(string codigo)
        {
            switch (codigo)
            {
                case "51": return "Saldo insuficiente";
                case "55": return "PIN incorrecto";
                case "56": return "Token inválido o expirado";
                case "61": return "Excede el límite diario disponible";
                case "75": return "Tarjeta bloqueada o inactiva";
                case "91": return "Cajero no inicializado o fuera de servicio";
                case "92": return "El cajero no tiene los billetes necesarios";
                case "93": return "Excede la capacidad de efectivo del cajero";
                case "96": return "Error del sistema; la operación no se aplicó";
                default: return $"Código desconocido: {codigo}";
            }
        }

        // Método temporal solo para probar la conexión desde el Form1
        public bool ProbarConexion(out string mensaje)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    mensaje = "¡Conexión exitosa a BD_CajeroRed!";
                    return true;
                }
            }
            catch (Exception ex)
            {
                mensaje = "Error de conexión: " + ex.Message;
                return false;
            }
        }
    }

}

