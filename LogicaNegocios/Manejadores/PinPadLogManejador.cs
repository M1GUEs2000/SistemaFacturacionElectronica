using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using AccesoDatos.Abstractions;
using DF_PinPad.Wrapper.Logging;
using DF_PinPad.Wrapper.Models;

namespace LogicaNegocios
{
    /// <summary>
    /// Datos del cobro original que se necesitan para anularlo. Los tres primeros
    /// campos son los que pide AnulacionRequest de la DLL; el resto es para mostrarle
    /// al usuario qué está por anular antes de confirmar.
    /// </summary>
    public class DatosAnulacion
    {
        public string NumeroFactura { get; set; }
        public string Red { get; set; }
        public string Referencia { get; set; }
        public string Autorizacion { get; set; }
        public string RedAdquirente { get; set; }
        public string NumeroTarjeta { get; set; }
        public string NombreGrupoTarjeta { get; set; }
        public decimal Monto { get; set; }
    }

    /// <summary>
    /// Implementación de <see cref="ISqlLogger"/> respaldada en Access (la misma .accdb del POS),
    /// NO en SQL Server. Persiste la auditoría del pinpad en 3 tablas:
    ///   PINPAD_LOG          — cabecera (IniciarTransaccion/FinalizarTransaccion/VincularNumeroFactura)
    ///   PINPAD_AUTORIZADAS  — detalle del cobro (GuardarDetallePago)
    ///   PINPAD_ANULACIONES  — detalle de anulación (RegistrarAnulacion)
    ///
    /// REGLA: las dos tablas de detalle son SOLO de operaciones aprobadas. Todo lo que el
    /// autorizador no devuelva en "00" —rechazos, errores de trama, excepciones— queda
    /// únicamente en PINPAD_LOG, que para eso es la bitácora.
    ///
    /// MODELO SIN ID NUMÉRICO: la llave de correlación entre las 3 tablas es la NUMEROFACTURA
    /// (columna TRANSACCIONID en las tablas de detalle guarda esa misma factura). Como la
    /// interfaz ISqlLogger identifica cada operación con un `long` (no un string) y hay
    /// operaciones SIN factura (leer tarjeta, consultar config, anular), se usa un TOKEN
    /// interno (contador) mapeado EN MEMORIA a la "clave de fila" que va en NUMEROFACTURA:
    /// la factura real si la hay, o un marcador único "OP-{ticks}-{token}" si no.
    /// FinalizarTransaccion / VincularNumeroFactura resuelven la clave por ese token.
    ///
    /// REGLA CRÍTICA: todos los métodos TRAGAN sus propias excepciones. El wrapper
    /// (PinPadService) llama a GuardarDetallePago/FinalizarTransaccion DESPUÉS de que la
    /// tarjeta ya fue cobrada; si el logger relanzara, se perdería el resultado de un cobro
    /// real. Un fallo de auditoría nunca debe tumbar la transacción.
    ///
    /// ⚠️ En Access, las columnas NUMEROFACTURA y TRANSACCIONID deben ser de tipo TEXTO
    /// (la factura lleva guiones, ej. "001-001-000000123"). EXITOSO es Sí/No (True = -1).
    /// </summary>
    public class PinPadLogManejador : ISqlLogger
    {
        private readonly IConexionBD _conexion;

        // token -> clave de fila (lo que va en NUMEROFACTURA). Correlaciona en memoria el
        // INSERT inicial con los UPDATE/INSERT posteriores, que la interfaz solo identifica
        // por `long`. Estático el contador (uniquidad entre instancias); el mapa por instancia.
        private static long _secuenciaToken = 0;
        private readonly ConcurrentDictionary<long, string> _clavePorToken = new ConcurrentDictionary<long, string>();

        // token -> tarjeta/autorización/referencia. Los métodos de detalle los reciben pero
        // FinalizarTransaccion no (su firma la fija ISqlLogger), y es ahí donde se escribe
        // la cabecera. Lo llenan TANTO GuardarDetallePago (cobro) COMO GuardarDetalleAnulacion:
        // si solo lo llenara el cobro, las anulaciones guardarían esas 3 columnas vacías.
        private readonly ConcurrentDictionary<long, (string Tarjeta, string Autorizacion, string Referencia)> _datosPorToken =
            new ConcurrentDictionary<long, (string, string, string)>();

