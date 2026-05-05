using AccesoDatos.Abstractions;
using System;
using System.Data;

namespace LogicaNegocios
{
    public class FacturasPendientesManejador
    {
        private readonly IConexionBD _conexion;
        private readonly LogManejador _log;

        public FacturasPendientesManejador(
            IConexionBD conexion,
            LogManejador log
        )
        {
            _conexion = conexion;
            _log = log;
        }
        // ==========================================================
        // MOSTRAR TODOS LOS PENDIENTES (TODOS LOS TIPOS)
        // ==========================================================
        public DataSet Mostrar()
        {
            string sql = @"
                SELECT 
                    NUMEROFACTURA,
                    CLAVEACCESO,
                    RUTAXMLFIRMADO,
                    FECHAREGISTRO,
                    INTENTOS,
                    ESTADO,
                    TIPO
                FROM FACTURAS_PENDIENTES
                ORDER BY FECHAREGISTRO ASC
            ";

            return _conexion.Seleccionar(sql);
        }

        // ==========================================================
        // CONSULTAR POR NUMERO DE DOCUMENTO (CUALQUIER TIPO)
        // ==========================================================
        public DataSet Consultar(string numeroDocumento)
        {
            string sql = @"
                SELECT 
                    NUMEROFACTURA,
                    CLAVEACCESO,
                    RUTAXMLFIRMADO,
                    FECHAREGISTRO,
                    INTENTOS,
                    ESTADO,
                    TIPO
                FROM FACTURAS_PENDIENTES
                WHERE NUMEROFACTURA = '" + numeroDocumento + @"'
            ";

            return _conexion.Seleccionar(sql);
        }


        public DataSet ConsultarPorNumeroYTipo(string numeroDocumento, string tipo)
        {
            if (string.IsNullOrWhiteSpace(numeroDocumento) || string.IsNullOrWhiteSpace(tipo))
                return new DataSet();

            string sql = @"
        SELECT 
            NUMEROFACTURA,
            CLAVEACCESO,
            RUTAXMLFIRMADO,
            FECHAREGISTRO,
            INTENTOS,
            ESTADO,
            TIPO
        FROM FACTURAS_PENDIENTES
        WHERE NUMEROFACTURA = '" + numeroDocumento.Trim() + @"'
          AND TIPO = '" + tipo.Trim().ToUpper() + @"'
    ";

            return _conexion.Seleccionar(sql);
        }

        private string EscaparTextoSql(string valor)
        {
            if (valor == null)
                return "";

            return valor.Replace("'", "''");
        }

        private string ConstruirTextoSql(string valor)
        {
            if (valor == null)
                return "NULL";

            return "'" + EscaparTextoSql(valor) + "'";
        }

        private string ConstruirFechaSqlUniversal(DateTime fecha)
        {
            return "'" + fecha.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + "'";
        }
        // ==========================================================
        // INSERTAR PENDIENTE (GENÉRICO)
        // ==========================================================
        public int Insertar(
        string numeroDocumento,
        string claveAcceso,
        string rutaXmlFirmado,
        DateTime fechaRegistro,
        int intentos,
        string estado,
        string tipo,
        string usuario,
        string ip)
        {
            string sql = @"
        INSERT INTO FACTURAS_PENDIENTES(
            NUMEROFACTURA,
            CLAVEACCESO,
            RUTAXMLFIRMADO,
            FECHAREGISTRO,
            INTENTOS,
            ESTADO,
            TIPO
        ) VALUES (
            " + ConstruirTextoSql(numeroDocumento) + @",
            " + ConstruirTextoSql(claveAcceso) + @",
            " + ConstruirTextoSql(rutaXmlFirmado) + @",
            " + ConstruirFechaSqlUniversal(fechaRegistro) + @",
            " + intentos.ToString() + @",
            " + ConstruirTextoSql(estado) + @",
            " + ConstruirTextoSql(tipo) + @"
        )";

            _log.CrearLog(
                $"Se registró pendiente [{tipo}]: {numeroDocumento}",
                usuario,
                ip,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        private string ConstruirFechaSql(DateTime fecha)
        {
            string proveedor = Environment.GetEnvironmentVariable("DB_PROVIDER")?.Trim().ToUpper();

            if (proveedor == "SQL")
            {
                return "'" + fecha.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            }

            return "#" + fecha.ToString("yyyy/MM/dd HH:mm:ss") + "#";
        }


        // ==========================================================
        // ACTUALIZAR ESTADO
        // ==========================================================
        public int ActualizarEstado(
            string numeroDocumento,
            string estado,
            string tipo,
            string usuario,
            string ip)
        {
            string sql = @"
    UPDATE FACTURAS_PENDIENTES SET
        ESTADO = " + ConstruirTextoSql(estado) + @",
        FECHAREGISTRO = " + ConstruirFechaSqlUniversal(DateTime.Now) + @"
    WHERE NUMEROFACTURA = " + ConstruirTextoSql(numeroDocumento) + @"
      AND TIPO = " + ConstruirTextoSql(tipo);

            _log.CrearLog(
                "ACTUALIZAR ESTADO FACTURAS_PENDIENTES",
                usuario,
                ip,
                $"NUMERO={numeroDocumento} | TIPO={tipo} | ESTADO={estado}"
            );

            return _conexion.Ejecutar(sql);
        }

        // ==========================================================
        // ELIMINAR PENDIENTE (CUANDO YA SE COMPLETÓ TODO)
        // ==========================================================
        public int Eliminar(
        string numeroDocumento,
        string tipo,
        string usuario,
        string ip
    )
        {
            if (string.IsNullOrWhiteSpace(tipo))
                throw new Exception("Tipo de documento vacío.");

            if (string.IsNullOrWhiteSpace(numeroDocumento))
                throw new Exception("Número de documento vacío.");

            string sql = @"
        DELETE FROM FACTURAS_PENDIENTES
        WHERE TIPO = '" + tipo.Replace("'", "''") + @"'
          AND NUMEROFACTURA = '" + numeroDocumento.Replace("'", "''") + @"'
    ";

            return _conexion.Ejecutar(sql);
        }

        // PENDIENTES

        public string ObtenerEstadoPendientePorTipo(string tipo)
            => ObtenerSiguienteEstado("PENDIENTE", tipo);

        public string ObtenerEstadoPendienteAutorizacionPorTipo(string tipo)
            => ObtenerSiguienteEstado("PENDIENTE_AUTORIZACION", tipo);

        public string ObtenerEstadoPendienteCorreoPorTipo(string tipo)
            => ObtenerSiguienteEstado("PENDIENTE_CORREO", tipo);

        private string ObtenerSiguienteEstado(string prefijo, string tipo)
        {
            string sql = @"
        SELECT ESTADO
        FROM FACTURAS_PENDIENTES
        WHERE TIPO = '" + tipo + @"'
          AND ESTADO LIKE '" + prefijo + @"%'
    ";

            DataSet ds = _conexion.Seleccionar(sql);
            int max = 0;

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string valor = row["ESTADO"].ToString().Trim();
                    if (!valor.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase)) continue;
                    string numero = valor.Substring(prefijo.Length).Trim();
                    if (int.TryParse(numero, out int num) && num > max) max = num;
                }
            }

