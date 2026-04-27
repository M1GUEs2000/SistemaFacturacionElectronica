using Facturacion.api.Models.Solicitudes;
using Facturacion.api.DTOs;
using Facturacion.api.Models.Respuestas;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Facturacion.api.Servicios
{
    public interface IServicioFacturacionTabla
    {
        // COMANDOS
        Task<RespuestaGeneral<string>> InsertarAsync(SolicitudFacturacion s);
        Task<RespuestaGeneral<string>> EliminarAsync(SolicitudFacturacion s);
        Task<RespuestaGeneral<string>> EliminarPorSecuencialAsync(string numeroFactura, string usuario, string ip);

        // CONSULTAS PRINCIPALES
        Task<RespuestaGeneral<List<DtoFacturacion>>> ConsultarPorNumeroAsync(string numeroFactura);
        Task<RespuestaGeneral<List<DtoFacturacion>>> ConsultarFacturaConsumidorFinalAsync(string fecha, string hora, string numeroFactura);
        Task<RespuestaGeneral<List<DtoFacturacion>>> ConsultarFacturaNormalAsync(string fecha, string hora, string cedula, string numeroFactura);

        // CONSULTAS AGRUPADAS
        Task<RespuestaGeneral<List<DtoFacturacionResumen>>> ConsultarTotalesAsync(string fecha);
        Task<RespuestaGeneral<List<DtoFacturacionResumen>>> ConsultarFechasAsync(string desde, string hasta);
        Task<RespuestaGeneral<List<DtoFacturacionResumen>>> ConsultarFechasPorClienteAsync(string desde, string hasta, string cliente);
        Task<RespuestaGeneral<List<DtoFacturacionResumen>>> ConsultarPendientesAsync(string desde, string hasta);
        Task<RespuestaGeneral<List<DtoFacturacionResumen>>> ConsultarConsumidorFinalPorFechaAsync(string desde, string hasta);

        // FILTROS COMPLETOS
        Task<RespuestaGeneral<List<DtoFacturacion>>> ConsultarFiltroAsync(string desde, string hasta, string producto, string cliente, string formaPago);

        // COMBOS
        Task<RespuestaGeneral<List<DtoComboSimple>>> ConsultarClientesAsync();
        Task<RespuestaGeneral<List<DtoComboSimple>>> ConsultarProductosAsync();
        Task<RespuestaGeneral<List<DtoComboSimple>>> ConsultarFormasPagoAsync();

        // SECUENCIALES
        Task<RespuestaGeneral<string>> ObtenerSecuencialPendienteAsync();
        Task<RespuestaGeneral<string>> ObtenerSecuencialConsumidorAsync();

        //Consulta general 

        Task<RespuestaGeneral<List<DtoConsultaGeneral>>> ConsultarGeneralAsync(SolicitudConsultaGeneral s);
    }
}