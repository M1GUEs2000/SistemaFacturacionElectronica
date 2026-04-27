using Facturacion.api.Models.Solicitudes;
using Facturacion.api.DTOs;
using Facturacion.api.Models.Respuestas;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Facturacion.api.Servicios
{
    public interface IServicioProveedores
    {
        Task<RespuestaGeneral<List<DtoProveedor>>> ListarAsync();

        Task<RespuestaGeneral<List<DtoProveedor>>> ListarActivosAsync();

        Task<RespuestaGeneral<DtoProveedor>> ConsultarPorIdAsync(int idProveedor);

        Task<RespuestaGeneral<DtoProveedor>> ConsultarPorIdentificacionAsync(string identificacion);

        Task<RespuestaGeneral<string>> InsertarAsync(SolicitudProveedor solicitud);

        Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudProveedor solicitud);

        Task<RespuestaGeneral<string>> EliminarAsync(int idProveedor, string usuario, string ip);
    }
}