using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CajeroAutomatico.Entidades;

namespace CajeroAutomatico.Datos
{
    // Capa de Datos: toda la lectura/escritura a archivos planos vive aquí.
    // Los datos se almacenan cifrados mediante AES-256 en disco.
    public static class RepositorioArchivos
    {
        private static readonly string CarpetaDatos = "Datos";
        private static readonly string ArchivoUsuarios = Path.Combine(CarpetaDatos, "usuarios.csv");
        private static readonly string ArchivoTransacciones = Path.Combine(CarpetaDatos, "transacciones.csv");
        private static readonly string ArchivoConfig = Path.Combine(CarpetaDatos, "config.csv");
        private const char Sep = ';';

        // Llave de 32 bytes (256 bits) y Vector de Inicialización (IV) de 16 bytes (128 bits)
        private static readonly byte[] ClaveAes = Encoding.UTF8.GetBytes("C@j3r0_Aut0m@t1c0_S3cur3_K3y_32B"); // 32 caracteres
        private static readonly byte[] IvAes = Encoding.UTF8.GetBytes("Iv_Cajero_16Byte");                 // 16 caracteres

        public static void AsegurarArchivos()
        {
            Directory.CreateDirectory(CarpetaDatos);

            if (!File.Exists(ArchivoUsuarios))
                EscribirTextoEncriptado(ArchivoUsuarios, "");

            if (!File.Exists(ArchivoTransacciones))
                EscribirTextoEncriptado(ArchivoTransacciones, "");

            if (!File.Exists(ArchivoConfig))
                EscribirTextoEncriptado(ArchivoConfig, "");
        }

        public static bool VerificarConexion()
        {
            try
            {
                AsegurarArchivos();
                return File.Exists(ArchivoUsuarios) && File.Exists(ArchivoTransacciones) && File.Exists(ArchivoConfig);
            }
            catch
            {
                return false;
            }
        }

         //==========================================
         //MÉTODOS DE CRIPTOGRAFÍA(AES-256)
         //==========================================


        private static void EscribirTextoEncriptado(string rutaArchivo, string contenidoPlano)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = ClaveAes;
                aes.IV = IvAes;

