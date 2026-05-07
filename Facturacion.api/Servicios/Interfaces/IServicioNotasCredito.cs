using Facturacion.api.Models.Solicitudes;
using Facturacion.api.DTOs;
using Facturacion.api.Models.Respuestas;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Facturacion.api.Servicios
{
    public interface IServicioNotasCredito
    {
        // CRUD ENCABEZADO
        Task<RespuestaGeneral<string>> InsertarAsync(SolicitudNotaCredito solicitud);
        Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudNotaCredito solicitud);
        Task<RespuestaGeneral<string>> EliminarAsync(string numeroNota, string usuario, string ip);

        // CONSULTAS
        Task<RespuestaGeneral<DtoNotaCredito>> ConsultarPorNumeroAsync(string numeroNota);
        Task<RespuestaGeneral<List<DtoNotaCredito>>> ListarAsync(int top);

        // CONSULTAS TABLA
        Task<RespuestaGeneral<List<DtoNotaCreditoListado>>> ConsultarPorFechasAsync(string fechaDesde, string fechaHasta);
        Task<RespuestaGeneral<List<DtoNotaCreditoListado>>> ConsultarPorClienteAsync(string fechaDesde, string fechaHasta, string cedula);
        Task<RespuestaGeneral<List<DtoNotaCreditoListado>>> ConsultarPorNumeroNotaFechasAsync(string fechaDesde, string fechaHasta, string numeroNota);
        Task<RespuestaGeneral<List<DtoNotaCreditoListado>>> ConsultarPorClienteYNumeroNotaAsync(string fechaDesde, string fechaHasta, string cedula, string numeroNota);

        // DETALLE
        Task<RespuestaGeneral<string>> InsertarDetalleAsync(SolicitudNotaCreditoDetalle solicitud);
        Task<RespuestaGeneral<List<DtoNotaCreditoDetalle>>> ConsultarDetalleAsync(string numeroNota);
        Task<RespuestaGeneral<string>> EliminarDetalleAsync(string numeroNota);

        // UTILIDADES
        Task<RespuestaGeneral<string>> RenombrarNotaAsync(string vieja, string nueva);
        Task<RespuestaGeneral<List<string>>> ListarFacturasUnicasAsync();
    }
}