        public PinPadLogManejador(IConexionBD conexion)
        {
            _conexion = conexion;
        }

        private static long NuevoToken()
        {
            return Interlocked.Increment(ref _secuenciaToken);
        }

        private string ClaveDe(long token)
        {
            return _clavePorToken.TryGetValue(token, out string clave) ? clave : null;
        }

        // =====================================================================
        // CABECERA — PINPAD_LOG
        // =====================================================================

        public long IniciarTransaccion(string tipoOperacion, string usuarioSistema, string cajaId = null, string numeroFactura = null)
        {
            long token = NuevoToken();

            // Clave de fila: la factura real si viene; si no (leer tarjeta, consultar config,
            // anular), un marcador único e irrepetible entre corridas para poder ubicar la
            // fila en el UPDATE de FinalizarTransaccion.
            string clave = string.IsNullOrWhiteSpace(numeroFactura)
                ? ("OP-" + DateTime.Now.Ticks + "-" + token)
                : numeroFactura.Trim();
            _clavePorToken[token] = clave;

            try
            {
                string sql = @"INSERT INTO PINPAD_LOG
                    (FECHAINICIO, TIPOOPERACION, USUARIOSISTEMA, CAJAID, NUMEROFACTURA, MAQUINAORIGEN)
                    VALUES (@fecha, @tipo, @usuario, @caja, @factura, @maquina)";

                _conexion.Ejecutar(sql,
                    ("fecha", DateTime.Now),
                    ("tipo", tipoOperacion ?? ""),
                    ("usuario", usuarioSistema ?? ""),
                    ("caja", cajaId ?? ""),
                    ("factura", clave),
                    ("maquina", Environment.MachineName ?? ""));
            }
            catch { /* swallow: auditoría no bloquea el cobro */ }

            return token;
        }

        /// <summary>
        /// ⚠️ codigoRespuesta y codigoRespuestaAut son DISTINTOS: el primero es el nivel LAN
        /// (pinpad: 00/01/02/20/ER) y el segundo el del autorizador (00=aprobada, 51=sin cupo,
        /// 54=vencida…). Se guardan en columnas separadas —CODIGORESPUESTA y CODIGOAUTORIZACION—
        /// justamente para poder distinguir un rechazo del banco de un fallo de comunicación.
        ///
        /// No confundir CODIGOAUTORIZACION (código de respuesta del autorizador, 2 dígitos) con
        /// AUTORIZACION (el número de autorización que devuelve un cobro aprobado).
        /// </summary>
        public void FinalizarTransaccion(long transaccionId, string codigoRespuesta, string mensajeRespuesta,
            bool exitoso, string excepcionMensaje, string codigoRespuestaAut = null)
        {
            string clave = ClaveDe(transaccionId);
            if (clave == null) return; // sin clave mapeada no hay fila que actualizar

            _datosPorToken.TryGetValue(transaccionId, out var datos);

            try
            {
                // ⚠️ Los parámetros de Access son POSICIONALES: el orden del array debe
                // coincidir con el orden de los @ en el SQL.
                string sql = @"UPDATE PINPAD_LOG SET
                    CODIGORESPUESTA = @cod,
                    CODIGOAUTORIZACION = @codAut,
                    NUMEROTARJETA = @tarjeta,
                    AUTORIZACION = @autoriz,
                    REFERENCIA = @refer,
                    MENSAJERESPUESTA = @msg,
                    EXITOSO = @exito,
                    EXCEPCIONMENSAJE = @exc,
                    FECHAFIN = @fin
                    WHERE NUMEROFACTURA = @factura";

                _conexion.Ejecutar(sql,
                    ("cod", codigoRespuesta ?? ""),
                    ("codAut", codigoRespuestaAut ?? ""),
                    ("tarjeta", datos.Tarjeta ?? ""),
                    ("autoriz", datos.Autorizacion ?? ""),
                    ("refer", datos.Referencia ?? ""),
                    ("msg", mensajeRespuesta ?? ""),
                    ("exito", exitoso),
                    ("exc", excepcionMensaje ?? ""),
                    ("fin", DateTime.Now),
                    ("factura", clave));
            }
            catch { /* swallow: auditoría no bloquea el cobro */ }
        }

