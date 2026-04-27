using Facturacion.api.Models.Solicitudes;
using Facturacion.api.DTOs;
using Facturacion.api.Models.Respuestas;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Facturacion.api.Servicios
{
    public interface IServicioRetenciones
    {
        Task<RespuestaGeneral<string>> InsertarAsync(SolicitudRetenciones solicitud);

        Task<RespuestaGeneral<string>> ActualizarAsync(string numeroRetencion, string estado, string codigo,
                                                       string claveAcceso, string ambiente,
                                                       string tipoEmision, string fechaEmision, string horaEmision,
                                                       string usuario, string ip);

        Task<RespuestaGeneral<string>> EliminarAsync(string numeroRetencion, string usuario, string ip);

        Task<RespuestaGeneral<DtoRetencion>> ConsultarAsync(string numeroRetencion);

        Task<RespuestaGeneral<List<DtoRetencion>>> ListarAsync(int top, string filtro);

        Task<RespuestaGeneral<List<DtoRetencionConsulta>>> ConsultarAvanzadoAsync(
            string fechaDesde,
            string fechaHasta,
            string sujetoRetenido,
            string numeroRetencion,
            string numeroFactura
        );
    }
}