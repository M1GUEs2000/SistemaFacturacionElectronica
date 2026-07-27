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
        // RENUMERAR + CAMBIAR ESTADO (EMISIÓN RECHAZADA POR EL SRI)
        // ==========================================================

        /// <summary>
        /// Cambia a la vez el NUMEROFACTURA y el ESTADO de un pendiente. Se usa cuando
        /// una factura que se estaba emitiendo con su secuencial real es rechazada por
        /// el SRI y pasa a ser un pendiente interno ("PENDIENTE001").
        ///
        /// Las dos tablas TIENEN que quedar con el mismo número: el reporte busca el
        /// estado en un mapa indexado por NUMEROFACTURA usando el número que muestra
        /// la grilla (que sale de FACTURACION). Si acá quedara el secuencial real y en
        /// FACTURACION el "PENDIENTE001", el documento aparece sin botón para procesar.
        ///
        /// Parametrizado: en Access los parámetros son POSICIONALES, así que el orden
        /// del array coincide con el orden de los @ en el SQL. La fecha va como string
        /// con el mismo formato que usa ConstruirFechaSqlUniversal, para no depender de
        /// si la columna es Fecha/Hora o texto.
        /// </summary>
        public int ActualizarNumeroYEstado(
            string numeroActual,
            string numeroNuevo,
            string estado,
            string tipo,
            string usuario,
            string ip)
        {
            string sql = @"
        UPDATE FACTURAS_PENDIENTES SET
            NUMEROFACTURA = @numNuevo,
            ESTADO        = @estado,
            FECHAREGISTRO = @fecha
        WHERE NUMEROFACTURA = @numActual
          AND TIPO          = @tipo";

            string fecha = DateTime.Now.ToString(
                "yyyy-MM-dd HH:mm:ss",
                System.Globalization.CultureInfo.InvariantCulture);

            _log.CrearLog(
                "Pendiente renumerado " + numeroActual + " -> " + numeroNuevo + " [" + estado + "]",
                usuario,
                ip,
                sql
            );

            return _conexion.Ejecutar(sql,
                ("numNuevo", numeroNuevo),
                ("estado", estado),
                ("fecha", fecha),
                ("numActual", numeroActual),
                ("tipo", (tipo ?? "").Trim().ToUpper()));
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

        // ==========================================================
        // CONSULTA POR DOCUMENTO INDIVIDUAL (clicks puntuales)
        //
        // Va a la BD por un solo documento y delega la decisión en
        // ConstruirAccionDesdeEstado. Usar SOLO para un documento suelto;
        // para pintar una grilla entera usar ConsultarEstadosPendientesPorTipo
        // + ConstruirAccionDesdeEstado, que hace 1 sola consulta en vez de N.
        // ==========================================================
        public AccionPendienteDocumento ConsultarAccionPendienteDocumento(
            string numeroDocumento,
            string tipoDocumento   // "FACTURA" | "NOTADECREDITO" | "RETENCION"
        )
        {
            if (string.IsNullOrWhiteSpace(numeroDocumento) ||
                string.IsNullOrWhiteSpace(tipoDocumento))
            {
                return ConstruirAccionDesdeEstado(numeroDocumento, tipoDocumento, null);
            }

            DataSet dsPendiente = ConsultarPorNumeroYTipo(
                numeroDocumento.Trim().ToUpperInvariant(),
                tipoDocumento.Trim().ToUpperInvariant()
            );

            string estado = null;
            if (dsPendiente != null &&
                dsPendiente.Tables.Count > 0 &&
                dsPendiente.Tables[0].Rows.Count > 0)
            {
                estado = dsPendiente.Tables[0].Rows[0]["ESTADO"]?.ToString();
            }

            return ConstruirAccionDesdeEstado(numeroDocumento, tipoDocumento, estado);
        }

        // ==========================================================
        // CONSULTA EN LOTE — TODOS LOS PENDIENTES DE UN TIPO (1 sola query)
        //
        // Reemplaza el N+1 al pintar grillas: en vez de consultar la BD por
        // cada fila, se trae de golpe el mapa numero→estado y se busca en
        // memoria. La tabla FACTURAS_PENDIENTES es chica (solo lo pendiente),
        // así que traerla entera cuesta menos que una fila-por-fila.
        // ==========================================================
        public System.Collections.Generic.Dictionary<string, string> ConsultarEstadosPendientesPorTipo(
            string tipoDocumento)
        {
            var mapa = new System.Collections.Generic.Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(tipoDocumento))
                return mapa;

            // Parametrizado: además de correcto, evita romper si el tipo trae comilla.
            const string sql = @"
                SELECT NUMEROFACTURA, ESTADO
                FROM FACTURAS_PENDIENTES
                WHERE TIPO = @tipo";

            DataSet ds = _conexion.Seleccionar(sql, ("tipo", tipoDocumento.Trim().ToUpper()));

            if (ds == null || ds.Tables.Count == 0)
                return mapa;

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                string numero = row["NUMEROFACTURA"]?.ToString()?.Trim().ToUpperInvariant() ?? "";
                if (numero.Length == 0)
                    continue;

                // Si hubiera duplicados, el primero gana — igual que el Rows[0] de antes.
                if (!mapa.ContainsKey(numero))
                    mapa[numero] = row["ESTADO"]?.ToString()?.Trim().ToUpperInvariant() ?? "";
            }

            return mapa;
        }

        // ==========================================================
        // DECISIÓN PURA — de un estado ya conocido al botón/PDF/XML
        //
        // Sin acceso a BD: recibe el estado (null/"" si el documento NO está
        // en FACTURAS_PENDIENTES) y devuelve qué pintar. Lo comparten la
        // consulta individual y la de lote, para que ambas decidan igual.
        // ==========================================================
        public AccionPendienteDocumento ConstruirAccionDesdeEstado(
            string numeroDocumento,
            string tipoDocumento,
            string estadoPendiente)
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

            // ==================================================
            // 1) CASO ESPECIAL: NÚMERO PENDIENTE (NO EXISTE PDF/XML)
            // ==================================================
            if (numeroDocumento.StartsWith("PENDIENTE"))
            {
                r.MostrarPdf = false;
                r.MostrarXml = false;
            }

            // ==================================================
            // 2) ¿ESTÁ EN FACTURAS_PENDIENTES?
            // ==================================================
            estadoPendiente = estadoPendiente?.Trim().ToUpperInvariant() ?? "";

            if (string.IsNullOrWhiteSpace(estadoPendiente))
            {
                // No es pendiente → se respeta lo definido arriba
                return r;
            }

            r.Existe = true;
            r.EstadoPendiente = estadoPendiente;

            // ==================================================
            // 3) DECIDIR ACCIÓN SEGÚN ESTADO
            // ==================================================
            // EMITIENDO = la factura se insertó en BD y se mandó al SRI, pero el
            // proceso nunca llegó a escribir el desenlace (corte de luz, crash).
            // No sabemos si el SRI la recibió, así que se ofrece el mismo botón que
            // un PENDIENTE_AUTORIZACION: ir a preguntarle al SRI por la clave de
            // acceso. PDF y XML todavía no existen.
            if (estadoPendiente == "EMITIENDO")
            {
                r.TextoBoton = "CONSULTAR SRI";
                r.Accion = "AUTORIZAR";
                r.MostrarPdf = false;
                r.MostrarXml = false;
                return r;
            }

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