        public void VincularNumeroFactura(long transaccionId, string numeroFactura)
        {
            string claveAnterior = ClaveDe(transaccionId);
            if (claveAnterior == null || string.IsNullOrWhiteSpace(numeroFactura)) return;

            string claveNueva = numeroFactura.Trim();
            if (claveNueva == claveAnterior) return;

            try
            {
                // Renombrar la factura en todas las tablas que la usan como llave (el
                // secuencial previsto pudo cambiar por reintento del SRI).
                _conexion.Ejecutar(
                    "UPDATE PINPAD_LOG SET NUMEROFACTURA = @nueva WHERE NUMEROFACTURA = @vieja",
                    ("nueva", claveNueva), ("vieja", claveAnterior));

                _conexion.Ejecutar(
                    "UPDATE PINPAD_AUTORIZADAS SET NUMEROFACTURA = @nueva, TRANSACCIONID = @nueva WHERE NUMEROFACTURA = @vieja",
                    ("nueva", claveNueva), ("vieja", claveAnterior));

                _clavePorToken[transaccionId] = claveNueva;
            }
            catch { /* swallow */ }
        }

        // =====================================================================
        // DETALLE COBRO — PINPAD_AUTORIZADAS
        // =====================================================================

        public void GuardarDetallePago(long transaccionId, ProcesoPagoRequest request, ProcesoPagoResult result)
        {
            // La factura es la llave (TRANSACCIONID = NUMEROFACTURA). Preferimos la del
            // request; si no viniera, la clave mapeada por el token.
            string clave = !string.IsNullOrWhiteSpace(request?.NumeroFactura)
                ? request.NumeroFactura.Trim()
                : (ClaveDe(transaccionId) ?? "");

            _datosPorToken.AddOrUpdate(transaccionId,
                (result?.NumeroTarjeta, result?.Autorizacion, result?.Referencia),
                (t, previo) => (string.IsNullOrWhiteSpace(result?.NumeroTarjeta) ? previo.Tarjeta : result.NumeroTarjeta,
                                result?.Autorizacion, result?.Referencia));

            // PINPAD_AUTORIZADAS es solo de cobros APROBADOS: el wrapper llama a este método
            // pase lo que pase, y sin este filtro entraban filas vacías o con basura
            // (ej. "ERROR EN TRAMA" y el resto en blanco). Lo no aprobado queda en PINPAD_LOG,
            // que para eso es la bitácora.
            //
            // Se piden los DOS códigos en "00" —misma regla que ProcesosTarjetas usa para
            // decidir si emite la factura—: el del autorizador puede venir "00" cuando en
            // realidad falló la trama del pinpad y el autorizador nunca contestó.
            if (result == null) return;

            if (!string.Equals((result.CodigoRespuesta ?? "").Trim(), "00", StringComparison.Ordinal) ||
                !string.Equals((result.CodigoRespuestaAut ?? "").Trim(), "00", StringComparison.Ordinal))
                return;

            try
            {
                string sql = @"INSERT INTO PINPAD_AUTORIZADAS
                    (TRANSACCIONID, MONTO, TIPOTRANSACCION, RED,
                     CODIGORESPUESTAAUT, MENSAJERESPUESTAAUT, REDADQUIRENTE,
                     REFERENCIA, LOTE, AUTORIZACION, TID, MID,
                     CODIGOADQUIRENTE, NOMBREADQUIRENTE, NOMBREGRUPOTARJETA,
                     MODOLECTURA, TARJETAHABIENTE, NUMEROTARJETA, FECHAVENCIMIENTO, NUMEROFACTURA)
                    VALUES
                    (@transId, @monto, @tipoTrans, @red,
                     @codAut, @msgAut, @redAdq,
                     @refer, @lote, @autoriz, @tid, @mid,
                     @codAdq, @nomAdq, @grupo,
                     @modo, @habiente, @tarjeta, @vence, @factura)";

                _conexion.Ejecutar(sql,
                    ("transId", clave),
                    ("monto", request?.Monto ?? 0m),
                    ("tipoTrans", request?.TipoTransaccion ?? ""),
                    ("red", request?.Red ?? ""),
                    ("codAut", result?.CodigoRespuestaAut ?? ""),
                    ("msgAut", result?.MensajeRespuestaAut ?? ""),
                    ("redAdq", result?.RedAdquirente ?? ""),
                    ("refer", result?.Referencia ?? ""),
                    ("lote", result?.Lote ?? ""),
                    ("autoriz", result?.Autorizacion ?? ""),
                    ("tid", result?.TID ?? ""),
                    ("mid", result?.MID ?? ""),
                    ("codAdq", result?.CodigoAdquirente ?? ""),
                    ("nomAdq", result?.NombreAdquirente ?? ""),
                    ("grupo", result?.NombreGrupoTarjeta ?? ""),
                    ("modo", result?.ModoLectura ?? ""),
                    ("habiente", result?.TarjetaHabiente ?? ""),
                    ("tarjeta", result?.NumeroTarjeta ?? ""),
                    ("vence", result?.FechaVencimiento ?? ""),
                    ("factura", clave));
            }
            catch { /* swallow */ }
        }

