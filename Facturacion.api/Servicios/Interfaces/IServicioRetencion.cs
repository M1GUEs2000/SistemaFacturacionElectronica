using Facturacion.api.Models.Solicitudes;
using System.Threading.Tasks;
using Facturacion.api.Models.Respuestas;

namespace Facturacion.api.Servicios.Interfaces
{
    public interface IServicioRetencion
    {
        Task<RespuestaRetencion> CrearRetencionAsync(
            SolicitudCrearRetencion solicitud
        );

        Task<RespuestaRetencion> ProcesarRetencionDesdeAutorizacionAsync(
            SolicitudProcesarPendienteDocumento solicitud
        );

        Task<RespuestaRetencion> PreviewRetencionAsync(
            SolicitudCrearRetencion solicitud
        );
    }
}