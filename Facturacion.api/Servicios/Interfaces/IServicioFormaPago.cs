using Facturacion.api.Models.Solicitudes;
using Facturacion.api.DTOs;
using Facturacion.api.Models.Respuestas;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Facturacion.api.Servicios
{
    public interface IServicioFormaPago
    {
        // LISTADOS
        Task<RespuestaGeneral<System.Collections.Generic.List<DtoFormaPago>>> MostrarAsync(string filtro);
        Task<RespuestaGeneral<DtoFormaPago>> ConsultarAsync(string formas);
        Task<RespuestaGeneral<List<DtoTotalesFormaPago>>> ConsultarTotalesAsync(string fecha);

        // COMANDOS
        Task<RespuestaGeneral<string>> InsertarAsync(SolicitudFormaPago solicitud);
        Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudFormaPago solicitud);
        Task<RespuestaGeneral<string>> EliminarAsync(string formas, string usuario, string ip);
    }
}