using System;

namespace DF_PinPad.Wrapper.Models
{
    // =====================================================================
    // 1) CONSULTA DE CONFIGURACIÓN BÁSICA
    //    LAN.ConsultaConfiguracionBasica() -> RespuestaConfiguracionBasica
    //    (no requiere objeto de envío; usa la config ya cargada en LANConfig)
    // =====================================================================
    public class ConfiguracionBasicaResult
    {
        public bool Exitoso { get; set; }
        public string CodigoRespuesta { get; set; }
        public string MensajeRespuesta { get; set; }
        public string TramaPinPad { get; set; }
        public string ExcepcionMensaje { get; set; }
        public long TransaccionLogId { get; set; }
    }

    // =====================================================================
    // 2) CONSULTA DE TARJETA
    //    LAN.ConsultaTarjeta() -> RespuestaConsultaTarjeta
    // =====================================================================
    public class ConsultaTarjetaResult
    {
        public bool Exitoso { get; set; }
        public string CodigoRespuesta { get; set; }
        public string MensajeRespuesta { get; set; }
        public string NumeroTarjeta { get; set; }
        public string BinTarjeta { get; set; }
        public string FechaVencimiento { get; set; }
        public string TramaPinPad { get; set; }
        public string ExcepcionMensaje { get; set; }
        public long TransaccionLogId { get; set; }
    }

    // =====================================================================
    // 3) LECTURA DE TARJETA
    //    LAN.LecturaTarjeta() -> RespuestaLecturaTarjeta
    // =====================================================================
    public class LecturaTarjetaResult
    {
        public bool Exitoso { get; set; }
        public string CodigoRespuesta { get; set; }
        public string MensajeRespuesta { get; set; }
        public string RedAdquirienteCorriente { get; set; }
        public string RedAdquirienteDiferido { get; set; }
        public string NumeroTarjeta { get; set; }
        public string NumeroTarjetaEncriptado { get; set; }
        public string BinTarjeta { get; set; }
        public string FechaVencimiento { get; set; }
        public string TramaPinPad { get; set; }
        public string ExcepcionMensaje { get; set; }
        public long TransaccionLogId { get; set; }
    }

    // =====================================================================
    // 4) PROCESO DE PAGO
    //    LAN.ProcesoPago(EnvioProcesoPago) -> RespuestaProcesoPago
    // =====================================================================
    public class ProcesoPagoRequest
    {
        /// <summary>
        /// Número de factura del POS (o su valor previsto/secuencial, si se cobra
        /// antes de que el backend confirme el número final). Se usa como llave de
        /// correlación en la auditoría, no como identificador interno (ver Id
        /// autoincremental de PinPad_Transacciones). Si más tarde el número final
        /// difiere (ej. reintento por secuencial repetido en el SRI), corregir con
        /// IPinPadService.VincularNumeroFactura(logIdSql, numeroFinal).
        /// </summary>
        public string NumeroFactura { get; set; }

        /// <summary>Monto total de la transacción, en dólares (ej. 10.50).</summary>
        public decimal Monto { get; set; }

        /// <summary>Base imponible (grabada con IVA). Si no se especifica, se asume igual a Monto.</summary>
        public decimal? BaseImponible { get; set; }

        /// <summary>Base 0% (no grabada). Por defecto 0.</summary>
        public decimal Base0 { get; set; }

        /// <summary>IVA calculado. Si no se especifica, el servicio no lo calcula automáticamente.</summary>
        public decimal IVA { get; set; }

        public decimal Servicio { get; set; }
        public decimal Propina { get; set; }
        public decimal MontoFijo { get; set; }

        /// <summary>"Corriente" o "Diferido" (determina el valor real 1/2 enviado a la DLL).</summary>
        public string TipoTransaccion { get; set; } = "Corriente";

        /// <summary>"Datafast", "Medianet" o "Austro" (determina el valor real 1/2/3 enviado a la DLL).</summary>
        public string Red { get; set; } = "Datafast";

