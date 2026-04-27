using System.Threading.Tasks;
using System.Web.Http;
using Facturacion.api.Controllers;
using Facturacion.api.Factories;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios;
using Facturacion.api.Servicios.Interfaces;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/retenciones")]
    public class RetencionesController : BaseController
    {
        private IServicioRetencion _servicio;

        protected override void InicializarServicios()
        {
            _servicio = ServiciosFactory.CreateServicioRetencion(AppServices);
        }

        // =====================================================
        // 1) CREAR RETENCIÓN ELECTRÓNICA COMPLETA
        // =====================================================
        [HttpPost]
        [Route("crear")]
        public async Task<IHttpActionResult> CrearRetencion([FromBody] SolicitudCrearRetencion solicitud)
        {
            if (solicitud == null)
                return BadRequest("Solicitud inválida.");

            var resultado = await _servicio.CrearRetencionAsync(solicitud);
            return Ok(resultado);
        }

        // =====================================================
        // 2) REPROCESAR DESDE AUTORIZACIÓN / PDF / CORREO
        // =====================================================
        [HttpPost]
        [Route("procesar-autorizacion")]
        public async Task<IHttpActionResult> ProcesarDesdeAutorizacion([FromBody] SolicitudProcesarPendienteDocumento solicitud)
        {
            if (solicitud == null)
                return BadRequest("Solicitud inválida.");

            var resultado = await _servicio.ProcesarRetencionDesdeAutorizacionAsync(solicitud);
            return Ok(resultado);
        }

        // =====================================================
        // 3) PREVIEW RETENCIÓN (PDF OFFLINE)
        // =====================================================
        [HttpPost]
        [Route("preview")]
        public async Task<IHttpActionResult> PreviewRetencion([FromBody] SolicitudCrearRetencion solicitud)
        {
            if (solicitud == null)
                return BadRequest("Solicitud inválida.");

            var resultado = await _servicio.PreviewRetencionAsync(solicitud);
            return Ok(resultado);
        }
    }
}
