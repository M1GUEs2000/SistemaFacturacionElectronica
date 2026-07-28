using DF_PinPad.Wrapper.Models;

namespace DF_PinPad.Wrapper.Services
{
    /// <summary>
    /// Contrato con las 5 operaciones soportadas por el pin pad, cada una
    /// registrando su propia auditoría completa en SQL Server.
    /// </summary>
    public interface IPinPadService
    {
        ConfiguracionBasicaResult ConsultarConfiguracionBasica();

        ConsultaTarjetaResult ConsultarTarjeta();

        LecturaTarjetaResult LeerTarjeta();

        ProcesoPagoResult ProcesarPago(ProcesoPagoRequest request);

        AnulacionResult Anular(AnulacionRequest request);

        ConfiguracionRedResult ConfigurarRedPinPad(ConfiguracionRedRequest request);

        ProcesoControlResult ProcesarControl(ProcesoControlRequest request);

        /// <summary>
        /// Vincula una transacción de pago ya registrada con el número real de factura
        /// del POS, una vez confirmado por el backend de facturación.
        /// </summary>
        void VincularNumeroFactura(long transaccionId, string numeroFactura);
    }
}
