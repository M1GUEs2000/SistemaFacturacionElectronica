using System;
using System.Collections.Concurrent;
using System.Threading;
using AccesoDatos.Abstractions;
using DF_PinPad.Wrapper.Logging;
using DF_PinPad.Wrapper.Models;

namespace LogicaNegocios
{
    /// <summary>
    /// Implementación de <see cref="ISqlLogger"/> respaldada en Access (la misma .accdb del POS),
    /// NO en SQL Server. Persiste la auditoría del pinpad en 3 tablas:
    ///   PINPAD_LOG          — cabecera (IniciarTransaccion/FinalizarTransaccion/VincularNumeroFactura)
    ///   PINPAD_AUTORIZADAS  — detalle del cobro (GuardarDetallePago)
    ///   PINPAD_ANULACIONES  — detalle de anulación (GuardarDetalleAnulacion)
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

        public void FinalizarTransaccion(long transaccionId, string codigoRespuesta, string mensajeRespuesta,
            bool exitoso, string excepcionMensaje)
        {
            string clave = ClaveDe(transaccionId);
            if (clave == null) return; // sin clave mapeada no hay fila que actualizar

            try
            {
                string sql = @"UPDATE PINPAD_LOG SET
                    CODIGORESPUESTA = @cod,
                    MENSAJERESPUESTA = @msg,
                    EXITOSO = @exito,
                    EXCEPCIONMENSAJE = @exc,
                    FECHAFIN = @fin
                    WHERE NUMEROFACTURA = @factura";

                _conexion.Ejecutar(sql,
                    ("cod", codigoRespuesta ?? ""),
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

            try
            {
                string sql = @"INSERT INTO PINPAD_AUTORIZADAS
                    (TRANSACCIONID, MONTO, TIPOTRANSACCION, RED,
                     CODIGORESPUESTAAUT, MENSAJERESPUESTAAUT, REDADQUIRENTE,
                     REFERENCIA, LOTE, AUTORIZACION, TID, MID,
                     CODIGOADQUIRENTE, NOMBREADQUIRENTE, NOMBREGRUPOTARJETA,
                     MODOLECTURA, TARJETAHABIENTE, NUMEROTARJETA, NUMEROFACTURA)
                    VALUES
                    (@transId, @monto, @tipoTrans, @red,
                     @codAut, @msgAut, @redAdq,
                     @refer, @lote, @autoriz, @tid, @mid,
                     @codAdq, @nomAdq, @grupo,
                     @modo, @habiente, @tarjeta, @factura)";

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
                    ("factura", clave));
            }
            catch { /* swallow */ }
        }

        // =====================================================================
        // DETALLE ANULACIÓN — PINPAD_ANULACIONES
        // =====================================================================

        public void GuardarDetalleAnulacion(long transaccionId, string referenciaOriginal, string autorizacionOriginal, string redAdquirente)
        {
            string clave = ClaveDe(transaccionId) ?? "";

            try
            {
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
            catch { /* swallow */ }
        }

        // =====================================================================
        // NO USADOS — el modelo Access es de 3 tablas. Los datos de tarjeta ya
        // quedan en PINPAD_AUTORIZADAS (via result de GuardarDetallePago).
        // =====================================================================

        public void GuardarDetalleTarjeta(long transaccionId,
            string numeroTarjeta, string numeroTarjetaEncriptado, string binTarjeta,
            string fechaVencimiento, string redAdquirienteCorriente, string redAdquirienteDiferido)
        { /* no-op */ }

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
