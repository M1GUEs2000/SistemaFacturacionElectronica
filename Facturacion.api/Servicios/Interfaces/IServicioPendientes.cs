using Facturacion.api.Models.Solicitudes;
using Facturacion.api.DTOs;
using Facturacion.api.Models.Respuestas;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace Facturacion.api.Servicios
{
    public interface IServicioPendientes
    {
        // LISTADOS
        Task<RespuestaGeneral<List<DtoPendiente>>> MostrarTodosAsync();
        Task<RespuestaGeneral<DtoPendiente>> ConsultarAsync(string numeroDocumento);
        Task<RespuestaGeneral<DtoPendiente>> ConsultarPorNumeroYTipoAsync(string numeroDocumento, string tipo);

        // COMANDOS
        Task<RespuestaGeneral<string>> InsertarAsync(SolicitudPendiente solicitud);
        Task<RespuestaGeneral<string>> ActualizarEstadoAsync(string numeroDocumento, string estado, string tipo, string usuario, string ip);
        Task<RespuestaGeneral<string>> EliminarAsync(string numeroDocumento, string tipo, string usuario, string ip);

        // GENERADORES DE ESTADO
        Task<RespuestaGeneral<string>> ObtenerEstadoPendienteAsync(string tipo);
        Task<RespuestaGeneral<string>> ObtenerEstadoPendienteAutorizacionAsync(string tipo);
        Task<RespuestaGeneral<string>> ObtenerEstadoPendienteCorreoAsync(string tipo);

        // ACCION UI
        Task<RespuestaGeneral<DtoAccionPendienteDocumento>> ConsultarAccionDocumentoAsync(string numeroDocumento, string tipoDocumento);
    }
}