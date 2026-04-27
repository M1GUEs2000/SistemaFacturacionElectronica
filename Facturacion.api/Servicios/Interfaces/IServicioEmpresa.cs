using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Models.DTOs;
using Facturacion.api.Models.Respuestas;
using System.Threading.Tasks;

namespace Facturacion.api.Servicios
{
    public interface IServicioEmpresa
    {
        Task<RespuestaGeneral<System.Collections.Generic.List<DtoEmpresa>>> ObtenerTodasAsync(string nombre);

        Task<RespuestaGeneral<DtoEmpresa>> ObtenerPorNombreAsync(string nombre);

        Task<RespuestaGeneral<string>> CrearAsync(SolicitudEmpresa solicitud);

        Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudEmpresa solicitud);

        Task<RespuestaGeneral<string>> EliminarAsync(string nombre, string usuario, string ip);

        Task<RespuestaGeneral<string>> ActualizarEstadoRucAsync(
            string nombreEmpresa,
            string nuevoEstado,
            string usuario,
            string ip
        );

        Task<RespuestaGeneral<string>> CambiarCredencialesAsync(string nuevoUsuario, string nuevaClave);
    }
}