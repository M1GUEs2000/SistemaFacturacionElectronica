using Facturacion.api.Models.Solicitudes;
using Facturacion.api.DTOs;
using Facturacion.api.Models.Respuestas;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Facturacion.api.Servicios
{
    public interface IServicioSecuenciales
    {
        // LISTADOS
        Task<RespuestaGeneral<List<DtoSecuencial>>> MostrarAsync();
        Task<RespuestaGeneral<DtoSecuencial>> ConsultarPorTipoAsync(string tipoComprobante);

        // UTILIDADES
        Task<RespuestaGeneral<long>> ObtenerSecuencialRealAsync(string tipoComprobante);
        Task<RespuestaGeneral<string>> ObtenerSecuencialFormateadoAsync(string tipoComprobante);
        Task<RespuestaGeneral<string>> ObtenerCodigoNumericoAsync(string tipoComprobante);

        // CRUD
        Task<RespuestaGeneral<string>> InsertarAsync(SolicitudSecuencial solicitud);
        Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudSecuencial solicitud);
        Task<RespuestaGeneral<string>> IncrementarAsync(string tipoComprobante);
        Task<RespuestaGeneral<string>> EliminarAsync(string tipoComprobante, string usuario, string ip);
    }
}