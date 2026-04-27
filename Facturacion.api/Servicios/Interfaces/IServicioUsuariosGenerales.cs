using Facturacion.api.Models.DTOs;
using Facturacion.api.Models.Respuestas;
using Facturacion.api.Models.Solicitudes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Facturacion.api.Servicios.Interfaces
{
    public interface IServicioUsuariosGenerales
    {
        Task<RespuestaGeneral<List<DtoUsuarioGeneral>>> ObtenerTodosAsync();
        Task<RespuestaGeneral<DtoUsuarioGeneral>> ObtenerPorIdAsync(int id);
        Task<RespuestaGeneral<DtoUsuarioGeneral>> VerificarAdminAsync(SolicitudLoginAdmin solicitud);
        Task<RespuestaGeneral<string>> CrearAsync(SolicitudUsuarioGeneral solicitud);
        Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudUsuarioGeneral solicitud);
        Task<RespuestaGeneral<string>> EliminarAsync(int id);
    }
}