        // =====================================================================
        // DETALLE ANULACIÓN — PINPAD_ANULACIONES
        // =====================================================================

        /// <summary>
        /// NO escribe en PINPAD_ANULACIONES: solo guarda los datos del cobro original para
        /// que FinalizarTransaccion los ponga en la cabecera de PINPAD_LOG.
        ///
        /// La DLL llama a este método ANTES de saber si el pinpad aceptó la anulación (su
        /// firma la fija ISqlLogger y no trae el resultado), así que desde aquí es imposible
        /// aplicar la regla de "solo se guarda con 00". Además la anulación entraría con
        /// NUMEROFACTURA = "OP-..." porque AnulacionRequest no lleva factura.
        ///
        /// Quien inserta en PINPAD_ANULACIONES es <see cref="RegistrarAnulacion"/>, que llama
        /// la UI DESPUÉS de verificar la respuesta y con la factura real. Una anulación
        /// rechazada queda solo en PINPAD_LOG, igual que un cobro no aprobado.
        /// </summary>
        public void GuardarDetalleAnulacion(long transaccionId, string referenciaOriginal, string autorizacionOriginal, string redAdquirente)
        {
            // Este método no trae la tarjeta (solo referencia/autorización del cobro
            // original); la manda GuardarDetalleTarjeta. Se conserva la que ya estuviera
            // guardada porque no hay garantía de cuál de los dos llama la DLL primero.
            _datosPorToken.AddOrUpdate(transaccionId,
                (null, autorizacionOriginal, referenciaOriginal),
                (t, previo) => (previo.Tarjeta, autorizacionOriginal, referenciaOriginal));
        }

        // =====================================================================
        // ANULACIÓN DESDE EL REPORTE — consulta y registro con factura REAL
        //
        // A diferencia de todo lo de arriba (que lo dispara el wrapper), estos
        // métodos los llama la UI para anular un cobro ya hecho. Por eso NO tragan
        // excepciones: si falla la lectura, el usuario debe enterarse en vez de que
        // se anule a ciegas o el botón no haga nada.
        // =====================================================================