        /// <summary>
        /// Solo aplica si TipoTransaccion="Diferido". Valores válidos (DF_PinPad.TipoCredito):
        /// "DiferidoCorriente", "DiferidoConIntereses", "DiferidoSinIntereses",
        /// "DiferidoConInteresesConMesesDeGracia", "DiferidoSinInteresesConMesesDeGracia",
        /// "DiferidoPlus", "DiferidoPlusCuotas". Si es null/vacío y TipoTransaccion="Diferido",
        /// se usa "DiferidoCorriente" por defecto.
        /// </summary>
        public string TipoCredito { get; set; }

        /// <summary>
        /// Opcional — solo para pagos con billetera OTT. Valores válidos (DF_PinPad.TipoBilletera):
        /// "OTTDiners" u "OTTPacifico". Dejar null/vacío para pagos normales con tarjeta física.
        /// </summary>
        public string Billetera { get; set; }

        public int? PlazoDiferido { get; set; }
        public int? MesesGracia { get; set; }

        public string Referencia { get; set; }
        public string UsuarioSistema { get; set; }
    }

    public class ProcesoPagoResult
    {
        public bool Exitoso { get; set; }

        public string CodigoRespuesta { get; set; }
        public string CodigoRespuestaAut { get; set; }
        public string MensajeRespuestaAut { get; set; }

        public string RedAdquirente { get; set; }
        public string Referencia { get; set; }
        public string Lote { get; set; }
        public string Hora { get; set; }
        public string Fecha { get; set; }
        public string Autorizacion { get; set; }
        public string TID { get; set; }
        public string MID { get; set; }

        public string CodigoAdquirente { get; set; }
        public string NombreAdquirente { get; set; }
        public string NombreGrupoTarjeta { get; set; }
        public string ModoLectura { get; set; }
        public string TarjetaHabiente { get; set; }

        public string NumeroTarjeta { get; set; }
        public string FechaVencimiento { get; set; }
        public string NumeroTarjetaEncriptado { get; set; }
        public string TarjetaTruncadaOTT { get; set; }
        public string TramaPinPad { get; set; }

        /// <summary>"Valor del interés o financiamiento" — solo aplica a diferidos con interés.</summary>
        public string ValorInteres { get; set; }
        /// <summary>"Mensaje para impresión de premios o publicidad" del emisor.</summary>
        public string MensajePublicidad { get; set; }

        // Los siguientes 6 campos SOLO aplican cuando ModoLectura="03" (Chip) — el resto
        // de los casos vienen en blanco (así lo documenta la especificación oficial).
        public string AplicacionEMV { get; set; }
        public string AID { get; set; }
        public string Criptograma { get; set; }
        public string VerificacionPIN { get; set; }
        public string ARQC { get; set; }
        public string TVR { get; set; }
        public string TSI { get; set; }

        public string TramaEnviadaHex { get; set; }
        public string TramaRespuestaHex { get; set; }

        public string ExcepcionMensaje { get; set; }

        /// <summary>Id generado en dbo.PinPad_Transacciones.</summary>
        public long TransaccionLogId { get; set; }
    }

    // =====================================================================
    // 5) ANULACIÓN
    //    LAN.ProcesoControl(EnvioProcesoControl) -> RespuestaProcesoControl
    //    (EnvioProcesoControl identifica la transacción original por Lote + Referencia)
    // =====================================================================
    public class AnulacionRequest
    {
        /// <summary>Número de la factura cuyo cobro se está anulando. Se usa únicamente
        /// para correlacionar la auditoría; no se envía en la trama al pinpad.</summary>
        public string NumeroFactura { get; set; }

        /// <summary>"Datafast", "Medianet" o "Austro".</summary>
        public string Red { get; set; } = "Datafast";

        /// <summary>Código de autorización de la transacción original a anular.</summary>
        public string Autorizacion { get; set; }

