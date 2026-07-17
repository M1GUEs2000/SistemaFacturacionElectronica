using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Facturacion.api.Seguridad
{
    /// <summary>
    /// Aplica CORS con una lista explicita de origenes y responde los preflight
    /// OPTIONS antes de que lleguen a los controladores.
    /// </summary>
    public sealed class CorsHandler : DelegatingHandler
    {
        private readonly CorsSettings _settings;

        public CorsHandler(CorsSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var origin = ObtenerOrigen(request);
            var esOrigenPermitido = !string.IsNullOrWhiteSpace(origin) && _settings.Origenes.Contains(origin);
            var esPreflight = request.Method == HttpMethod.Options &&
                              request.Headers.Contains("Access-Control-Request-Method");

            if (esPreflight)
            {
                if (!esOrigenPermitido)
                    return request.CreateResponse(HttpStatusCode.Forbidden);

                var preflight = request.CreateResponse(HttpStatusCode.NoContent);
                AgregarHeadersCors(preflight, origin, incluirPreflight: true);
                return preflight;
            }

            var response = await base.SendAsync(request, cancellationToken);
            if (esOrigenPermitido)
                AgregarHeadersCors(response, origin, incluirPreflight: false);

            return response;
        }

        private void AgregarHeadersCors(HttpResponseMessage response, string origin, bool incluirPreflight)
        {
            response.Headers.TryAddWithoutValidation("Access-Control-Allow-Origin", origin);
            response.Headers.TryAddWithoutValidation("Access-Control-Expose-Headers", TraceIdHandler.ResponseHeaderName);
            response.Headers.TryAddWithoutValidation("Vary", "Origin");

            if (!incluirPreflight)
                return;

            response.Headers.TryAddWithoutValidation(
                "Access-Control-Allow-Headers",
                string.Join(",", _settings.Headers));
            response.Headers.TryAddWithoutValidation(
                "Access-Control-Allow-Methods",
                string.Join(",", _settings.Metodos));
            response.Headers.TryAddWithoutValidation("Access-Control-Max-Age", "600");
        }

        private static string ObtenerOrigen(HttpRequestMessage request)
        {
            if (request == null || request.Headers == null)
                return null;

            System.Collections.Generic.IEnumerable<string> valores;
            if (!request.Headers.TryGetValues("Origin", out valores))
                return null;

            Uri uri;
            var valor = valores.FirstOrDefault();
            return Uri.TryCreate(valor, UriKind.Absolute, out uri)
                ? uri.GetLeftPart(UriPartial.Authority)
                : null;
        }
    }
}
