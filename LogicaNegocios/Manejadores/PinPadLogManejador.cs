using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;
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
    /// NO en SQL Server. PINPAD_LOG es la cabecera de cada operación y TRANSACCIONID
    /// relaciona sus detalles, tramas y eventos. NUMEROFACTURA es una relación comercial:
    /// varias operaciones (cobro, intentos y anulación) pueden pertenecer a la misma factura.
    ///
    /// REGLA: PINPAD_AUTORIZADAS y PINPAD_ANULACIONES son SOLO de operaciones aprobadas.
    /// Los intentos rechazados permanecen en PINPAD_LOG, eventos, tramas y detalle técnico.
    ///
    /// MODELO SIN AUTONUMÉRICO: ISqlLogger exige devolver un long, por lo que se mantiene
    /// un token efímero en memoria. En base se genera un TRANSACCIONID textual único "TX-..."
    /// que nunca se confunde con el número de factura.
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

        // El token satisface el contrato de ISqlLogger durante la ejecución. TRANSACCIONID
        // es la llave persistente, y NUMEROFACTURA se conserva en un mapa separado.
        private static long _secuenciaToken = 0;
        private readonly ConcurrentDictionary<long, string> _transaccionPorToken =
            new ConcurrentDictionary<long, string>();
        private readonly ConcurrentDictionary<long, string> _facturaPorToken =
            new ConcurrentDictionary<long, string>();

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

        private string TransaccionDe(long token)
        {
            return _transaccionPorToken.TryGetValue(token, out string transaccionId)
                ? transaccionId
                : null;
        }

        private string FacturaDe(long token)
        {
            return _facturaPorToken.TryGetValue(token, out string numeroFactura)
                ? numeroFactura
                : "";
        }

        private static string NuevaTransaccionId()
        {
            return "TX-" + Guid.NewGuid().ToString("N");
        }

        // =====================================================================
        // CABECERA — PINPAD_LOG
        // =====================================================================

        public long IniciarTransaccion(string tipoOperacion, string usuarioSistema, string cajaId = null, string numeroFactura = null)
        {
            long token = NuevoToken();
            string transaccionId = NuevaTransaccionId();
            string factura = (numeroFactura ?? "").Trim();
            _transaccionPorToken[token] = transaccionId;
            _facturaPorToken[token] = factura;

            try
            {
                string sql = @"INSERT INTO PINPAD_LOG
                    (TRANSACCIONID, FECHAINICIO, TIPOOPERACION, USUARIOSISTEMA,
                     CAJAID, NUMEROFACTURA, MAQUINAORIGEN)
                    VALUES (@transId, @fecha, @tipo, @usuario, @caja, @factura, @maquina)";

                _conexion.Ejecutar(sql,
                    ("transId", transaccionId),
                    ("fecha", DateTime.Now),
                    ("tipo", tipoOperacion ?? ""),
                    ("usuario", usuarioSistema ?? ""),
                    ("caja", cajaId ?? ""),
                    ("factura", factura),
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
            string transaccionDb = TransaccionDe(transaccionId);
            if (transaccionDb == null) return;

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
                    WHERE TRANSACCIONID = @transId";

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
                    ("transId", transaccionDb));
            }
            catch { /* swallow: auditoría no bloquea el cobro */ }
        }

        public void VincularNumeroFactura(long transaccionId, string numeroFactura)
        {
            string transaccionDb = TransaccionDe(transaccionId);
            if (transaccionDb == null || string.IsNullOrWhiteSpace(numeroFactura)) return;

            string facturaAnterior = FacturaDe(transaccionId);
            string facturaNueva = numeroFactura.Trim();
            if (facturaNueva == facturaAnterior) return;

            try
            {
                _conexion.Ejecutar(
                    "UPDATE PINPAD_LOG SET NUMEROFACTURA = @factura WHERE TRANSACCIONID = @transId",
                    ("factura", facturaNueva), ("transId", transaccionDb));

                _conexion.Ejecutar(
                    "UPDATE PINPAD_AUTORIZADAS SET NUMEROFACTURA = @factura WHERE TRANSACCIONID = @transId",
                    ("factura", facturaNueva), ("transId", transaccionDb));

                _conexion.Ejecutar(
                    "UPDATE PINPAD_PAGO_EXTENDIDO SET NUMEROFACTURA = @factura WHERE TRANSACCIONID = @transId",
                    ("factura", facturaNueva), ("transId", transaccionDb));

                _conexion.Ejecutar(
                    "UPDATE PINPAD_ANULACIONES SET NUMEROFACTURA = @factura WHERE TRANSACCIONID = @transId",
                    ("factura", facturaNueva), ("transId", transaccionDb));

                _facturaPorToken[transaccionId] = facturaNueva;
            }
            catch { /* swallow */ }
        }

        // =====================================================================
        // DETALLE COBRO — PINPAD_AUTORIZADAS
        // =====================================================================

        public void GuardarDetallePago(long transaccionId, ProcesoPagoRequest request, ProcesoPagoResult result)
        {
            string transaccionDb = TransaccionDe(transaccionId);
            if (transaccionDb == null) return;

            string factura = !string.IsNullOrWhiteSpace(request?.NumeroFactura)
                ? request.NumeroFactura.Trim()
                : FacturaDe(transaccionId);

            _datosPorToken.AddOrUpdate(transaccionId,
                (result?.NumeroTarjeta, result?.Autorizacion, result?.Referencia),
                (t, previo) => (string.IsNullOrWhiteSpace(result?.NumeroTarjeta) ? previo.Tarjeta : result.NumeroTarjeta,
                                result?.Autorizacion, result?.Referencia));

            GuardarPagoExtendido(transaccionDb, factura, result);

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
                    ("transId", transaccionDb),
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
                    ("factura", factura));
            }
            catch { /* swallow */ }
        }

        private void GuardarPagoExtendido(string transaccionId, string numeroFactura, ProcesoPagoResult result)
        {
            if (result == null) return;

            object valorInteres = DBNull.Value;
            if (decimal.TryParse(result.ValorInteres,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal interes))
            {
                valorInteres = interes;
            }

            try
            {
                string sql = @"INSERT INTO PINPAD_PAGO_EXTENDIDO
                    (TRANSACCIONID, NUMEROFACTURA, NUMEROTARJETAENCRIPTADO,
                     VALORINTERES, MENSAJEPUBLICIDAD, APLICACIONEMV, AID,
                     CRIPTOGRAMA, VERIFICACIONPIN, ARQC, TVR, TSI, FECHAREGISTRO)
                    VALUES
                    (@transId, @factura, @tarjetaEnc,
                     @interes, @publicidad, @aplicacion, @aid,
                     @criptograma, @pin, @arqc, @tvr, @tsi, @fecha)";

                _conexion.Ejecutar(sql,
                    ("transId", transaccionId),
                    ("factura", numeroFactura ?? ""),
                    ("tarjetaEnc", result.NumeroTarjetaEncriptado ?? ""),
                    ("interes", valorInteres),
                    ("publicidad", result.MensajePublicidad ?? ""),
                    ("aplicacion", result.AplicacionEMV ?? ""),
                    ("aid", result.AID ?? ""),
                    ("criptograma", result.Criptograma ?? ""),
                    ("pin", result.VerificacionPIN ?? ""),
                    ("arqc", result.ARQC ?? ""),
                    ("tvr", result.TVR ?? ""),
                    ("tsi", result.TSI ?? ""),
                    ("fecha", DateTime.Now));
            }
            catch { /* la auditoría extendida no bloquea el cobro */ }
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
        /// aplicar la regla de "solo se guarda con 00".
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
        /// El reporte calcula el estado ANULADA uniendo PINPAD_ANULACIONES.NUMEROFACTURA
        /// con la factura, así que esta fila se crea únicamente tras una respuesta aprobada.
        /// </summary>
        public void RegistrarAnulacion(long transaccionId, string numeroFactura, string referenciaOriginal,
            string autorizacionOriginal, string redAdquirente)
        {
            string transaccionDb = TransaccionDe(transaccionId);
            string factura = (numeroFactura ?? "").Trim();
            if (transaccionDb == null || factura.Length == 0)
                return;

            string sql = @"INSERT INTO PINPAD_ANULACIONES
                (TRANSACCIONID, REFERENCIAORIGINAL, AUTORIZACIONORIGINAL, REDADQUIRENTE, NUMEROFACTURA)
                VALUES (@transId, @refer, @autoriz, @red, @factura)";

            _conexion.Ejecutar(sql,
                ("transId", transaccionDb),
                ("refer", referenciaOriginal ?? ""),
                ("autoriz", autorizacionOriginal ?? ""),
                ("red", redAdquirente ?? ""),
                ("factura", factura));
        }

        // =====================================================================
        // CONSULTA DE AUDITORIA - PINPAD_LOG
        // =====================================================================

        /// <summary>
        /// Bitácora completa filtrada. AUTORIZACION y REFERENCIA se buscan sobre PINPAD_LOG
        /// (no sobre PINPAD_AUTORIZADAS) para que también aparezcan las anulaciones, que
        /// guardan ahí los datos del cobro original.
        /// </summary>
        public DataSet ConsultarLog(DateTime fechaDesde, DateTime fechaHasta,
            string numeroTarjeta, string numeroFactura, string tipoOperacion,
            string autorizacion = null, string referencia = null)
        {
            string sql = @"SELECT
                    L.TRANSACCIONID AS [TRANSACCION ID],
                    L.NUMEROFACTURA AS [NUMERO FACTURA],
                    L.TIPOOPERACION AS [TIPO OPERACION],
                    L.FECHAINICIO AS [FECHA INICIO],
                    L.FECHAFIN AS [FECHA FIN],
                    L.CODIGORESPUESTA AS [CODIGO RESPUESTA],
                    L.CODIGOAUTORIZACION AS [CODIGO AUTORIZADOR],
                    L.MENSAJERESPUESTA AS [MENSAJE RESPUESTA],
                    IIF(L.EXITOSO, 'SI', 'NO') AS EXITOSO,
                    L.NUMEROTARJETA AS [NUMERO TARJETA],
                    L.AUTORIZACION AS [AUTORIZACION],
                    L.REFERENCIA AS [REFERENCIA],
                    L.EXCEPCIONMENSAJE AS [EXCEPCION],
                    L.CAJAID AS [CAJA ID],
                    L.USUARIOSISTEMA AS [USUARIO],
                    L.MAQUINAORIGEN AS [MAQUINA],

                    PA.MONTO AS [PAGO MONTO],
                    PA.TIPOTRANSACCION AS [PAGO TIPO],
                    PA.RED AS [PAGO RED],
                    PA.REDADQUIRENTE AS [PAGO RED ADQUIRENTE],
                    PA.LOTE AS [LOTE],
                    PA.TID AS [PAGO TID],
                    PA.MID AS [PAGO MID],
                    PA.CODIGOADQUIRENTE AS [PAGO CODIGO ADQUIRENTE],
                    PA.NOMBREADQUIRENTE AS [PAGO NOMBRE ADQUIRENTE],
                    PA.NOMBREGRUPOTARJETA AS [PAGO GRUPO TARJETA],
                    PA.MODOLECTURA AS [PAGO MODO LECTURA],
                    PA.TARJETAHABIENTE AS [PAGO TARJETAHABIENTE],
                    PA.FECHAVENCIMIENTO AS [PAGO VENCIMIENTO],

                    PE.NUMEROTARJETAENCRIPTADO AS [PAGO TARJETA ENCRIPTADA],
                    PE.VALORINTERES AS [PAGO VALOR INTERES],
                    PE.MENSAJEPUBLICIDAD AS [PAGO PUBLICIDAD],
                    PE.APLICACIONEMV AS [EMV APLICACION],
                    PE.AID AS [EMV AID],
                    PE.CRIPTOGRAMA AS [EMV CRIPTOGRAMA],
                    PE.VERIFICACIONPIN AS [EMV VERIFICACION PIN],
                    PE.ARQC AS [EMV ARQC],
                    PE.TVR AS [EMV TVR],
                    PE.TSI AS [EMV TSI],
                    PE.FECHAREGISTRO AS [PAGO FECHA DETALLE],

                    PAN.REDADQUIRENTE AS [ANULACION RED ADQUIRENTE],

                    T.DIRECCION AS [TRAMA DIRECCION],
                    T.TRAMAHEX AS [TRAMA CONTENIDO],
                    T.FECHAHORA AS [TRAMA FECHA],

                    E.FECHAHORA AS [EVENTO FECHA],
                    E.TIPOEVENTO AS [EVENTO TIPO],
                    E.ORIGEN AS [EVENTO ORIGEN],
                    E.MENSAJE AS [EVENTO MENSAJE]
                FROM (((((PINPAD_LOG AS L
                    LEFT JOIN PINPAD_AUTORIZADAS AS PA
                        ON L.TRANSACCIONID = PA.TRANSACCIONID)
                    LEFT JOIN PINPAD_PAGO_EXTENDIDO AS PE
                        ON L.TRANSACCIONID = PE.TRANSACCIONID)
                    LEFT JOIN PINPAD_ANULACIONES AS PAN
                        ON L.TRANSACCIONID = PAN.TRANSACCIONID)
                    LEFT JOIN PINPAD_TRAMAS AS T
                        ON L.TRANSACCIONID = T.TRANSACCIONID)
                    LEFT JOIN PINPAD_EVENTOS AS E
                        ON L.TRANSACCIONID = E.TRANSACCIONID)
                WHERE L.FECHAINICIO >= @desde
                  AND L.FECHAINICIO < @hasta";

            var parametros = new List<(string nombre, object valor)>
            {
                ("desde", fechaDesde.Date),
                ("hasta", fechaHasta.Date.AddDays(1))
            };

            if (!string.IsNullOrWhiteSpace(numeroTarjeta))
            {
                sql += " AND L.NUMEROTARJETA LIKE @tarjeta";
                parametros.Add(("tarjeta", "%" + numeroTarjeta.Trim() + "%"));
            }

            if (!string.IsNullOrWhiteSpace(numeroFactura))
            {
                sql += " AND L.NUMEROFACTURA LIKE @factura";
                parametros.Add(("factura", "%" + numeroFactura.Trim() + "%"));
            }

            // Se busca en la cabecera Y en el detalle del cobro: las filas anteriores a que
            // PINPAD_LOG guardara AUTORIZACION/REFERENCIA solo tienen el dato en
            // PINPAD_AUTORIZADAS. Access liga los parámetros por POSICIÓN, así que el mismo
            // valor se agrega dos veces (un @ por cada aparición en el SQL).
            if (!string.IsNullOrWhiteSpace(autorizacion))
            {
                sql += " AND (L.AUTORIZACION LIKE @autorizLog OR PA.AUTORIZACION LIKE @autorizPago)";
                string patron = "%" + autorizacion.Trim() + "%";
                parametros.Add(("autorizLog", patron));
                parametros.Add(("autorizPago", patron));
            }

            if (!string.IsNullOrWhiteSpace(referencia))
            {
                sql += " AND (L.REFERENCIA LIKE @referLog OR PA.REFERENCIA LIKE @referPago)";
                string patron = "%" + referencia.Trim() + "%";
                parametros.Add(("referLog", patron));
                parametros.Add(("referPago", patron));
            }

            if (!string.IsNullOrWhiteSpace(tipoOperacion) && tipoOperacion != "Todos")
            {
                sql += " AND L.TIPOOPERACION = @tipo";
                parametros.Add(("tipo", tipoOperacion.Trim()));
            }

            sql += " ORDER BY L.FECHAFIN DESC, L.FECHAINICIO DESC, T.DIRECCION, E.FECHAHORA";

            return ConsolidarEventos(_conexion.Seleccionar(sql, parametros.ToArray()));
        }

        private static DataSet ConsolidarEventos(DataSet datos)
        {
            if (datos == null || datos.Tables.Count == 0)
                return datos;

            DataTable origen = datos.Tables[0];
            if (!origen.Columns.Contains("EVENTO MENSAJE"))
                return datos;

            DataTable salida = origen.Clone();
            salida.TableName = "PINPAD_LOG_COMPLETO";
            salida.Columns.Remove("TRAMA DIRECCION");
            salida.Columns.Remove("TRAMA CONTENIDO");
            salida.Columns.Remove("TRAMA FECHA");
            salida.Columns.Remove("EVENTO FECHA");
            salida.Columns.Remove("EVENTO TIPO");
            salida.Columns.Remove("EVENTO ORIGEN");
            salida.Columns.Remove("EVENTO MENSAJE");
            salida.Columns.Add("TRAMA ENVIADA", typeof(string));
            salida.Columns.Add("TRAMA ENVIADA FECHA", typeof(DateTime));
            salida.Columns.Add("TRAMA RESPUESTA", typeof(string));
            salida.Columns.Add("TRAMA RESPUESTA FECHA", typeof(DateTime));
            salida.Columns.Add("EVENTOS", typeof(string));

            var filasPorTransaccion = new Dictionary<string, DataRow>();
            var eventosPorTransaccion = new Dictionary<string, StringBuilder>();
            var firmasEventos = new Dictionary<string, HashSet<string>>();

            foreach (DataRow fila in origen.Rows)
            {
                string transaccionId = Texto(fila, "TRANSACCION ID");
                string clave = string.IsNullOrWhiteSpace(transaccionId)
                    ? "LEGACY-" + Texto(fila, "FECHA INICIO") + "|" +
                        Texto(fila, "TIPO OPERACION") + "|" + Texto(fila, "NUMERO FACTURA")
                    : transaccionId;

                if (!filasPorTransaccion.TryGetValue(clave, out DataRow consolidada))
                {
                    consolidada = salida.NewRow();
                    foreach (DataColumn columna in salida.Columns)
                    {
                        if (columna.ColumnName != "EVENTOS" &&
                            columna.ColumnName != "TRAMA ENVIADA" &&
                            columna.ColumnName != "TRAMA ENVIADA FECHA" &&
                            columna.ColumnName != "TRAMA RESPUESTA" &&
                            columna.ColumnName != "TRAMA RESPUESTA FECHA")
                        {
                            consolidada[columna.ColumnName] = fila[columna.ColumnName];
                        }
                    }

                    salida.Rows.Add(consolidada);
                    filasPorTransaccion[clave] = consolidada;
                    eventosPorTransaccion[clave] = new StringBuilder();
                    firmasEventos[clave] = new HashSet<string>();
                }

                string direccionTrama = Texto(fila, "TRAMA DIRECCION").ToUpperInvariant();
                if (direccionTrama == "ENVIADA")
                {
                    consolidada["TRAMA ENVIADA"] = fila["TRAMA CONTENIDO"];
                    consolidada["TRAMA ENVIADA FECHA"] = fila["TRAMA FECHA"];
                }
                else if (direccionTrama == "RESPUESTA")
                {
                    consolidada["TRAMA RESPUESTA"] = fila["TRAMA CONTENIDO"];
                    consolidada["TRAMA RESPUESTA FECHA"] = fila["TRAMA FECHA"];
                }

                string mensajeEvento = Texto(fila, "EVENTO MENSAJE");
                string tipoEvento = Texto(fila, "EVENTO TIPO");
                string origenEvento = Texto(fila, "EVENTO ORIGEN");
                if (mensajeEvento.Length == 0 && tipoEvento.Length == 0 && origenEvento.Length == 0)
                    continue;

                string firmaEvento = Texto(fila, "EVENTO FECHA") + "|" +
                    tipoEvento + "|" + origenEvento + "|" + mensajeEvento;
                if (!firmasEventos[clave].Add(firmaEvento))
                    continue;

                StringBuilder eventos = eventosPorTransaccion[clave];
                if (eventos.Length > 0)
                    eventos.AppendLine();

                if (fila["EVENTO FECHA"] != DBNull.Value)
                {
                    eventos.Append('[')
                        .Append(Convert.ToDateTime(fila["EVENTO FECHA"]).ToString("yyyy-MM-dd HH:mm:ss"))
                        .Append("] ");
                }

                if (tipoEvento.Length > 0)
                    eventos.Append(tipoEvento);
                if (origenEvento.Length > 0)
                    eventos.Append(" | ").Append(origenEvento);
                if (mensajeEvento.Length > 0)
                    eventos.Append(" | ").Append(mensajeEvento);

                consolidada["EVENTOS"] = eventos.ToString();
            }

            var resultado = new DataSet();
            resultado.Tables.Add(salida);
            return resultado;
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

        /// <summary>
        /// Valores distintos de una columna de PINPAD_LOG para las sugerencias de la
        /// pantalla de consulta. `columna` SIEMPRE es un literal de los métodos de abajo,
        /// nunca entrada del usuario; lo que el usuario escribe va parametrizado en @filtro.
        /// </summary>
        private DataSet ConsultarDistintos(string columna, string filtro)
        {
            string sql = "SELECT DISTINCT TOP 20 " + columna +
                         " FROM PINPAD_LOG" +
                         " WHERE " + columna + " IS NOT NULL" +
                         "   AND " + columna + " <> ''";

            if (string.IsNullOrWhiteSpace(filtro))
                return _conexion.Seleccionar(sql + " ORDER BY " + columna);

            sql += " AND " + columna + " LIKE @filtro ORDER BY " + columna;
            return _conexion.Seleccionar(sql, ("filtro", "%" + filtro.Trim() + "%"));
        }

        public DataSet ConsultarTarjetas(string numeroTarjeta)
        {
            return ConsultarDistintos("NUMEROTARJETA", numeroTarjeta);
        }

        public DataSet ConsultarFacturas(string numeroFactura)
        {
            return ConsultarDistintos("NUMEROFACTURA", numeroFactura);
        }

        public DataSet ConsultarAutorizaciones(string autorizacion)
        {
            return ConsultarDistintos("AUTORIZACION", autorizacion);
        }

        public DataSet ConsultarReferencias(string referencia)
        {
            return ConsultarDistintos("REFERENCIA", referencia);
        }

        private static string Texto(DataRow fila, string columna)
        {
            if (!fila.Table.Columns.Contains(columna) || fila[columna] == DBNull.Value)
                return "";

            return fila[columna].ToString().Trim();
        }

        /// <summary>
        /// Registra todos los datos devueltos por ConsultaTarjeta/LecturaTarjeta y conserva
        /// el número visible en la cabecera para facilitar la búsqueda desde la pantalla.
        /// </summary>
        public void GuardarDetalleTarjeta(long transaccionId,
            string numeroTarjeta, string numeroTarjetaEncriptado, string binTarjeta,
            string fechaVencimiento, string redAdquirienteCorriente, string redAdquirienteDiferido)
        {
            string transaccionDb = TransaccionDe(transaccionId);
            if (transaccionDb == null) return;

            if (!string.IsNullOrWhiteSpace(numeroTarjeta))
            {
                _datosPorToken.AddOrUpdate(transaccionId,
                    (numeroTarjeta, null, null),
                    (t, previo) => (numeroTarjeta, previo.Autorizacion, previo.Referencia));
            }

            try
            {
                string sql = @"INSERT INTO PINPAD_DETALLE_TARJETA
                    (TRANSACCIONID, NUMEROTARJETA, NUMEROTARJETAENCRIPTADO,
                     BINTARJETA, FECHAVENCIMIENTO, REDADQUIRIENTECORRIENTE,
                     REDADQUIRIENTEDIFERIDO, FECHAREGISTRO)
                    VALUES
                    (@transId, @tarjeta, @tarjetaEnc,
                     @bin, @vence, @redCorriente,
                     @redDiferido, @fecha)";

                _conexion.Ejecutar(sql,
                    ("transId", transaccionDb),
                    ("tarjeta", numeroTarjeta ?? ""),
                    ("tarjetaEnc", numeroTarjetaEncriptado ?? ""),
                    ("bin", binTarjeta ?? ""),
                    ("vence", fechaVencimiento ?? ""),
                    ("redCorriente", redAdquirienteCorriente ?? ""),
                    ("redDiferido", redAdquirienteDiferido ?? ""),
                    ("fecha", DateTime.Now));
            }
            catch { /* la auditoría no bloquea la operación */ }
        }

        // =====================================================================
        // DETALLES TÉCNICOS, TRAMAS Y EVENTOS
        // =====================================================================

        public void GuardarDetalleConfigRed(long transaccionId, ConfiguracionRedRequest request)
        {
            string transaccionDb = TransaccionDe(transaccionId);
            if (transaccionDb == null || request == null) return;

            try
            {
                string sql = @"INSERT INTO PINPAD_DETALLE_CONFIG_RED
                    (TRANSACCIONID, DIRECCIONIP, MASCARA, GATEWAY,
                     PRINCIPALHOST, PRINCIPALPUERTO, ALTERNOHOST,
                     ALTERNOPUERTO, PUERTOESCUCHA, FECHAREGISTRO)
                    VALUES
                    (@transId, @ip, @mascara, @gateway,
                     @hostPrincipal, @puertoPrincipal, @hostAlterno,
                     @puertoAlterno, @puertoEscucha, @fecha)";

                _conexion.Ejecutar(sql,
                    ("transId", transaccionDb),
                    ("ip", request.DireccionIP ?? ""),
                    ("mascara", request.Mascara ?? ""),
                    ("gateway", request.Gateway ?? ""),
                    ("hostPrincipal", request.PrincipalHost ?? ""),
                    ("puertoPrincipal", request.PrincipalPuerto ?? ""),
                    ("hostAlterno", request.AlternoHost ?? ""),
                    ("puertoAlterno", request.AlternoPuerto ?? ""),
                    ("puertoEscucha", request.PuertoEscucha ?? ""),
                    ("fecha", DateTime.Now));
            }
            catch { /* la auditoría no bloquea la configuración */ }
        }

        public void GuardarDetalleProcesoControl(long transaccionId, string lote, string referencia)
        {
            string transaccionDb = TransaccionDe(transaccionId);
            if (transaccionDb == null) return;

            try
            {
                string sql = @"INSERT INTO PINPAD_DETALLE_PROCESO_CONTROL
                    (TRANSACCIONID, LOTE, REFERENCIA, FECHAREGISTRO)
                    VALUES (@transId, @lote, @referencia, @fecha)";

                _conexion.Ejecutar(sql,
                    ("transId", transaccionDb),
                    ("lote", lote ?? ""),
                    ("referencia", referencia ?? ""),
                    ("fecha", DateTime.Now));
            }
            catch { /* la auditoría no bloquea el proceso */ }
        }

        public void GuardarTrama(long transaccionId, string direccion, string tramaHex)
        {
            string transaccionDb = TransaccionDe(transaccionId);
            if (transaccionDb == null || string.IsNullOrWhiteSpace(direccion)) return;

            try
            {
                string sql = @"INSERT INTO PINPAD_TRAMAS
                    (TRANSACCIONID, DIRECCION, TRAMAHEX, FECHAHORA)
                    VALUES (@transId, @direccion, @trama, @fecha)";

                _conexion.Ejecutar(sql,
                    ("transId", transaccionDb),
                    ("direccion", direccion.Trim()),
                    ("trama", tramaHex ?? ""),
                    ("fecha", DateTime.Now));
            }
            catch { /* la auditoría no bloquea la operación */ }
        }

        public void RegistrarEvento(string tipoEvento, string origen, string mensaje, long? transaccionId = null)
        {
            string transaccionDb = transaccionId.HasValue
                ? TransaccionDe(transaccionId.Value)
                : null;

            try
            {
                string sql = @"INSERT INTO PINPAD_EVENTOS
                    (TRANSACCIONID, FECHAHORA, TIPOEVENTO, ORIGEN, MENSAJE)
                    VALUES (@transId, @fecha, @tipo, @origen, @mensaje)";

                _conexion.Ejecutar(sql,
                    ("transId", (object)transaccionDb ?? DBNull.Value),
                    ("fecha", DateTime.Now),
                    ("tipo", tipoEvento ?? ""),
                    ("origen", origen ?? ""),
                    ("mensaje", mensaje ?? ""));
            }
            catch { /* la auditoría nunca debe tumbar la operación */ }
        }
    }
}