            return prefijo + (max + 1).ToString("000");
        }

        //Pintar boton accion

        public AccionPendienteDocumento ConsultarAccionPendienteDocumento(
            string numeroDocumento,
            string tipoDocumento   // "FACTURA" | "NOTADECREDITO" | "RETENCION"
        )
        {
            var r = new AccionPendienteDocumento
            {
                Existe = false,
                Tipo = tipoDocumento,
                NumeroDocumento = numeroDocumento,

                // DEFAULT: mostrar
                MostrarPdf = true,
                MostrarXml = true
            };

            // ==================================================
            // 0) VALIDACIONES BASE
            // ==================================================
            if (string.IsNullOrWhiteSpace(numeroDocumento))
                return r;

            if (string.IsNullOrWhiteSpace(tipoDocumento))
                return r;

            numeroDocumento = numeroDocumento.Trim().ToUpperInvariant();
            tipoDocumento = tipoDocumento.Trim().ToUpperInvariant();

            // ==================================================
            // 1) CASO ESPECIAL: NÚMERO PENDIENTE (NO EXISTE PDF/XML)
            // ==================================================
            if (numeroDocumento.StartsWith("PENDIENTE"))
            {
                r.MostrarPdf = false;
                r.MostrarXml = false;
            }

            // ==================================================
            // 2) CONSULTAR FACTURAS_PENDIENTES
            // ==================================================
            DataSet dsPendiente = ConsultarPorNumeroYTipo(
                numeroDocumento,
                tipoDocumento
            );

            if (dsPendiente == null ||
                dsPendiente.Tables.Count == 0 ||
                dsPendiente.Tables[0].Rows.Count == 0)
            {
                // No es pendiente → se respeta lo definido arriba
                return r;
            }

            DataRow rowPend = dsPendiente.Tables[0].Rows[0];

            string estadoPendiente =
                rowPend["ESTADO"]?.ToString()?.Trim().ToUpperInvariant() ?? "";

            if (string.IsNullOrWhiteSpace(estadoPendiente))
                return r;

            r.Existe = true;
            r.EstadoPendiente = estadoPendiente;

            // ==================================================
            // 3) DECIDIR ACCIÓN SEGÚN ESTADO
            // ==================================================
            if (estadoPendiente == "NO_AUTORIZADO")
            {
                r.TextoBoton = "ERROR";
                r.Accion = "NO_AUTORIZADO";
                r.MostrarPdf = false;
                r.MostrarXml = false;
                return r;
            }

            if (estadoPendiente.StartsWith("PENDIENTE_AUTORIZACION"))
            {
                r.TextoBoton = "CONSULTAR SRI";
                r.Accion = "AUTORIZAR";

                // ❌ NO mostrar nunca
                r.MostrarPdf = false;
                r.MostrarXml = false;

                return r;
            }

            if (estadoPendiente.StartsWith("PENDIENTE_CORREO"))
            {
                r.TextoBoton = "ENVIAR CORREO";
                r.Accion = "CORREO";

                return r;
            }

            if (estadoPendiente.StartsWith("PENDIENTE"))
            {
                r.TextoBoton = "PROCESAR";
                r.Accion = "PROCESAR";

                return r;
            }

            // ==================================================
            // 4) OTROS ESTADOS
            // ==================================================
            return r;
        }




        public class AccionPendienteDocumento
        {
            public bool Existe { get; set; }

            public string Tipo { get; set; }              // NOTA_CREDITO
            public string NumeroDocumento { get; set; }

            public string EstadoPendiente { get; set; }   // PENDIENTE / PENDIENTE_AUTORIZACION / PENDIENTE_CORREO

            public string TextoBoton { get; set; }         // PROCESAR / CONSULTAR SRI / ENVIAR CORREO
            public string Accion { get; set; }             // PROCESAR / AUTORIZAR / CORREO

            public bool MostrarPdf { get; set; }
            public bool MostrarXml { get; set; }
        }





    }
}
