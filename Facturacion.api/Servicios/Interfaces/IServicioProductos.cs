using Facturacion.api.Models.Solicitudes;
using Facturacion.api.DTOs;
using Facturacion.api.Models.Respuestas;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Facturacion.api.Servicios
{
    public interface IServicioProductos
    {
        // LISTADOS
        Task<RespuestaGeneral<List<DtoProducto>>> MostrarActivosAsync();
        Task<RespuestaGeneral<List<DtoProducto>>> MostrarOrdenAsync();
        Task<RespuestaGeneral<List<DtoProducto>>> MostrarTodoAsync(string nombre);
        Task<RespuestaGeneral<List<string>>> MostrarEstadosAsync();
        Task<RespuestaGeneral<DtoProducto>> ConsultarPorNombreAsync(string nombre);

        // CRUD
        Task<RespuestaGeneral<string>> InsertarAsync(SolicitudProducto solicitud);
        Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudProducto solicitud);
        Task<RespuestaGeneral<string>> EliminarAsync(string nombre, string usuario, string ip);
    }
}