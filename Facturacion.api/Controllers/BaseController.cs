using System;
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

            if (RequiereAutenticacion())
            {
                string rol;
                string empresaToken = ValidarTokenYObtenerEmpresa(controllerContext, out rol);

                if (string.Equals(rol, JwtHelper.RolAdmin, StringComparison.OrdinalIgnoreCase))
                {
                    // Operador (admin): SÍ puede dirigirse a un tenant concreto por
                    // el header X-Empresa (aprovisionamiento). Sin header, queda en
                    // la BD general. La autorización real la impone [SoloAdmin].
                    EmpresaActual = LeerHeaderEmpresa(controllerContext);
                }
                else
                {
                    // Tenant: la empresa se fija SIEMPRE desde el claim del token,
                    // nunca del header. Así un token emitido para la empresa A no
                    // puede operar sobre la B enviando X-Empresa: B.
                    EmpresaActual = empresaToken;
                }
            }
            else
            {
                // Endpoints sin token (login): aún no existe ningún token del que
                // derivar el tenant, así que se toma del header X-Empresa para
                // localizar la BD de la empresa que se está autenticando.
                EmpresaActual = LeerHeaderEmpresa(controllerContext);
            }

            AppServices = AppServicesFactory.Create(EmpresaActual);
            InicializarServicios();
        }

        private string ValidarTokenYObtenerEmpresa(HttpControllerContext ctx, out string rol)
        {
            var auth = ctx.Request.Headers.Authorization;
            string empresa;
            rol = null;

            if (auth == null || auth.Scheme != "Bearer" ||
                !JwtHelper.ValidarConRol(auth.Parameter, out empresa, out rol))
            {
                throw NoAutorizado(ctx);
            }

            // Un token de tenant sin empresa está malformado; el de admin sí puede
            // venir sin empresa (opera sobre la BD general).
            bool esAdmin = string.Equals(rol, JwtHelper.RolAdmin, StringComparison.OrdinalIgnoreCase);
            if (!esAdmin && string.IsNullOrEmpty(empresa))
                throw NoAutorizado(ctx);

            return empresa;
        }

        private static string LeerHeaderEmpresa(HttpControllerContext ctx)
        {
            IEnumerable<string> valores;
            if (ctx.Request.Headers.TryGetValues("X-Empresa", out valores))
                return valores.FirstOrDefault();

            return null;
        }

        private static HttpResponseException NoAutorizado(HttpControllerContext ctx)
        {
            return new HttpResponseException(
                ctx.Request.CreateResponse(
                    HttpStatusCode.Unauthorized,
                    new { mensaje = "No autorizado. Token inválido o ausente." }
                )
            );
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
