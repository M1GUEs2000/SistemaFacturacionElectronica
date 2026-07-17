using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using Serilog;

namespace Facturacion.api.Seguridad
{
    public static class CorsConfig
    {
        private const string HeadersPredeterminados = "authorization,content-type,x-empresa";
        private const string MetodosPredeterminados = "GET,POST,PUT,DELETE,PATCH,OPTIONS";

        public static void Registrar(HttpConfiguration config)
        {
            var configuracion = Cargar();
            if (configuracion.Origenes.Count == 0)
            {
                Log.Warning("CORS no se habilito porque CorsOrigins no contiene origenes HTTP/HTTPS validos.");
                return;
            }

            config.MessageHandlers.Add(new CorsHandler(configuracion));
            Log.Information("CORS habilitado para {CorsOrigins}.", string.Join(",", configuracion.Origenes));
        }

        private static CorsSettings Cargar()
        {
            return new CorsSettings(
                ObtenerOrigenes(ConfigurationManager.AppSettings["CorsOrigins"]),
                ObtenerListaSegura(
                    ConfigurationManager.AppSettings["CorsAllowedHeaders"],
                    HeadersPredeterminados),
                ObtenerListaSegura(
                    ConfigurationManager.AppSettings["CorsAllowedMethods"],
                    MetodosPredeterminados));
        }

        private static List<string> ObtenerOrigenes(string valor)
        {
            var resultado = new List<string>();
            foreach (var origen in Separar(valor))
            {
                Uri uri;
                if (!Uri.TryCreate(origen, UriKind.Absolute, out uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
                    !string.IsNullOrEmpty(uri.Query) ||
                    !string.IsNullOrEmpty(uri.Fragment) ||
                    (uri.AbsolutePath != "/" && !string.IsNullOrEmpty(uri.AbsolutePath)))
                {
                    Log.Warning("Origen CORS ignorado por ser invalido: {CorsOrigin}.", origen);
                    continue;
                }

                var normalizado = uri.GetLeftPart(UriPartial.Authority);
                if (!resultado.Contains(normalizado, StringComparer.OrdinalIgnoreCase))
                    resultado.Add(normalizado);
            }

            return resultado;
        }

        private static List<string> ObtenerListaSegura(string valor, string predeterminado)
        {
            var resultado = Separar(string.IsNullOrWhiteSpace(valor) ? predeterminado : valor)
                .Where(item => item != "*")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return resultado.Count == 0 ? Separar(predeterminado) : resultado;
        }

        private static List<string> Separar(string valor)
        {
            return (valor ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToList();
        }
    }

    public sealed class CorsSettings
    {
        public CorsSettings(IList<string> origenes, IList<string> headers, IList<string> metodos)
        {
            Origenes = new HashSet<string>(origenes, StringComparer.OrdinalIgnoreCase);
            Headers = headers;
            Metodos = metodos;
        }

        public HashSet<string> Origenes { get; private set; }
        public IList<string> Headers { get; private set; }
        public IList<string> Metodos { get; private set; }
    }
}