        /// <summary>
        /// Referencia de la transacción original a anular. IMPORTANTE (corregido): a
        /// diferencia de lo que se pensó antes, en Anulación esta SÍ se usa con su valor
        /// real (numérico, máx. 6 dígitos, rellenado con ceros) — confirmado comparando
        /// una trama real de anulación exitosa. No es un campo decorativo.
        /// </summary>
        public string Referencia { get; set; }

        public string UsuarioSistema { get; set; }
    }

    public class AnulacionResult
    {
        public bool Exitoso { get; set; }
        public string CodigoRespuesta { get; set; }
        public string CodigoRespuestaAut { get; set; }
        public string MensajeRespuesta { get; set; }
        public string Lote { get; set; }
        public string Referencia { get; set; }
        public string Autorizacion { get; set; }
        public string RedAdquirente { get; set; }
        public string CodigoAdquirente { get; set; }
        public string NombreAdquirente { get; set; }
        public string TarjetaHabiente { get; set; }
        public string NumeroTarjeta { get; set; }
        public string FechaVencimiento { get; set; }
        public string NombreGrupoTarjeta { get; set; }
        public string TramaPinPad { get; set; }
        public string ExcepcionMensaje { get; set; }
        public long TransaccionLogId { get; set; }
    }

    // =====================================================================
    // 6) PROCESO DE CONFIGURACIÓN DE RED DEL PIN PAD
    //    LAN.ProcesoConfigPinPad(EnvioProcesoConfigPinPad) -> RespuestaProcesoConfigPinPad
    //    (configura los parámetros de RED del dispositivo físico: IP, máscara, gateway,
    //     host/puerto principal y alterno, puerto de escucha — igual a la pestaña "CP"
    //     del simulador PinPad .NET)
    // =====================================================================
    public class ConfiguracionRedRequest
    {
        public string DireccionIP { get; set; }
        public string Mascara { get; set; }
        public string Gateway { get; set; }
        public string PrincipalHost { get; set; }
        public string PrincipalPuerto { get; set; }
        public string AlternoHost { get; set; }
        public string AlternoPuerto { get; set; }
        public string PuertoEscucha { get; set; }
        public string UsuarioSistema { get; set; }
    }

    public class ConfiguracionRedResult
    {
        public bool Exitoso { get; set; }
        public string CodigoRespuesta { get; set; }
        public string MensajeRespuesta { get; set; }
        public string ExcepcionMensaje { get; set; }
        public long TransaccionLogId { get; set; }
    }

    // =====================================================================
    // 7) PROCESO DE CONTROL ("PC" en el simulador)
    //    LAN.ProcesoControl(EnvioProcesoControl) -> RespuestaProcesoControl
    //    Identifica la transacción por Lote + Referencia (NO tiene RedAdquirente ni
    //    Autorizacion — esos campos solo existen en EnvioProcesoPago).
    // =====================================================================
    public class ProcesoControlRequest
    {
        /// <summary>Número de lote de la transacción (numérico, máx. 6 dígitos).</summary>
        public string Lote { get; set; }

        /// <summary>Referencia de la transacción (numérica, máx. 6 dígitos).</summary>
        public string Referencia { get; set; }

        /// <summary>
        /// MID a usar en esta operación puntual. Si se deja vacío, se usa el MID
        /// configurado globalmente en la pestaña "0. Conexión".
        /// </summary>
        public string MID { get; set; }

        /// <summary>TID a usar en esta operación puntual (vacío = usar el de Conexión).</summary>
        public string TID { get; set; }

        /// <summary>CajaID a usar en esta operación puntual (vacío = usar el de Conexión).</summary>
        public string CajaID { get; set; }

        public string UsuarioSistema { get; set; }
    }

    public class ProcesoControlResult
    {
        public bool Exitoso { get; set; }
        public string CodigoRespuesta { get; set; }
        public string MensajeRespuesta { get; set; }
        public string TramaPinPad { get; set; }
        public string ExcepcionMensaje { get; set; }
        public long TransaccionLogId { get; set; }
    }
}
