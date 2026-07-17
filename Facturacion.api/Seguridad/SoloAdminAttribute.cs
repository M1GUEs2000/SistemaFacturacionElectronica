using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Facturacion.api.Auth;

namespace Facturacion.api.Seguridad
{
    /// <summary>
    /// Autorización de endpoints de operador: exige un token válido cuyo claim
    /// rol == "admin". Se emite solo en el login de administrador general
    /// (UsuariosGenerales/verificar); el token de un tenant lleva rol "empresa"
    /// y por tanto no pasa.
    ///
    ///   - Sin token o firma inválida -> 401 Unauthorized.
    ///   - Token válido pero rol != admin -> 403 Forbidden.
    ///
    /// Las acciones marcadas con [AllowAnonymous] quedan exentas (p. ej. el
    /// propio login de admin, que aún no tiene token).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class SoloAdminAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (PermiteAnonimo(actionContext))
                return;

            var auth = actionContext.Request.Headers.Authorization;
            string empresa, rol;

            if (auth == null || auth.Scheme != "Bearer" ||
                !JwtHelper.ValidarConRol(auth.Parameter, out empresa, out rol))
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.Unauthorized,
                    new { mensaje = "No autorizado. Token inválido o ausente." });
                return;
            }

            if (!string.Equals(rol, JwtHelper.RolAdmin, StringComparison.OrdinalIgnoreCase))
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.Forbidden,
                    new { mensaje = "Acceso denegado. Se requiere rol de administrador." });
            }
        }

        private static bool PermiteAnonimo(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any() ||
                   actionContext.ControllerContext.ControllerDescriptor
                       .GetCustomAttributes<AllowAnonymousAttribute>().Any();
        }
    }
}
