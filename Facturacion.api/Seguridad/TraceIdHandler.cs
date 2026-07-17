using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Context;

namespace Facturacion.api.Seguridad
{
    /// <summary>
    /// Asigna un identificador a cada petición y registra su resultado.
    /// </summary>
    public sealed class TraceIdHandler : DelegatingHandler
    {
        public const string RequestPropertyKey = "Facturacion.TraceId";
        public const string ResponseHeaderName = "X-Trace-Id";

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var traceId = ObtenerTraceId(request);
            var cronometro = Stopwatch.StartNew();

            using (LogContext.PushProperty("TraceId", traceId))
            {
                var response = await base.SendAsync(request, cancellationToken);

                if (response != null && !response.Headers.Contains(ResponseHeaderName))
                    response.Headers.Add(ResponseHeaderName, traceId);

                Log.Information(
                    "HTTP {Method} {Path} respondio {StatusCode} en {ElapsedMilliseconds} ms.",
                    request.Method,
                    request.RequestUri == null ? string.Empty : request.RequestUri.AbsolutePath,
                    response == null ? 0 : (int)response.StatusCode,
                    cronometro.ElapsedMilliseconds);

                return response;
            }
        }

        public static string ObtenerTraceId(HttpRequestMessage request)
        {
            if (request == null)
                return Guid.NewGuid().ToString("N");

            object valor;
            if (request.Properties.TryGetValue(RequestPropertyKey, out valor) && valor is string existente)
                return existente;

            var traceId = Guid.NewGuid().ToString("N");
            request.Properties[RequestPropertyKey] = traceId;
            return traceId;
        }
    }
}
