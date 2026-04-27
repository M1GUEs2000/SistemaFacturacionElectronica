using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Facturacion.api.App_Start;
using Facturacion.api.Factories;
using Facturacion.api.Models.Respuestas;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios.Interfaces;
using LogicaNegocios;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/notas")]
    public class NotasController : ApiController
    {
        private IServicioNota _servicio;

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            string empresa = null;
            IEnumerable<string> valores;
            if (controllerContext.Request.Headers.TryGetValues("X-Empresa", out valores))
                empresa = valores.FirstOrDefault();

            var appServices = AppServicesFactory.Create(empresa);
            _servicio = ServiciosFactory.CreateServicioNota(appServices);
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
