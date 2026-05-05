using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Facturacion.api.Factories;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios.Interfaces;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/notas")]
    public class NotasController : BaseController
    {
        private IServicioNota _servicio;

        protected override void InicializarServicios()
        {
            _servicio = ServiciosFactory.CreateServicioNota(AppServices);
        }


        [HttpPost]
        [Route("crear")]
        public async Task<IHttpActionResult> Crear([FromBody] SolicitudNota solicitud)
        {
            if (solicitud == null)
                return BadRequest("Solicitud inválida.");

            var resultado = await _servicio.CrearNotaAsync(solicitud);

            if (resultado == null)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Respuesta nula del servicio."));

            if (!resultado.Exito)
                return Content(HttpStatusCode.BadRequest, resultado);

            return Ok(resultado);
        }

        [HttpPost]
        [Route("preview")]
        public async Task<IHttpActionResult> Preview([FromBody] SolicitudNota solicitud)
        {
            if (solicitud == null)
                return BadRequest("Solicitud inválida.");

            var resultado = await _servicio.PreviewNotaAsync(solicitud);

            if (resultado == null)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Respuesta nula del servicio."));

            if (!resultado.Exito)
                return Content(HttpStatusCode.BadRequest, resultado);

            return Ok(resultado);
        }

        [HttpPost]
        [Route("procesar-autorizacion")]
        public async Task<IHttpActionResult> ProcesarDesdeAutorizacion([FromBody] SolicitudProcesarPendienteDocumento solicitud)
        {
            if (solicitud == null)
                return BadRequest("Solicitud inválida.");

            var resultado = await _servicio.ProcesarNotaDesdeAutorizacionAsync(solicitud);

            if (resultado == null)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Respuesta nula del servicio."));

            if (!resultado.Exito)
                return Content(HttpStatusCode.BadRequest, resultado);

            return Ok(resultado);
        }
    }
}
