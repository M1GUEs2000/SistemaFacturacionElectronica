using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Facturacion.api.Auth;
using Facturacion.api.Factories;
using Facturacion.api.Models.Respuestas;
using LogicaNegocios.Services;

namespace Facturacion.api.Controllers
{
    public abstract class BaseController : ApiController
    {
        protected AppServices AppServices { get; private set; }
        protected string EmpresaActual { get; private set; }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            IEnumerable<string> valores;
            if (controllerContext.Request.Headers.TryGetValues("X-Empresa", out valores))
                EmpresaActual = valores.FirstOrDefault();

            if (RequiereAutenticacion())
                ValidarToken(controllerContext);

            AppServices = AppServicesFactory.Create(EmpresaActual);
            InicializarServicios();
        }

        private void ValidarToken(HttpControllerContext ctx)
        {
            var auth = ctx.Request.Headers.Authorization;
            if (auth == null || auth.Scheme != "Bearer" ||
                !JwtHelper.Validar(auth.Parameter, out _))
            {
                throw new HttpResponseException(
                    ctx.Request.CreateResponse(
                        HttpStatusCode.Unauthorized,
                        new { mensaje = "No autorizado. Token inválido o ausente." }
                    )
                );
            }
        }

        protected virtual bool RequiereAutenticacion() => true;

        protected virtual void InicializarServicios() { }

        protected IHttpActionResult HandleResponse<T>(RespuestaGeneral<T> respuesta)
        {
            if (respuesta == null)
                return ResponseMessage(
                    Request.CreateErrorResponse(
                        HttpStatusCode.InternalServerError,
                        "Respuesta nula del servicio."
                    )
                );

            if (!respuesta.exito)
                return Content(HttpStatusCode.BadRequest, respuesta);

            return Ok(respuesta);
        }
    }
}
