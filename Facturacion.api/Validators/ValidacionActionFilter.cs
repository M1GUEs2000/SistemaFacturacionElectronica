using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using FluentValidation;
using FluentValidation.Results;
using Facturacion.api.Models.Respuestas;

namespace Facturacion.api.Validators
{
    /// <summary>
    /// Valida los argumentos de accion contra su validador registrado antes de
    /// que la peticion llegue a la logica de negocio. Una solicitud invalida
    /// devuelve 400 con la misma forma <see cref="RespuestaGeneral{T}"/> del resto
    /// de la API. Los tipos sin validador registrado pasan sin cambios.
    /// </summary>
    public sealed class ValidacionActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            foreach (var argumento in actionContext.ActionArguments.Values)
            {
                if (argumento == null)
                    continue;

                var validador = ValidadorRegistro.ObtenerValidador(argumento.GetType());
                if (validador == null)
                    continue;

                ValidationResult resultado =
                    validador.Validate(new ValidationContext<object>(argumento));

                if (resultado.IsValid)
                    continue;

                var errores = resultado.Errors
                    .Select(e => new { campo = e.PropertyName, error = e.ErrorMessage })
                    .ToList();

                var mensaje = string.Join(" ", resultado.Errors.Select(e => e.ErrorMessage));

                var respuesta = new RespuestaGeneral<object>
                {
                    exito = false,
                    mensaje = mensaje,
                    data = errores
                };

                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.BadRequest, respuesta);
                return;
            }
        }
    }
}
