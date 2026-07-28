using DF_PinPad.Wrapper.Models;

namespace DF_PinPad.Wrapper.Logging
{
    public interface ISqlLogger
    {
        /// <summary>
        /// Inserta el registro de cabecera (dbo.PinPad_Transacciones) al iniciar cualquier
        /// operación, y devuelve su Id para usarlo en FinalizarTransaccion/detalle/tramas.
        /// </summary>
        long IniciarTransaccion(string tipoOperacion, string usuarioSistema, string cajaId = null, string numeroFactura = null);

        /// <summary>
        /// Actualiza la cabecera con el resultado final. "codigoRespuesta" es el nivel LAN
        /// (pin pad: 00/01/02/20/ER); "codigoRespuestaAut" es el nivel autorizador/banco
        /// (00=Aprobada, 51=Sin cupo, 54=Vencida, 89=Terminal inválido, etc. — ver
        /// dbo.PinPad_CatalogoCodigosRespuesta). Son campos DISTINTOS según la especificación
        /// oficial de Datafast — no mezclarlos.
        /// </summary>
        void FinalizarTransaccion(long transaccionId, string codigoRespuesta, string mensajeRespuesta,
            bool exitoso, string excepcionMensaje, string codigoRespuestaAut = null);

        /// <summary>
        /// Vincula una transacción ya registrada con el número REAL de factura del POS,
        /// una vez que el backend de facturación lo confirma (llamar DESPUÉS de que la
        /// factura ya fue creada exitosamente, no antes — el número previo/estimado del
        /// secuencial puede no coincidir con el finalmente asignado).
        /// </summary>
        void VincularNumeroFactura(long transaccionId, string numeroFactura);

        void GuardarDetallePago(long transaccionId, ProcesoPagoRequest request, ProcesoPagoResult result);

        void GuardarDetalleTarjeta(long transaccionId,
            string numeroTarjeta, string numeroTarjetaEncriptado, string binTarjeta,
            string fechaVencimiento, string redAdquirienteCorriente, string redAdquirienteDiferido);

        void GuardarDetalleAnulacion(long transaccionId, string referenciaOriginal, string autorizacionOriginal, string redAdquirente);

        void GuardarDetalleConfigRed(long transaccionId, ConfiguracionRedRequest request);

        void GuardarDetalleProcesoControl(long transaccionId, string lote, string referencia);

        void GuardarTrama(long transaccionId, string direccion, string tramaHex);

        void RegistrarEvento(string tipoEvento, string origen, string mensaje, long? transaccionId = null);
    }
}
