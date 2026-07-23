using System;
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
    /// REGLA CRÍTICA: todos los métodos TRAGAN sus propias excepciones. El wrapper
    /// (PinPadService) llama a GuardarDetallePago/FinalizarTransaccion DESPUÉS de que la
    /// tarjeta ya fue cobrada; si el logger relanzara, se perdería el resultado de un cobro
    /// real. Un fallo de auditoría nunca debe tumbar la transacción.
    ///
    /// Access guarda True como -1; no filtramos por booleanos aquí, así que basta con pasar
    /// el bool nativo al parámetro (OLEDB lo persiste como -1/0).
    /// </summary>
    public class PinPadLogManejador : ISqlLogger
    {
        private readonly IConexionBD _conexion;

        public PinPadLogManejador(IConexionBD conexion)
        {
            _conexion = conexion;
        }

        // =====================================================================
        // CABECERA — PINPAD_LOG
        // =====================================================================

        public long IniciarTransaccion(string tipoOperacion, string usuarioSistema, string cajaId = null, string numeroFactura = null)
        {
            try
            {
                long nuevoId = SiguienteId();

                string sql = @"INSERT INTO PINPAD_LOG
                    (ID, TIPO_OPERACION, USUARIO_SISTEMA, CAJA_ID, NUMERO_FACTURA, FECHA_INICIO)
                    VALUES (@id, @tipo, @usuario, @caja, @factura, @fecha)";

                _conexion.Ejecutar(sql,
                    ("id", nuevoId),
                    ("tipo", tipoOperacion ?? ""),
                    ("usuario", usuarioSistema ?? ""),
                    ("caja", cajaId ?? ""),
                    ("factura", numeroFactura ?? ""),
                    ("fecha", DateTime.Now));

                return nuevoId;
            }
            catch
            {
                // Nunca bloquear el cobro por un fallo de auditoría. Se devuelve un Id
                // "sin persistir" (negativo con marca de tiempo) para que los métodos
                // siguientes sigan operando sin excepción.
                return -DateTime.Now.Ticks;
            }
        }

        public void FinalizarTransaccion(long transaccionId, string codigoRespuesta, string mensajeRespuesta,
            bool exitoso, string excepcionMensaje)
        {
            try
            {
                string sql = @"UPDATE PINPAD_LOG SET
                    CODIGO_RESPUESTA = @cod,
                    MENSAJE_RESPUESTA = @msg,
                    EXITOSO = @exito,
                    EXCEPCION_MENSAJE = @exc,
                    FECHA_FIN = @fin
                    WHERE ID = @id";

                _conexion.Ejecutar(sql,
                    ("cod", codigoRespuesta ?? ""),
                    ("msg", mensajeRespuesta ?? ""),
                    ("exito", exitoso),
                    ("exc", excepcionMensaje ?? ""),
                    ("fin", DateTime.Now),
                    ("id", transaccionId));
            }
            catch { /* swallow: auditoría no bloquea el cobro */ }
        }

        public void VincularNumeroFactura(long transaccionId, string numeroFactura)
        {
            try
            {
                string sql = @"UPDATE PINPAD_LOG SET NUMERO_FACTURA = @factura WHERE ID = @id";
                _conexion.Ejecutar(sql,
                    ("factura", numeroFactura ?? ""),
                    ("id", transaccionId));
            }
            catch { /* swallow */ }
        }

        // =====================================================================
        // DETALLE COBRO — PINPAD_AUTORIZADAS
        // =====================================================================

        public void GuardarDetallePago(long transaccionId, ProcesoPagoRequest request, ProcesoPagoResult result)
        {
            try
            {
                decimal baseImponible = request?.BaseImponible ?? (request?.Monto ?? 0m);

                string sql = @"INSERT INTO PINPAD_AUTORIZADAS
                    (TRANSACCION_ID, MONTO, BASE_IMPONIBLE, BASE0, IVA,
                     TIPO_TRANSACCION, RED, TIPO_CREDITO, EXITOSO,
                     CODIGO_RESPUESTA_AUT, MENSAJE_RESPUESTA_AUT,
                     AUTORIZACION, REFERENCIA, LOTE, RED_ADQUIRENTE,
                     NOMBRE_GRUPO_TARJETA, TARJETA_HABIENTE, NUMERO_TARJETA,
                     MODO_LECTURA, FECHA, HORA, TID, MID)
                    VALUES
                    (@transId, @monto, @baseImp, @base0, @iva,
                     @tipoTrans, @red, @tipoCred, @exito,
                     @codAut, @msgAut,
                     @autoriz, @refer, @lote, @redAdq,
                     @grupo, @habiente, @tarjeta,
                     @modo, @fecha, @hora, @tid, @mid)";

                _conexion.Ejecutar(sql,
                    ("transId", transaccionId),
                    ("monto", request?.Monto ?? 0m),
                    ("baseImp", baseImponible),
                    ("base0", request?.Base0 ?? 0m),
                    ("iva", request?.IVA ?? 0m),
                    ("tipoTrans", request?.TipoTransaccion ?? ""),
                    ("red", request?.Red ?? ""),
                    ("tipoCred", request?.TipoCredito ?? ""),
                    ("exito", result != null && result.Exitoso),
                    ("codAut", result?.CodigoRespuestaAut ?? ""),
                    ("msgAut", result?.MensajeRespuestaAut ?? ""),
                    ("autoriz", result?.Autorizacion ?? ""),
                    ("refer", result?.Referencia ?? ""),
                    ("lote", result?.Lote ?? ""),
                    ("redAdq", result?.RedAdquirente ?? ""),
                    ("grupo", result?.NombreGrupoTarjeta ?? ""),
                    ("habiente", result?.TarjetaHabiente ?? ""),
                    ("tarjeta", result?.NumeroTarjeta ?? ""),
                    ("modo", result?.ModoLectura ?? ""),
                    ("fecha", result?.Fecha ?? ""),
                    ("hora", result?.Hora ?? ""),
                    ("tid", result?.TID ?? ""),
                    ("mid", result?.MID ?? ""));
            }
            catch { /* swallow */ }
        }

        // =====================================================================
        // DETALLE ANULACIÓN — PINPAD_ANULACIONES
        // =====================================================================

        public void GuardarDetalleAnulacion(long transaccionId, string referenciaOriginal, string autorizacionOriginal, string redAdquirente)
        {
            try
            {
                string sql = @"INSERT INTO PINPAD_ANULACIONES
                    (TRANSACCION_ID, REFERENCIA_ORIGINAL, AUTORIZACION_ORIGINAL, RED_ADQUIRENTE)
                    VALUES (@transId, @refer, @autoriz, @red)";

                _conexion.Ejecutar(sql,
                    ("transId", transaccionId),
                    ("refer", referenciaOriginal ?? ""),
                    ("autoriz", autorizacionOriginal ?? ""),
                    ("red", redAdquirente ?? ""));
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

        // =====================================================================
        // Id autoasignado: Access abre/cierra por llamada, así que @@IDENTITY no es
        // fiable entre llamadas -> MAX(ID)+1. NZ() protege la tabla vacía.
        // =====================================================================
        private long SiguienteId()
        {
            var ds = _conexion.Seleccionar("SELECT NZ(MAX(ID),0)+1 AS NUEVO FROM PINPAD_LOG");
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                return Convert.ToInt64(ds.Tables[0].Rows[0]["NUEVO"]);
            return 1;
        }
    }
}
