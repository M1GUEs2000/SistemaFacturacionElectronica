using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Models.DTOs;
using Facturacion.api.Models.Respuestas;
using System.Threading.Tasks;

namespace Facturacion.api.Servicios
{
    public interface IServicioLogin
    {
        Task<RespuestaGeneral<DtoLogin>> LoginAsync(string clave);

        Task<RespuestaGeneral<DtoLogin>> LoginConUsuarioAsync(SolicitudLogin solicitud);

        Task<RespuestaGeneral<string>> ValidarClaveEliminarAsync(string clave);

        Task<RespuestaGeneral<string>> ValidarClaveTotalesAsync(string clave);

        Task<RespuestaGeneral<string>> ValidarClaveConsultaAsync(string clave);

        Task<RespuestaGeneral<string>> ValidarClaveTablasAsync(string clave);
    }
}