        /// <summary>
        /// Datos del cobro original necesarios para armar el AnulacionRequest de la DLL
        /// (Red + Autorizacion + Referencia). Monto y tarjeta son solo para confirmarle
        /// al usuario QUÉ está por anular.
        /// </summary>
        public DatosAnulacion ConsultarCobroParaAnular(string numeroFactura, string autorizacion)
        {
            if (string.IsNullOrWhiteSpace(numeroFactura) || string.IsNullOrWhiteSpace(autorizacion))
                return null;

            // ⚠️ Los parámetros de Access son POSICIONALES: el orden del array debe
            // coincidir con el orden de los @ en el SQL.
            string sql = @"SELECT TOP 1 RED, REFERENCIA, AUTORIZACION, REDADQUIRENTE,
                                  NUMEROTARJETA, NOMBREGRUPOTARJETA, MONTO
                           FROM PINPAD_AUTORIZADAS
                           WHERE NUMEROFACTURA = @factura AND AUTORIZACION = @autoriz";

            DataSet ds = _conexion.Seleccionar(sql,
                ("factura", numeroFactura.Trim()),
                ("autoriz", autorizacion.Trim()));

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return null;

            DataRow fila = ds.Tables[0].Rows[0];

            return new DatosAnulacion
            {
                NumeroFactura = numeroFactura.Trim(),
                // La DLL espera en Red el MISMO selector que se mandó al cobrar
                // (ProcesoPagoRequest.Red = "Datafast"), no el adquirente.
                Red = Texto(fila, "RED"),
                Referencia = Texto(fila, "REFERENCIA"),
                Autorizacion = Texto(fila, "AUTORIZACION"),
                RedAdquirente = Texto(fila, "REDADQUIRENTE"),
                NumeroTarjeta = Texto(fila, "NUMEROTARJETA"),
                NombreGrupoTarjeta = Texto(fila, "NOMBREGRUPOTARJETA"),
                Monto = fila["MONTO"] == DBNull.Value ? 0m : Convert.ToDecimal(fila["MONTO"])
            };
        }

        /// <summary>¿La factura ya tiene una anulación registrada? Evita mandar dos veces
        /// la misma anulación al pinpad.</summary>
        public bool YaAnulada(string numeroFactura)
        {
            if (string.IsNullOrWhiteSpace(numeroFactura))
                return false;

            DataSet ds = _conexion.Seleccionar(
                "SELECT COUNT(*) AS TOTAL FROM PINPAD_ANULACIONES WHERE NUMEROFACTURA = @factura",
                ("factura", numeroFactura.Trim()));

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return false;

            return Convert.ToInt32(ds.Tables[0].Rows[0]["TOTAL"]) > 0;
        }

        /// <summary>
        /// Registra la anulación CON EL NÚMERO REAL DE FACTURA.
        ///
        /// Es indispensable hacerlo desde aquí: AnulacionRequest no lleva número de
        /// factura, así que la fila que graba sola la DLL cae en el caso "operación sin
        /// factura" de IniciarTransaccion y queda con NUMEROFACTURA = "OP-...". El reporte
        /// calcula el estado ANULADA uniendo PINPAD_ANULACIONES.NUMEROFACTURA con la
        /// factura, así que sin esta fila el cobro anulado seguiría viéndose APROBADA.
        /// </summary>
        public void RegistrarAnulacion(string numeroFactura, string referenciaOriginal,
            string autorizacionOriginal, string redAdquirente)
        {
            string clave = (numeroFactura ?? "").Trim();
            if (clave.Length == 0)
                return;

            string sql = @"INSERT INTO PINPAD_ANULACIONES
                (TRANSACCIONID, REFERENCIAORIGINAL, AUTORIZACIONORIGINAL, REDADQUIRENTE, NUMEROFACTURA)
                VALUES (@transId, @refer, @autoriz, @red, @factura)";

            _conexion.Ejecutar(sql,
                ("transId", clave),
                ("refer", referenciaOriginal ?? ""),
                ("autoriz", autorizacionOriginal ?? ""),
                ("red", redAdquirente ?? ""),
                ("factura", clave));
        }

        // =====================================================================
        // CONSULTA DE AUDITORIA - PINPAD_LOG
        // =====================================================================

