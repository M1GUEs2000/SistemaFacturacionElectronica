using Facturacion.api.Models.Solicitudes;
using Facturacion.api.DTOs;
using Facturacion.api.Models.Respuestas;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Facturacion.api.Servicios
{
    public interface IServicioParametrosFacturas
    {
        // LISTADOS
        Task<RespuestaGeneral<List<DtoParametrosFactura>>> MostrarAsync();
        Task<RespuestaGeneral<DtoParametrosFactura>> ConsultarPorNombreAsync(string nombre);

        // VALIDACIÓN
        Task<RespuestaGeneral<bool>> EsProduccionAsync(string nombre);
        Task<RespuestaGeneral<string>> CambiarAProduccionAsync(string nombre);

        // CRUD
        Task<RespuestaGeneral<string>> InsertarAsync(SolicitudParametrosFactura solicitud);
        Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudParametrosFactura solicitud);
        Task<RespuestaGeneral<string>> EliminarAsync(string nombre, string usuario, string ip);
    }
}