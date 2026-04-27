using System.Collections.Generic;
using System.Threading.Tasks;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Models.DTOs;
using Facturacion.api.Models.Respuestas;

namespace Facturacion.api.Servicios
{
    public interface IServicioCliente
    {
        Task<RespuestaGeneral<List<DtoCliente>>> ObtenerTodosAsync();

        Task<RespuestaGeneral<DtoCliente>> ObtenerPorCedulaAsync(string cedula);

        Task<RespuestaGeneral<string>> CrearAsync(SolicitudCliente solicitud);

        Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudCliente solicitud);

        Task<RespuestaGeneral<string>> EliminarAsync(
            string cedula,
            string nombre,
            string usuario,
            string ip
        );
    }
}