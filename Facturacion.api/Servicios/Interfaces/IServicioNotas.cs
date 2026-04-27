using Facturacion.api.Models.Solicitudes;
using System.Threading.Tasks;
using Facturacion.api.Models.Respuestas;

namespace Facturacion.api.Servicios.Interfaces
{
    public interface IServicioNota
    {
        Task<RespuestaNota> CrearNotaAsync(
            SolicitudNota solicitud
        );

        Task<RespuestaNota> ProcesarNotaDesdeAutorizacionAsync(
            SolicitudProcesarPendienteDocumento solicitud
        );

        Task<RespuestaNota> PreviewNotaAsync(
            SolicitudNota solicitud
        );
    }
}