        public DataSet ConsultarLog(DateTime fechaDesde, DateTime fechaHasta,
            string numeroTarjeta, string tipoOperacion)
        {
            string sql = @"SELECT
                    FECHAINICIO AS [FECHA INICIO],
                    FECHAFIN AS [FECHA FIN],
                    TIPOOPERACION AS [TIPO OPERACION],
                    NUMEROTARJETA AS [NUMERO TARJETA],
                    NUMEROFACTURA AS [NUMERO FACTURA],
                    AUTORIZACION,
                    REFERENCIA,
                    CODIGORESPUESTA AS [CODIGO RESPUESTA],
                    MENSAJERESPUESTA AS [MENSAJE RESPUESTA],
                    IIF(EXITOSO, 'SI', 'NO') AS EXITOSO,
                    EXCEPCIONMENSAJE AS [EXCEPCION]
                FROM PINPAD_LOG
                WHERE FECHAINICIO >= @desde
                  AND FECHAINICIO < @hasta";

            var parametros = new List<(string nombre, object valor)>
            {
                ("desde", fechaDesde.Date),
                ("hasta", fechaHasta.Date.AddDays(1))
            };

            if (!string.IsNullOrWhiteSpace(numeroTarjeta))
            {
                sql += " AND NUMEROTARJETA LIKE @tarjeta";
                parametros.Add(("tarjeta", "%" + numeroTarjeta.Trim() + "%"));
            }

            if (!string.IsNullOrWhiteSpace(tipoOperacion) && tipoOperacion != "Todos")
            {
                sql += " AND TIPOOPERACION = @tipo";
                parametros.Add(("tipo", tipoOperacion.Trim()));
            }

            sql += " ORDER BY FECHAINICIO DESC, FECHAFIN DESC";

            return _conexion.Seleccionar(sql, parametros.ToArray());
        }

        public DataSet ConsultarTiposOperacion()
        {
            const string sql = @"SELECT DISTINCT TIPOOPERACION
                                 FROM PINPAD_LOG
                                 WHERE TIPOOPERACION IS NOT NULL
                                   AND TIPOOPERACION <> ''
                                 ORDER BY TIPOOPERACION";

            DataSet ds = _conexion.Seleccionar(sql);
            if (ds != null && ds.Tables.Count > 0)
            {
                DataRow todos = ds.Tables[0].NewRow();
                todos["TIPOOPERACION"] = "Todos";
                ds.Tables[0].Rows.InsertAt(todos, 0);
            }

            return ds;
        }

        public DataSet ConsultarTarjetas(string numeroTarjeta)
        {
            string sql = @"SELECT DISTINCT TOP 20 NUMEROTARJETA
                           FROM PINPAD_LOG
                           WHERE NUMEROTARJETA IS NOT NULL
                             AND NUMEROTARJETA <> ''";

            if (string.IsNullOrWhiteSpace(numeroTarjeta))
            {
                sql += " ORDER BY NUMEROTARJETA";
                return _conexion.Seleccionar(sql);
            }

            sql += " AND NUMEROTARJETA LIKE @tarjeta ORDER BY NUMEROTARJETA";
            return _conexion.Seleccionar(sql,
                ("tarjeta", "%" + numeroTarjeta.Trim() + "%"));
        }

        private static string Texto(DataRow fila, string columna)
        {
            if (!fila.Table.Columns.Contains(columna) || fila[columna] == DBNull.Value)
                return "";

            return fila[columna].ToString().Trim();
        }

        /// <summary>
        /// No hay tabla de detalle de tarjeta, pero de aquí sale el NUMEROTARJETA de la
        /// cabecera en las ANULACIONES: es la tarjeta que realmente se pasó al anular
        /// —la equivocada, si se equivocaron— y no la del cobro original.
        /// </summary>
        public void GuardarDetalleTarjeta(long transaccionId,
            string numeroTarjeta, string numeroTarjetaEncriptado, string binTarjeta,
            string fechaVencimiento, string redAdquirienteCorriente, string redAdquirienteDiferido)
        {
            if (string.IsNullOrWhiteSpace(numeroTarjeta)) return;

            // Merge: la autorización/referencia las pone GuardarDetalleAnulacion y el
            // orden entre ambos no está garantizado.
            _datosPorToken.AddOrUpdate(transaccionId,
                (numeroTarjeta, null, null),
                (t, previo) => (numeroTarjeta, previo.Autorizacion, previo.Referencia));
        }

        // =====================================================================
        // NO USADOS — el modelo Access es de 3 tablas.
        // =====================================================================

        public void GuardarDetalleConfigRed(long transaccionId, ConfiguracionRedRequest request)
        { /* no-op */ }

        public void GuardarDetalleProcesoControl(long transaccionId, string lote, string referencia)
        { /* no-op */ }

        public void GuardarTrama(long transaccionId, string direccion, string tramaHex)
        { /* no-op */ }

        public void RegistrarEvento(string tipoEvento, string origen, string mensaje, long? transaccionId = null)
        { /* no-op */ }
    }
}