                using (FileStream fs = new FileStream(rutaArchivo, FileMode.Create, FileAccess.Write))
                using (CryptoStream cs = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs, Encoding.UTF8))
                {
                    sw.Write(contenidoPlano);
                }
            }
        }

        private static string LeerTextoDesencriptado(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo)) return string.Empty;

            var info = new FileInfo(rutaArchivo);
            if (info.Length == 0) return string.Empty;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = ClaveAes;
                    aes.IV = IvAes;

                    using (FileStream fs = new FileStream(rutaArchivo, FileMode.Open, FileAccess.Read))
                    using (CryptoStream cs = new CryptoStream(fs, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (StreamReader sr = new StreamReader(cs, Encoding.UTF8))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (CryptographicException)
            {
                // Si el archivo existía previamente en texto plano (sin encriptar),
                // lo lee en plano y lo sobrescribe encriptado automáticamente.
                string contenidoPlano = File.ReadAllText(rutaArchivo, Encoding.UTF8);
                EscribirTextoEncriptado(rutaArchivo, contenidoPlano);
                return contenidoPlano;
            }
        }

        private static string[] LeerLineasDesencriptadas(string rutaArchivo)
        {
            string texto = LeerTextoDesencriptado(rutaArchivo);
            if (string.IsNullOrWhiteSpace(texto)) return Array.Empty<string>();

            return texto.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        // ==========================================
        // USUARIOS
        // ==========================================

        public static List<Usuario> CargarUsuarios()

        {

            var lista = new List<Usuario>();
            var lineas = LeerLineasDesencriptadas(ArchivoUsuarios);

            foreach (var linea in lineas)
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;
                var p = linea.Split(Sep);

                lista.Add(new Usuario

                {

                    NumeroTarjeta = p[0],

                    Nombre = p[1],

                    Pin = p[2],

                    Saldo = decimal.Parse(p[3], CultureInfo.InvariantCulture),

                    LimiteDiario = decimal.Parse(p[4], CultureInfo.InvariantCulture),

                    RetiradoHoy = decimal.Parse(p[5], CultureInfo.InvariantCulture),

                    FechaRetiroHoy = DateTime.Parse(p[6], CultureInfo.InvariantCulture),

                    UltimoAcceso = DateTime.Parse(p[7], CultureInfo.InvariantCulture),

                    IntentosFallidos = int.Parse(p[8]),

                    Bloqueado = bool.Parse(p[9]),

                    Tipo = (TipoUsuario)Enum.Parse(typeof(TipoUsuario), p[10]),

                    HistorialTarjetas = p[11].Length > 0 ? p[11].Split(',').ToList() : new List<string>(),

                    CambioPin = bool.Parse(p[12]),



                    // AGREGADO: Leemos la huella digital de la posición 13 (columna 14)

                    HuellaDigital = p.Length > 13 ? p[13] : "",

                    UID = p.Length > 14 ? p[14] : "",

                    HuellaSlot = p.Length > 15 ? int.Parse(p[15]) : -1

                });

            }

            return lista;

        }

        public static void GuardarUsuarios(List<Usuario> usuarios)
        {
            var lineas = usuarios.Select(u => string.Join(Sep.ToString(), new[]
            {
                u.NumeroTarjeta,
                u.Nombre,
                u.Pin,
                u.Saldo.ToString(CultureInfo.InvariantCulture),
                u.LimiteDiario.ToString(CultureInfo.InvariantCulture),
                u.RetiradoHoy.ToString(CultureInfo.InvariantCulture),
                u.FechaRetiroHoy.ToString("o", CultureInfo.InvariantCulture),
                u.UltimoAcceso.ToString("o", CultureInfo.InvariantCulture),
                u.IntentosFallidos.ToString(),
                u.Bloqueado.ToString(),
                u.Tipo.ToString(),
                string.Join(",", u.HistorialTarjetas),
                u.CambioPin.ToString(),
                u.HuellaDigital ?? "",
                u.UID ?? "",
                u.HuellaSlot.ToString()
            }));

            string contenido = string.Join(Environment.NewLine, lineas);
            EscribirTextoEncriptado(ArchivoUsuarios, contenido);
        }

        // ==========================================
        // TRANSACCIONES
        // ==========================================

        public static List<Transaccion> CargarTransacciones()
        {
            var lista = new List<Transaccion>();
            var lineas = LeerLineasDesencriptadas(ArchivoTransacciones);

            foreach (var linea in lineas)
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;
                var p = linea.Split(Sep);
                lista.Add(new Transaccion
                {
                    NumeroTarjeta = p[0],
                    Fecha = DateTime.Parse(p[1], CultureInfo.InvariantCulture),
                    Hora = TimeSpan.Parse(p[2], CultureInfo.InvariantCulture),
                    Tipo = (TipoTransaccion)Enum.Parse(typeof(TipoTransaccion), p[3]),
                    Monto = decimal.Parse(p[4], CultureInfo.InvariantCulture),
                    Desglose = ParsearDesglose(p[5])
                });
            }
            return lista;
        }

        public static void AgregarTransaccion(Transaccion t)
        {
            // Para archivos cifrados se lee el historial completo, se agrega el registro y se re-cifra
            var transacciones = CargarTransacciones();
            transacciones.Add(t);

            var lineas = transacciones.Select(tr => string.Join(Sep.ToString(), new[]
            {
                tr.NumeroTarjeta,
                tr.Fecha.ToString("o", CultureInfo.InvariantCulture),
                tr.Hora.ToString(),
                tr.Tipo.ToString(),
                tr.Monto.ToString(CultureInfo.InvariantCulture),
                SerializarDesglose(tr.Desglose)
            }));

            string contenido = string.Join(Environment.NewLine, lineas);
            EscribirTextoEncriptado(ArchivoTransacciones, contenido);
        }

        private static string SerializarDesglose(Dictionary<int, int> d) =>
            string.Join(",", d.Select(kv => $"{kv.Key}:{kv.Value}"));

        private static Dictionary<int, int> ParsearDesglose(string texto)
        {
            var dic = new Dictionary<int, int>();
            if (string.IsNullOrWhiteSpace(texto)) return dic;
            foreach (var par in texto.Split(','))
            {
                var kv = par.Split(':');
                dic[int.Parse(kv[0])] = int.Parse(kv[1]);
            }
            return dic;
        }

        // ==========================================
        // CONFIGURACIÓN DEL CAJERO
        // ==========================================

        public static CajeroConfig CargarConfig()
        {
            var config = new CajeroConfig();
            var contenido = LeerTextoDesencriptado(ArchivoConfig);
            if (string.IsNullOrWhiteSpace(contenido)) return config;

            var p = contenido.Split(Sep);
            config.Estado = (EstadoCajero)Enum.Parse(typeof(EstadoCajero), p[0]);
            config.Inicializado = bool.Parse(p[1]);
            config.FechaInicializacion = DateTime.Parse(p[2], CultureInfo.InvariantCulture);
            config.Denominaciones = ParsearDesglose(p[3]);
            return config;
        }

        public static void GuardarConfig(CajeroConfig config)
        {
            var linea = string.Join(Sep.ToString(), new[]
            {
                config.Estado.ToString(),
                config.Inicializado.ToString(),
                config.FechaInicializacion.ToString("o", CultureInfo.InvariantCulture),
                SerializarDesglose(config.Denominaciones)
            });

            EscribirTextoEncriptado(ArchivoConfig, linea);
        }

        // ==========================================
        // REPORTES
        // ==========================================

        public static void GenerarReporte(string rutaArchivo, string titulo, List<string> lineas)
        {
            var contenido = new List<string> { titulo, new string('-', titulo.Length), "" };
            contenido.AddRange(lineas);
            File.WriteAllLines(rutaArchivo, contenido);
        }
    }
}