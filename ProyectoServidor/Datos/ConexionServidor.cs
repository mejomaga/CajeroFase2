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
            System.Configuration.ConfigurationManager.ConnectionStrings["BD_CajeroRed_Actualizada"]?.ConnectionString
            ?? System.Configuration.ConfigurationManager.AppSettings["CadenaConexion"]
            ?? @"Server=Melqui\SQLEXPRESS;Database=BD_CajeroRed_Actualizada;Trusted_Connection=True;TrustServerCertificate=True;";

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
            out int idUsuario, out int idTarjeta, out string nombreUsuario, out string codigoRespuesta, out string mensajeError)
        {
            idUsuario = 0;
            idTarjeta = 0;
            nombreUsuario = string.Empty;
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

                        bool usaPinHash = false;
                        using (SqlCommand cmdCheck = new SqlCommand("SELECT COUNT(1) FROM sys.parameters WHERE object_id = OBJECT_ID('sp_ValidarCredenciales') AND name = '@PinHash'", conexion))
                        {
                            usaPinHash = Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0;
                        }

                        if (usaPinHash)
                        {
                            string hashParaEnviar = CalcularSha256(pinPlano);
                            using (SqlCommand cmdSalt = new SqlCommand("SELECT PinSalt, PinHash FROM Tarjeta WHERE NumeroTarjeta = @Num", conexion))
                            {
                                cmdSalt.Parameters.AddWithValue("@Num", numeroTarjeta);
                                using (var reader = cmdSalt.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        string salt = reader["PinSalt"]?.ToString() ?? "";
                                        string pinHashReal = reader["PinHash"]?.ToString() ?? "";
                                        if (!string.IsNullOrEmpty(salt) && pinHashReal.Equals(CalcularSha256(pinPlano + salt), StringComparison.OrdinalIgnoreCase))
                                        {
                                            hashParaEnviar = CalcularSha256(pinPlano + salt);
                                        }
                                    }
                                }
                            }
                            comando.Parameters.AddWithValue("@PinHash", hashParaEnviar);
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
                        {
                            using (SqlCommand cmdNombre = new SqlCommand("SELECT Nombre FROM Usuario WHERE IdUsuario = @Id", conexion))
                            {
                                cmdNombre.Parameters.AddWithValue("@Id", idUsuario);
                                nombreUsuario = cmdNombre.ExecuteScalar()?.ToString() ?? "Cliente";
                            }
                            return true;
                        }

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

        public bool GenerarTokenOperacion(int idUsuario, string codigoCajero,
            out string tokenGenerado, out string mensajeError)
        {
            tokenGenerado = string.Empty;
            mensajeError = string.Empty;

            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();

                using var comando = new SqlCommand("sp_GenerarToken", conexion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@IdUsuario", idUsuario);
                comando.Parameters.AddWithValue("@CodigoCajero", codigoCajero);

                var pToken = new SqlParameter("@TokenGenerado", SqlDbType.NVarChar, 6)
                { Direction = ParameterDirection.Output };
                comando.Parameters.Add(pToken);

                comando.ExecuteNonQuery();

                tokenGenerado = pToken.Value?.ToString() ?? string.Empty;
                return !string.IsNullOrEmpty(tokenGenerado);
            }
            catch (Exception ex)
            {
                mensajeError = $"Error BD: {ex.Message}";
                return false;
            }
        }

        public bool ValidarTokenOperacion(int idUsuario, string codigoCajero,
            string tokenIngresado, out string mensajeError)
        {
            mensajeError = string.Empty;

            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();

                using var comando = new SqlCommand("sp_ValidarToken", conexion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@IdUsuario", idUsuario);
                comando.Parameters.AddWithValue("@CodigoCajero", codigoCajero);
                comando.Parameters.AddWithValue("@TokenIngresado", tokenIngresado);

                var pCodigo = new SqlParameter("@CodigoRespuesta", SqlDbType.NVarChar, 5)
                { Direction = ParameterDirection.Output };
                var pMensaje = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 100)
                { Direction = ParameterDirection.Output };
                comando.Parameters.Add(pCodigo);
                comando.Parameters.Add(pMensaje);

                comando.ExecuteNonQuery();

                string cod = pCodigo.Value?.ToString() ?? "96";
                mensajeError = pMensaje.Value?.ToString() ?? MapearCodigoError(cod);
                return cod == "00";
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
                        "UPDATE Cajero SET Estado = 'Desconectado' WHERE CodigoCajero = @Cod AND Estado IN ('Activo', 'En línea')", conexion))
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
                    using (SqlCommand comando = new SqlCommand(@"
                UPDATE Cajero
                SET UltimoHeartbeat = GETDATE(),
                    Estado = CASE WHEN Estado = 'Desconectado' AND Inicializado = 1
                                  THEN 'Activo' ELSE Estado END
                WHERE CodigoCajero = @Codigo", conexion))
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
                                    { "tipo",          reader.GetString(1) },
                                    { "monto",         reader.GetDecimal(2) },
                                    { "fecha",         reader.GetDateTime(3).ToString("dd/MM/yyyy") },
                                    { "hora",          reader.GetDateTime(3).ToString("HH:mm:ss") },
                                    { "cajero",        reader.GetString(4) },
                                    { "codigo",        reader.GetString(5) }
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

        public decimal ObtenerParametroDecimal(string nombre, decimal porDefecto = 0m)
        {
            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                using var comando = new SqlCommand("SELECT Valor FROM Parametro WHERE Nombre = @Nombre", conexion);
                comando.Parameters.AddWithValue("@Nombre", nombre);
                var resultado = comando.ExecuteScalar()?.ToString();
                return decimal.TryParse(resultado, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal v) ? v : porDefecto;
            }
            catch
            {
                return porDefecto;
            }
        }

        public bool ExisteTarjeta(string numeroTarjeta)
        {
            try
            {
                using (var conexion = new Microsoft.Data.SqlClient.SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    string query = @"
                SELECT COUNT(1) 
                FROM Tarjeta t
                INNER JOIN Usuario u ON t.IdUsuario = u.IdUsuario
                WHERE t.NumeroTarjeta = @Num AND t.Activa = 1 AND u.Estado = 'Activo'";

                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@Num", numeroTarjeta);
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch { return false; }
        }

        public string ObtenerEstadoCajero(string codigoCajero)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    using (SqlCommand comando = new SqlCommand("SELECT Estado FROM Cajero WHERE CodigoCajero = @Cod", conexion))
                    {
                        comando.Parameters.AddWithValue("@Cod", codigoCajero);
                        var resultado = comando.ExecuteScalar();
                        return resultado != null ? resultado.ToString() : "Desconectado";
                    }
                }
            }
            catch { return "Desconectado"; }
        }

        // =========================================================================
        // MÉTODOS INTERBANCARIOS (PROTOCOLO NETGEAR 2.2: UDP 5005 + TCP 5000)
        // =========================================================================

        public string ObtenerParametroTexto(string nombre, string porDefecto)
        {
            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                using var cmd = new SqlCommand("SELECT Valor FROM Parametro WHERE Nombre = @Nom", conexion);
                cmd.Parameters.AddWithValue("@Nom", nombre);
                var val = cmd.ExecuteScalar()?.ToString();
                return string.IsNullOrWhiteSpace(val) ? porDefecto : val.Trim();
            }
            catch
            {
                return porDefecto;
            }
        }

        public (string codigoBanco, string nombreBanco, decimal limiteDiario, decimal usadoHoy, decimal disponibleHoy) ObtenerEstadoInterbancarioPropio()
        {
            string codigo = ObtenerParametroTexto("MI_CODIGO_BANCO", "BANC03");
            string nombre = ObtenerParametroTexto("MI_NOMBRE_BANCO", "CAJERORED").Replace("|", "").Replace("\r", "").Replace("\n", "");
            decimal limite = ObtenerParametroDecimal("CUPO_DIARIO_IB", 25000.00m);
            decimal usado = 0m;

            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                const string sql = @"
                    SELECT ISNULL(SUM(Monto), 0)
                    FROM dbo.TransferenciasInterbancarias
                    WHERE CodigoRespuesta = '00'
                      AND CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE);";
                using var cmd = new SqlCommand(sql, conexion);
                usado = Convert.ToDecimal(cmd.ExecuteScalar());
            }
            catch { }

            decimal disponible = Math.Max(0m, limite - usado);
            return (codigo, nombre, limite, usado, disponible);
        }

        public bool VerificarCuentaInterbancaria(string numeroCuenta, out string nombreTitular)
        {
            nombreTitular = string.Empty;
            if (string.IsNullOrWhiteSpace(numeroCuenta)) return false;

            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                const string sql = @"
                    SELECT TOP 1 u.Nombre
                    FROM dbo.Usuario u
                    LEFT JOIN dbo.Tarjeta t ON t.IdUsuario = u.IdUsuario AND t.Activa = 1
                    WHERE u.Estado = 'Activo'
                      AND (t.NumeroTarjeta = @Cuenta OR CAST(u.IdUsuario AS NVARCHAR(30)) = @Cuenta);";

                using var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@Cuenta", numeroCuenta.Trim());
                var res = cmd.ExecuteScalar()?.ToString();
                if (!string.IsNullOrEmpty(res))
                {
                    nombreTitular = res;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public string ProcesarTransferenciaEntranteIB(
            string idMensaje,
            string tokenTransferencia,
            string bancoOrigen,
            string cuentaOrigen,
            string nombreOrigen,
            string bancoDestino,
            string cuentaDestino,
            decimal monto,
            string concepto,
            string ipRemota,
            out string mensajeRespuesta,
            out string nombreDestino,
            out decimal cupoRestante)
        {
            mensajeRespuesta = "No confirmado";
            nombreDestino = string.Empty;
            cupoRestante = 0m;

            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                using var cmd = new SqlCommand("dbo.sp_RecibirTransferenciaIB", conexion)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@IdMensaje", idMensaje ?? "");
                cmd.Parameters.AddWithValue("@TokenTransferencia", tokenTransferencia);
                cmd.Parameters.AddWithValue("@BancoOrigen", bancoOrigen);
                cmd.Parameters.AddWithValue("@CuentaOrigen", cuentaOrigen);
                cmd.Parameters.AddWithValue("@NombreOrigen", nombreOrigen ?? "");
                cmd.Parameters.AddWithValue("@BancoDestino", bancoDestino);
                cmd.Parameters.AddWithValue("@CuentaDestino", cuentaDestino);
                cmd.Parameters.AddWithValue("@Monto", Math.Round(monto, 2));
                cmd.Parameters.AddWithValue("@Concepto", (object)concepto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IpRemota", ipRemota ?? "");

                var pCod = new SqlParameter("@CodigoOut", SqlDbType.NVarChar, 5) { Direction = ParameterDirection.Output };
                var pMsg = new SqlParameter("@MensajeOut", SqlDbType.NVarChar, 200) { Direction = ParameterDirection.Output };
                var pNom = new SqlParameter("@NombreDestinoOut", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                var pCupo = new SqlParameter("@CupoRestanteOut", SqlDbType.Decimal) { Precision = 12, Scale = 2, Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pCod);
                cmd.Parameters.Add(pMsg);
                cmd.Parameters.Add(pNom);
                cmd.Parameters.Add(pCupo);

                cmd.ExecuteNonQuery();

                string codigo = pCod.Value?.ToString() ?? "96";
                mensajeRespuesta = pMsg.Value?.ToString() ?? "No confirmado";
                nombreDestino = pNom.Value?.ToString() ?? "";
                cupoRestante = pCupo.Value != DBNull.Value ? Convert.ToDecimal(pCupo.Value) : 0m;
                return codigo;
            }
            catch (Exception ex)
            {
                mensajeRespuesta = $"Error BD: {ex.Message}";
                return "96";
            }
        }

        public bool ConsultarEstadoTransferenciaIB(
            string bancoOrigen,
            string tokenTransferencia,
            out string bancoDestino,
            out string cuentaDestino,
            out string nombreDestino,
            out decimal monto,
            out decimal cupoRestante)
        {
            bancoDestino = ""; cuentaDestino = ""; nombreDestino = ""; monto = 0m; cupoRestante = 0m;
            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                const string sql = @"
                    SELECT BancoDestino, CuentaDestino, NombreDestino, Monto, ISNULL(CupoRestante, 0)
                    FROM dbo.TransferenciasInterbancarias
                    WHERE UPPER(BancoOrigen) = UPPER(@BancoOrigen)
                      AND TokenTransferencia = @Token
                      AND CodigoRespuesta = '00';";

                using var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@BancoOrigen", bancoOrigen.Trim());
                cmd.Parameters.AddWithValue("@Token", tokenTransferencia.Trim());

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    bancoDestino = reader.GetString(0);
                    cuentaDestino = reader.GetString(1);
                    nombreDestino = reader.GetString(2);
                    monto = reader.GetDecimal(3);
                    cupoRestante = reader.GetDecimal(4);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public void RegistrarBancoDescubierto(string codigo, string nombre, string ip, int puertoTcp, decimal limite, decimal usado)
        {
            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                const string sql = @"
                    IF EXISTS (SELECT 1 FROM dbo.BancosRed WHERE UPPER(CodigoBanco) = UPPER(@Cod))
                        UPDATE dbo.BancosRed
                        SET NombreBanco = @Nom, IP = @Ip, PuertoTcp = @Puerto,
                            LimiteDiario = @Lim, TransferidoHoy = @Usado, UltimoVisto = GETDATE()
                        WHERE UPPER(CodigoBanco) = UPPER(@Cod);
                    ELSE
                        INSERT INTO dbo.BancosRed (CodigoBanco, NombreBanco, IP, PuertoTcp, LimiteDiario, TransferidoHoy, Activo, Autorizado)
                        VALUES (@Cod, @Nom, @Ip, @Puerto, @Lim, @Usado, 1, 0);";

                using var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@Cod", codigo.Trim());
                cmd.Parameters.AddWithValue("@Nom", nombre.Trim());
                cmd.Parameters.AddWithValue("@Ip", ip.Trim());
                cmd.Parameters.AddWithValue("@Puerto", puertoTcp);
                cmd.Parameters.AddWithValue("@Lim", limite);
                cmd.Parameters.AddWithValue("@Usado", usado);
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        public void GuardarOAutorizarBancoRed(string codigo, string nombre, string ip, int puertoTcp, decimal limite, bool autorizado, bool activo = true)
        {
            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                const string sql = @"
                    IF EXISTS (SELECT 1 FROM dbo.BancosRed WHERE UPPER(CodigoBanco) = UPPER(@Cod))
                        UPDATE dbo.BancosRed
                        SET NombreBanco = @Nom, IP = @Ip, PuertoTcp = @Puerto,
                            LimiteDiario = @Lim, Autorizado = @Aut, Activo = @Act, UltimoVisto = GETDATE()
                        WHERE UPPER(CodigoBanco) = UPPER(@Cod);
                    ELSE
                        INSERT INTO dbo.BancosRed (CodigoBanco, NombreBanco, IP, PuertoTcp, LimiteDiario, TransferidoHoy, Activo, Autorizado)
                        VALUES (@Cod, @Nom, @Ip, @Puerto, @Lim, 0, @Act, @Aut);";

                using var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@Cod", codigo.Trim());
                cmd.Parameters.AddWithValue("@Nom", nombre.Replace("|", "").Trim());
                cmd.Parameters.AddWithValue("@Ip", ip.Trim());
                cmd.Parameters.AddWithValue("@Puerto", puertoTcp);
                cmd.Parameters.AddWithValue("@Lim", limite);
                cmd.Parameters.AddWithValue("@Aut", autorizado);
                cmd.Parameters.AddWithValue("@Act", activo);
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        public List<ProyectoServidor.Comunicaciones.BancoRedInfo> ObtenerBancosRed(bool soloAutorizados = false)
        {
            var lista = new List<ProyectoServidor.Comunicaciones.BancoRedInfo>();
            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                string sql = @"
                    SELECT CodigoBanco, NombreBanco, IP, PuertoTcp, PuertoUdp, LimiteDiario, TransferidoHoy, Activo, Autorizado
                    FROM dbo.BancosRed
                    WHERE Activo = 1";
                if (soloAutorizados)
                    sql += " AND Autorizado = 1";
                sql += " ORDER BY CodigoBanco;";

                using var cmd = new SqlCommand(sql, conexion);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new ProyectoServidor.Comunicaciones.BancoRedInfo
                    {
                        CodigoBanco = reader.GetString(0),
                        NombreBanco = reader.GetString(1),
                        IP = reader.GetString(2),
                        PuertoTcp = reader.GetInt32(3),
                        PuertoUdp = reader.GetInt32(4),
                        LimiteDiario = reader.GetDecimal(5),
                        TransferidoHoy = reader.GetDecimal(6),
                        Activo = reader.GetBoolean(7),
                        Autorizado = reader.GetBoolean(8)
                    });
                }
            }
            catch { }
            return lista;
        }

        public ProyectoServidor.Comunicaciones.BancoRedInfo ObtenerBancoRedPorCodigo(string codigoBanco)
        {
            if (string.IsNullOrWhiteSpace(codigoBanco)) return null;
            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                const string sql = @"
                    SELECT TOP 1 CodigoBanco, NombreBanco, IP, PuertoTcp, PuertoUdp, LimiteDiario, TransferidoHoy, Activo, Autorizado
                    FROM dbo.BancosRed
                    WHERE UPPER(CodigoBanco) = UPPER(@Cod);";

                using var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@Cod", codigoBanco.Trim());
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new ProyectoServidor.Comunicaciones.BancoRedInfo
                    {
                        CodigoBanco = reader.GetString(0),
                        NombreBanco = reader.GetString(1),
                        IP = reader.GetString(2),
                        PuertoTcp = reader.GetInt32(3),
                        PuertoUdp = reader.GetInt32(4),
                        LimiteDiario = reader.GetDecimal(5),
                        TransferidoHoy = reader.GetDecimal(6),
                        Activo = reader.GetBoolean(7),
                        Autorizado = reader.GetBoolean(8)
                    };
                }
            }
            catch { }
            return null;
        }

        public void GuardarIdentidadBancoPropio(string codigoBanco, string nombreBanco, decimal cupoDiario)
        {
            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                const string sql = @"
                    IF EXISTS (SELECT 1 FROM dbo.Parametro WHERE Nombre = 'MI_CODIGO_BANCO')
                        UPDATE dbo.Parametro SET Valor = @Cod WHERE Nombre = 'MI_CODIGO_BANCO';
                    ELSE
                        INSERT INTO dbo.Parametro (Nombre, Valor, Descripcion) VALUES ('MI_CODIGO_BANCO', @Cod, 'Código único de nuestro banco');

                    IF EXISTS (SELECT 1 FROM dbo.Parametro WHERE Nombre = 'MI_NOMBRE_BANCO')
                        UPDATE dbo.Parametro SET Valor = @Nom WHERE Nombre = 'MI_NOMBRE_BANCO';
                    ELSE
                        INSERT INTO dbo.Parametro (Nombre, Valor, Descripcion) VALUES ('MI_NOMBRE_BANCO', @Nom, 'Nombre de nuestro banco');

                    IF EXISTS (SELECT 1 FROM dbo.Parametro WHERE Nombre = 'CUPO_DIARIO_IB')
                        UPDATE dbo.Parametro SET Valor = @Cupo WHERE Nombre = 'CUPO_DIARIO_IB';
                    ELSE
                        INSERT INTO dbo.Parametro (Nombre, Valor, Descripcion) VALUES ('CUPO_DIARIO_IB', @Cupo, 'Cupo diario interbancario');";

                using var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@Cod", codigoBanco.Trim());
                cmd.Parameters.AddWithValue("@Nom", nombreBanco.Replace("|", "").Trim());
                cmd.Parameters.AddWithValue("@Cupo", cupoDiario.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        public bool ObtenerDatosOrigenUsuario(int idUsuario, out string cuentaOrigen, out string nombreOrigen, out decimal saldoActual)
        {
            cuentaOrigen = "";
            nombreOrigen = "";
            saldoActual = 0m;
            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                const string sql = @"
                    SELECT TOP 1 ISNULL(t.NumeroTarjeta, CAST(u.IdUsuario AS NVARCHAR(30))), u.Nombre, u.Saldo
                    FROM dbo.Usuario u
                    LEFT JOIN dbo.Tarjeta t ON t.IdUsuario = u.IdUsuario AND t.Activa = 1
                    WHERE u.IdUsuario = @Id AND u.Estado = 'Activo';";

                using var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@Id", idUsuario);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    cuentaOrigen = reader.GetString(0);
                    nombreOrigen = reader.GetString(1);
                    saldoActual = reader.GetDecimal(2);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool ReservarTransferenciaSalienteIB(
            int idUsuarioOrigen,
            string tokenTransferencia,
            string bancoOrigen,
            string cuentaOrigen,
            string nombreOrigen,
            string bancoDestino,
            string cuentaDestino,
            string nombreDestino,
            decimal monto,
            string concepto,
            out string codigoError,
            out string mensajeError)
        {
            codigoError = "96";
            mensajeError = "Error al reservar transferencia";

            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                using var tx = conexion.BeginTransaction(IsolationLevel.Serializable);

                try
                {
                    decimal saldoActual = -1m;
                    using (var cmdSaldo = new SqlCommand(
                        "SELECT Saldo FROM dbo.Usuario WITH (UPDLOCK, HOLDLOCK) WHERE IdUsuario = @Id AND Estado = 'Activo';",
                        conexion, tx))
                    {
                        cmdSaldo.Parameters.AddWithValue("@Id", idUsuarioOrigen);
                        var val = cmdSaldo.ExecuteScalar();
                        if (val != null && val != DBNull.Value)
                            saldoActual = Convert.ToDecimal(val);
                    }

                    if (saldoActual < 0m)
                    {
                        codigoError = "14";
                        mensajeError = "Cuenta origen inactiva o no encontrada";
                        tx.Rollback();
                        return false;
                    }

                    if (saldoActual < monto)
                    {
                        codigoError = "51";
                        mensajeError = "Saldo insuficiente para realizar la transferencia";
                        tx.Rollback();
                        return false;
                    }

                    decimal cupoDiario = 25000.00m;
                    using (var cmdCupo = new SqlCommand("SELECT Valor FROM dbo.Parametro WHERE Nombre = 'CUPO_DIARIO_IB';", conexion, tx))
                    {
                        var valCupo = cmdCupo.ExecuteScalar()?.ToString();
                        if (decimal.TryParse(valCupo, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal c))
                            cupoDiario = c;
                    }

                    decimal usadoHoy = 0m;
                    using (var cmdUsado = new SqlCommand(@"
                        SELECT ISNULL(SUM(Monto), 0)
                        FROM dbo.TransferenciasInterbancarias WITH (UPDLOCK, HOLDLOCK)
                        WHERE CodigoRespuesta = '00' AND CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE);", conexion, tx))
                    {
                        usadoHoy = Convert.ToDecimal(cmdUsado.ExecuteScalar());
                    }

                    if (usadoHoy + monto > cupoDiario)
                    {
                        codigoError = "65";
                        mensajeError = "Excede el cupo diario interbancario disponible";
                        tx.Rollback();
                        return false;
                    }

                    using (var cmdDesc = new SqlCommand("UPDATE dbo.Usuario SET Saldo = Saldo - @Monto WHERE IdUsuario = @Id;", conexion, tx))
                    {
                        cmdDesc.Parameters.AddWithValue("@Monto", Math.Round(monto, 2));
                        cmdDesc.Parameters.AddWithValue("@Id", idUsuarioOrigen);
                        cmdDesc.ExecuteNonQuery();
                    }

                    string huella = $"{bancoOrigen.Trim().ToUpper()}|{cuentaOrigen.Trim()}|{bancoDestino.Trim().ToUpper()}|{cuentaDestino.Trim()}|{monto.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}";

                    using (var cmdIns = new SqlCommand(@"
                        INSERT INTO dbo.TransferenciasInterbancarias (
                            Direccion, TokenTransferencia, BancoOrigen, CuentaOrigen, NombreOrigen,
                            BancoDestino, CuentaDestino, NombreDestino, IdUsuarioLocal, Monto, Concepto,
                            HuellaContenido, CodigoRespuesta, Estado, CupoRestante
                        )
                        VALUES (
                            'SALIENTE', @Token, @BOrig, @COrig, @NOrig,
                            @BDest, @CDest, @NDest, @IdUsr, @Monto, @Concepto,
                            @Huella, '96', 'RESERVADA', @CupoRest
                        );", conexion, tx))
                    {
                        cmdIns.Parameters.AddWithValue("@Token", tokenTransferencia);
                        cmdIns.Parameters.AddWithValue("@BOrig", bancoOrigen.Trim());
                        cmdIns.Parameters.AddWithValue("@COrig", cuentaOrigen.Trim());
                        cmdIns.Parameters.AddWithValue("@NOrig", nombreOrigen ?? "");
                        cmdIns.Parameters.AddWithValue("@BDest", bancoDestino.Trim());
                        cmdIns.Parameters.AddWithValue("@CDest", cuentaDestino.Trim());
                        cmdIns.Parameters.AddWithValue("@NDest", nombreDestino ?? "");
                        cmdIns.Parameters.AddWithValue("@IdUsr", idUsuarioOrigen);
                        cmdIns.Parameters.AddWithValue("@Monto", Math.Round(monto, 2));
                        cmdIns.Parameters.AddWithValue("@Concepto", (object)concepto ?? DBNull.Value);
                        cmdIns.Parameters.AddWithValue("@Huella", huella);
                        cmdIns.Parameters.AddWithValue("@CupoRest", cupoDiario - (usadoHoy + monto));
                        cmdIns.ExecuteNonQuery();
                    }

                    tx.Commit();
                    codigoError = "00";
                    mensajeError = "Reserva aplicada";
                    return true;
                }
                catch (Exception exTx)
                {
                    try { tx.Rollback(); } catch { }
                    mensajeError = exTx.Message;
                    return false;
                }
            }
            catch (Exception ex)
            {
                mensajeError = ex.Message;
                return false;
            }
        }

        public void ConfirmarTransferenciaSalienteIB(string bancoOrigen, string tokenTransferencia, string bancoDestino, decimal monto)
        {
            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                const string sql = @"
                    UPDATE dbo.TransferenciasInterbancarias
                    SET CodigoRespuesta = '00', Estado = 'CONFIRMADA'
                    WHERE UPPER(BancoOrigen) = UPPER(@BOrig) AND TokenTransferencia = @Token;

                    UPDATE dbo.BancosRed
                    SET TransferidoHoy = TransferidoHoy + @Monto, UltimoVisto = GETDATE()
                    WHERE UPPER(CodigoBanco) = UPPER(@BDest);";

                using var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@BOrig", bancoOrigen.Trim());
                cmd.Parameters.AddWithValue("@Token", tokenTransferencia.Trim());
                cmd.Parameters.AddWithValue("@BDest", bancoDestino.Trim());
                cmd.Parameters.AddWithValue("@Monto", Math.Round(monto, 2));
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        public void RevertirTransferenciaSalienteRechazadaIB(string bancoOrigen, string tokenTransferencia, string codigoRespuesta)
        {
            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                using var tx = conexion.BeginTransaction(IsolationLevel.Serializable);

                const string sql = @"
                    DECLARE @IdUsuario INT, @Monto DECIMAL(12,2), @EstadoActual NVARCHAR(20);
                    SELECT @IdUsuario = IdUsuarioLocal, @Monto = Monto, @EstadoActual = Estado
                    FROM dbo.TransferenciasInterbancarias WITH (UPDLOCK, HOLDLOCK)
                    WHERE UPPER(BancoOrigen) = UPPER(@BOrig) AND TokenTransferencia = @Token;

                    IF @EstadoActual IN ('RESERVADA', 'INCIERTA') AND @IdUsuario IS NOT NULL
                    BEGIN
                        UPDATE dbo.Usuario SET Saldo = Saldo + @Monto WHERE IdUsuario = @IdUsuario;
                        UPDATE dbo.TransferenciasInterbancarias
                        SET CodigoRespuesta = @Cod, Estado = 'RECHAZADA'
                        WHERE UPPER(BancoOrigen) = UPPER(@BOrig) AND TokenTransferencia = @Token;
                    END";

                using var cmd = new SqlCommand(sql, conexion, tx);
                cmd.Parameters.AddWithValue("@BOrig", bancoOrigen.Trim());
                cmd.Parameters.AddWithValue("@Token", tokenTransferencia.Trim());
                cmd.Parameters.AddWithValue("@Cod", string.IsNullOrWhiteSpace(codigoRespuesta) ? "96" : codigoRespuesta.Trim());
                cmd.ExecuteNonQuery();
                tx.Commit();
            }
            catch { }
        }

        public void MarcarTransferenciaSalienteInciertaIB(string bancoOrigen, string tokenTransferencia)
        {
            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                const string sql = @"
                    UPDATE dbo.TransferenciasInterbancarias
                    SET Estado = 'INCIERTA', CodigoRespuesta = '96'
                    WHERE UPPER(BancoOrigen) = UPPER(@BOrig) AND TokenTransferencia = @Token AND Estado = 'RESERVADA';";
                using var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@BOrig", bancoOrigen.Trim());
                cmd.Parameters.AddWithValue("@Token", tokenTransferencia.Trim());
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        public void AsegurarEsquemaInterbancario()
        {
            try
            {
                using var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();

                const string sqlTablas = @"
                    IF NOT EXISTS (SELECT 1 FROM dbo.Parametro WHERE Nombre = 'MI_CODIGO_BANCO')
                        INSERT INTO dbo.Parametro (Nombre, Valor, Descripcion) VALUES ('MI_CODIGO_BANCO', 'BANC03', 'Código único de nuestro banco');

                    IF NOT EXISTS (SELECT 1 FROM dbo.Parametro WHERE Nombre = 'MI_NOMBRE_BANCO')
                        INSERT INTO dbo.Parametro (Nombre, Valor, Descripcion) VALUES ('MI_NOMBRE_BANCO', 'CAJERORED', 'Nombre de nuestro banco');

                    IF NOT EXISTS (SELECT 1 FROM dbo.Parametro WHERE Nombre = 'CUPO_DIARIO_IB')
                        INSERT INTO dbo.Parametro (Nombre, Valor, Descripcion) VALUES ('CUPO_DIARIO_IB', '25000.00', 'Cupo diario interbancario');

                    IF OBJECT_ID('dbo.BancosRed', 'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.BancosRed (
                            CodigoBanco NVARCHAR(20) NOT NULL PRIMARY KEY,
                            NombreBanco NVARCHAR(100) NOT NULL,
                            IP NVARCHAR(45) NOT NULL,
                            PuertoTcp INT NOT NULL DEFAULT 5000,
                            PuertoUdp INT NOT NULL DEFAULT 5005,
                            LimiteDiario DECIMAL(12,2) NOT NULL DEFAULT 25000.00,
                            TransferidoHoy DECIMAL(12,2) NOT NULL DEFAULT 0.00,
                            Activo BIT NOT NULL DEFAULT 1,
                            Autorizado BIT NOT NULL DEFAULT 1,
                            UltimoVisto DATETIME NOT NULL DEFAULT GETDATE()
                        );
                        INSERT INTO dbo.BancosRed (CodigoBanco, NombreBanco, IP, PuertoTcp, PuertoUdp, LimiteDiario, TransferidoHoy, Activo, Autorizado)
                        VALUES ('BANC02', 'NETGEAR', '192.168.0.34', 5000, 5005, 25000.00, 0.00, 1, 1);
                    END

                    IF OBJECT_ID('dbo.TransferenciasInterbancarias', 'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.TransferenciasInterbancarias (
                            IdTransferencia INT IDENTITY(1,1) PRIMARY KEY,
                            Direccion NVARCHAR(10) NOT NULL CHECK (Direccion IN ('ENTRANTE', 'SALIENTE')),
                            TokenTransferencia NVARCHAR(60) NOT NULL,
                            IdMensaje NVARCHAR(60) NULL,
                            BancoOrigen NVARCHAR(20) NOT NULL,
                            CuentaOrigen NVARCHAR(30) NOT NULL,
                            NombreOrigen NVARCHAR(100) NOT NULL,
                            BancoDestino NVARCHAR(20) NOT NULL,
                            CuentaDestino NVARCHAR(30) NOT NULL,
                            NombreDestino NVARCHAR(100) NOT NULL,
                            IdUsuarioLocal INT NULL REFERENCES dbo.Usuario(IdUsuario),
                            Monto DECIMAL(12,2) NOT NULL CHECK (Monto > 0),
                            Concepto NVARCHAR(200) NULL,
                            HuellaContenido NVARCHAR(300) NOT NULL,
                            CodigoRespuesta NVARCHAR(5) NOT NULL,
                            Estado NVARCHAR(20) NOT NULL,
                            CupoRestante DECIMAL(12,2) NULL,
                            IpRemota NVARCHAR(45) NULL,
                            Fecha DATETIME NOT NULL DEFAULT GETDATE(),
                            CONSTRAINT UX_Transferencia_BancoToken UNIQUE (BancoOrigen, TokenTransferencia)
                        );
                    END";

                using (var cmdTablas = new SqlCommand(sqlTablas, conexion))
                {
                    cmdTablas.ExecuteNonQuery();
                }

                const string sqlSp = @"
                    CREATE OR ALTER PROCEDURE dbo.sp_RecibirTransferenciaIB
                        @IdMensaje NVARCHAR(60),
                        @TokenTransferencia NVARCHAR(60),
                        @BancoOrigen NVARCHAR(20),
                        @CuentaOrigen NVARCHAR(30),
                        @NombreOrigen NVARCHAR(100),
                        @BancoDestino NVARCHAR(20),
                        @CuentaDestino NVARCHAR(30),
                        @Monto DECIMAL(12,2),
                        @Concepto NVARCHAR(200),
                        @IpRemota NVARCHAR(45),
                        @CodigoOut NVARCHAR(5) OUTPUT,
                        @MensajeOut NVARCHAR(200) OUTPUT,
                        @NombreDestinoOut NVARCHAR(100) OUTPUT,
                        @CupoRestanteOut DECIMAL(12,2) OUTPUT
                    AS
                    BEGIN
                        SET NOCOUNT ON;
                        SET XACT_ABORT ON;

                        SET @CodigoOut = '96';
                        SET @MensajeOut = 'No confirmado';
                        SET @NombreDestinoOut = '';
                        SET @CupoRestanteOut = 0;

                        DECLARE @MiCodigo NVARCHAR(20) = ISNULL((SELECT Valor FROM dbo.Parametro WHERE Nombre = 'MI_CODIGO_BANCO'), 'BANC03');
                        DECLARE @CupoDiario DECIMAL(12,2) = ISNULL(TRY_CAST((SELECT Valor FROM dbo.Parametro WHERE Nombre = 'CUPO_DIARIO_IB') AS DECIMAL(12,2)), 25000.00);

                        DECLARE @HuellaNueva NVARCHAR(300) = CONCAT(
                            UPPER(LTRIM(RTRIM(@BancoOrigen))), '|', LTRIM(RTRIM(@CuentaOrigen)), '|',
                            UPPER(LTRIM(RTRIM(@BancoDestino))), '|', LTRIM(RTRIM(@CuentaDestino)), '|',
                            CONVERT(VARCHAR(30), CAST(@Monto AS DECIMAL(12,2)))
                        );

                        BEGIN TRY
                            BEGIN TRANSACTION;

                            DECLARE @HuellaExistente NVARCHAR(300), @CodExistente NVARCHAR(5), @NomDestExistente NVARCHAR(100), @CupoExistente DECIMAL(12,2);

                            SELECT 
                                @HuellaExistente = HuellaContenido,
                                @CodExistente = CodigoRespuesta,
                                @NomDestExistente = NombreDestino,
                                @CupoExistente = ISNULL(CupoRestante, 0)
                            FROM dbo.TransferenciasInterbancarias WITH (UPDLOCK, HOLDLOCK)
                            WHERE UPPER(BancoOrigen) = UPPER(LTRIM(RTRIM(@BancoOrigen)))
                              AND TokenTransferencia = LTRIM(RTRIM(@TokenTransferencia));

                            IF @HuellaExistente IS NOT NULL
                            BEGIN
                                IF @HuellaExistente = @HuellaNueva AND @CodExistente = '00'
                                BEGIN
                                    SET @CodigoOut = '00';
                                    SET @MensajeOut = 'Transferencia acreditada';
                                    SET @NombreDestinoOut = @NomDestExistente;
                                    SET @CupoRestanteOut = @CupoExistente;
                                    COMMIT TRANSACTION;
                                    RETURN;
                                END
                                ELSE
                                BEGIN
                                    SET @CodigoOut = '94';
                                    SET @MensajeOut = 'Referencia en conflicto: el mismo token fue enviado con datos distintos';
                                    COMMIT TRANSACTION;
                                    RETURN;
                                END
                            END

                            IF UPPER(LTRIM(RTRIM(@BancoDestino))) <> UPPER(LTRIM(RTRIM(@MiCodigo)))
                            BEGIN
                                SET @CodigoOut = '30';
                                SET @MensajeOut = 'El bancoDestino no corresponde a este banco';
                                COMMIT TRANSACTION;
                                RETURN;
                            END

                            IF NOT EXISTS (
                                SELECT 1 FROM dbo.BancosRed
                                WHERE UPPER(CodigoBanco) = UPPER(LTRIM(RTRIM(@BancoOrigen)))
                                  AND Activo = 1
                                  AND Autorizado = 1
                                  AND (IP = @IpRemota OR @IpRemota = '127.0.0.1' OR IP = '0.0.0.0')
                            )
                            BEGIN
                                SET @CodigoOut = '57';
                                SET @MensajeOut = 'Banco origen o dirección IP no autorizados';
                                COMMIT TRANSACTION;
                                RETURN;
                            END

                            IF @Monto IS NULL OR @Monto <= 0
                            BEGIN
                                SET @CodigoOut = '30';
                                SET @MensajeOut = 'Monto inválido';
                                COMMIT TRANSACTION;
                                RETURN;
                            END

                            DECLARE @IdUsuarioDestino INT = NULL;
                            SELECT TOP 1
                                @IdUsuarioDestino = u.IdUsuario,
                                @NombreDestinoOut = u.Nombre
                            FROM dbo.Usuario u WITH (UPDLOCK, HOLDLOCK)
                            LEFT JOIN dbo.Tarjeta t ON t.IdUsuario = u.IdUsuario AND t.Activa = 1
                            WHERE u.Estado = 'Activo'
                              AND (t.NumeroTarjeta = LTRIM(RTRIM(@CuentaDestino)) OR CAST(u.IdUsuario AS NVARCHAR(30)) = LTRIM(RTRIM(@CuentaDestino)));

                            IF @IdUsuarioDestino IS NULL
                            BEGIN
                                SET @CodigoOut = '14';
                                SET @MensajeOut = 'Cuenta inexistente o inactiva';
                                COMMIT TRANSACTION;
                                RETURN;
                            END

                            DECLARE @UsadoHoy DECIMAL(12,2) = ISNULL((
                                SELECT SUM(Monto)
                                FROM dbo.TransferenciasInterbancarias WITH (UPDLOCK, HOLDLOCK)
                                WHERE CodigoRespuesta = '00'
                                  AND CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE)
                            ), 0.00);

                            IF (@UsadoHoy + @Monto) > @CupoDiario
                            BEGIN
                                SET @CodigoOut = '65';
                                SET @MensajeOut = 'Cupo diario insuficiente';
                                SET @CupoRestanteOut = CASE WHEN @CupoDiario > @UsadoHoy THEN @CupoDiario - @UsadoHoy ELSE 0 END;
                                COMMIT TRANSACTION;
                                RETURN;
                            END

                            UPDATE dbo.Usuario
                            SET Saldo = Saldo + @Monto
                            WHERE IdUsuario = @IdUsuarioDestino;

                            SET @CupoRestanteOut = @CupoDiario - (@UsadoHoy + @Monto);

                            INSERT INTO dbo.TransferenciasInterbancarias (
                                Direccion, TokenTransferencia, IdMensaje, BancoOrigen, CuentaOrigen, NombreOrigen,
                                BancoDestino, CuentaDestino, NombreDestino, IdUsuarioLocal, Monto, Concepto,
                                HuellaContenido, CodigoRespuesta, Estado, CupoRestante, IpRemota
                            )
                            VALUES (
                                'ENTRANTE', LTRIM(RTRIM(@TokenTransferencia)), @IdMensaje, LTRIM(RTRIM(@BancoOrigen)), LTRIM(RTRIM(@CuentaOrigen)), ISNULL(@NombreOrigen, ''),
                                LTRIM(RTRIM(@BancoDestino)), LTRIM(RTRIM(@CuentaDestino)), @NombreDestinoOut, @IdUsuarioDestino, @Monto, @Concepto,
                                @HuellaNueva, '00', 'CONFIRMADA', @CupoRestanteOut, @IpRemota
                            );

                            UPDATE dbo.BancosRed
                            SET TransferidoHoy = TransferidoHoy + @Monto,
                                UltimoVisto = GETDATE()
                            WHERE UPPER(CodigoBanco) = UPPER(LTRIM(RTRIM(@BancoOrigen)));

                            INSERT INTO dbo.Bitacora (Origen, Accion, Detalle)
                            VALUES ('Servidor', 'IB_TRANSFERIR_RECIBIDO',
                                    CONCAT('Token: ', @TokenTransferencia, ' | Origen: ', @BancoOrigen, ' (', @CuentaOrigen, ') | Destino: ', @CuentaDestino, ' | Monto: Q.', @Monto));

                            SET @CodigoOut = '00';
                            SET @MensajeOut = 'Transferencia acreditada';

                            COMMIT TRANSACTION;
                        END TRY
                        BEGIN CATCH
                            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                            SET @CodigoOut = '96';
                            SET @MensajeOut = ERROR_MESSAGE();
                        END CATCH
                    END";

                using (var cmdSp = new SqlCommand(sqlSp, conexion))
                {
                    cmdSp.ExecuteNonQuery();
                }
            }
            catch { }
        }
    }
}
