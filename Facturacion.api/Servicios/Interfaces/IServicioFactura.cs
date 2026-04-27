using Facturacion.api.Models.Solicitudes;
using System.Threading.Tasks;
using Facturacion.api.Models.Respuestas;

public interface IServicioFactura
{
    Task<RespuestaFactura> CrearFacturaAsync(SolicitudFactura solicitud);

    Task<RespuestaFactura> AutorizarFacturaPendienteAsync(SolicitudProcesarPendienteDocumento solicitud);

    Task<RespuestaFactura> PreviewFacturaAsync(SolicitudFactura solicitud);

    Task<RespuestaFactura> ProcesarFacturaConsumidorFinalAsync(SolicitudProcesarFacturaConsumidorFinal solicitud);

    Task<RespuestaFactura> ReprocesarFacturaPendienteAsync(SolicitudProcesarFacturaConsumidorFinal solicitud);
}