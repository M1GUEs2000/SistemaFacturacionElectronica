using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using Serilog;
using Serilog.Context;

namespace Facturacion.api.Seguridad
{
    /// <summary>
    /// Convierte excepciones no controladas en un error JSON seguro.
    /// El detalle tecnico se registra en Serilog.
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var traceId = TraceIdHandler.ObtenerTraceId(context.Request);
            using (LogContext.PushProperty("TraceId", traceId))
                Log.Error(context.Exception, "Error no controlado en Web API.");

            var problem = new ProblemDetailsResponse
            {
                Type = "https://httpstatuses.com/500",
                Title = "Ocurrio un error interno al procesar la solicitud.",
                Status = (int)HttpStatusCode.InternalServerError,
                TraceId = traceId
            };

            var response = context.Request.CreateResponse(HttpStatusCode.InternalServerError, problem);
            response.Headers.Add(TraceIdHandler.ResponseHeaderName, traceId);

            context.Result = new ResponseMessageResult(response);
            return Task.CompletedTask;
        }
    }
}
