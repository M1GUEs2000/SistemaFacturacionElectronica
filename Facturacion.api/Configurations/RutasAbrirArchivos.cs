using System;
using System.IO;
using System.Web;
using System.Web.Routing;
using System.Web.Hosting;
using LogicaNegocios;

namespace FacturacionAPI.Extensions
{
    public static class StaticFilesFacturacionExtensions
    {
        // Call this from Global.asax Application_Start:
        // StaticFilesFacturacionExtensions.RegisterFacturacionStaticFiles(HttpRuntime.AppDomainAppPath, paths);
        public static void RegisterFacturacionStaticFiles(string contentRootPath, FacturacionPaths paths)
        {
            ExponerCarpeta(ResolverRuta(contentRootPath, paths.Facturas, "PDF"), "archivos/facturas/pdf");
            ExponerCarpeta(ResolverRuta(contentRootPath, paths.Facturas, "PDFPREVIEW"), "archivos/facturas/pdfpreview");
            ExponerCarpeta(ResolverRuta(contentRootPath, paths.Facturas, "XMLAUTORIZADOS"), "archivos/facturas/xmlautorizados");

            ExponerCarpeta(ResolverRuta(contentRootPath, paths.NotasCredito, "PDF"), "archivos/notascredito/pdf");
            ExponerCarpeta(ResolverRuta(contentRootPath, paths.NotasCredito, "PDFPREVIEW"), "archivos/notascredito/pdfpreview");
            ExponerCarpeta(ResolverRuta(contentRootPath, paths.NotasCredito, "XMLAUTORIZADOS"), "archivos/notascredito/xmlautorizados");

            ExponerCarpeta(ResolverRuta(contentRootPath, paths.Retenciones, "PDF"), "archivos/retenciones/pdf");
            ExponerCarpeta(ResolverRuta(contentRootPath, paths.Retenciones, "PDFPREVIEW"), "archivos/retenciones/pdfpreview");
            ExponerCarpeta(ResolverRuta(contentRootPath, paths.Retenciones, "XMLAUTORIZADOS"), "archivos/retenciones/xmlautorizados");
        }

        private static string ResolverRuta(string contentRootPath, string rutaBase, string subcarpeta)
        {
            var combinada = Path.Combine(rutaBase ?? string.Empty, subcarpeta);

            if (Path.IsPathRooted(combinada))
                return combinada;

            return Path.GetFullPath(Path.Combine(contentRootPath, combinada));
        }

        private static void ExponerCarpeta(string rutaFisica, string requestPath)
        {
            Directory.CreateDirectory(rutaFisica);

            // Register a route to serve files under the requestPath.
            // The route URL should not start with a leading slash.
            string routeUrl = requestPath.TrimStart('/');
            // Capture the rest of the path as 'filepath'
            var route = new Route(routeUrl + "/{*filepath}", new StaticFileRouteHandler(rutaFisica));
            RouteTable.Routes.Add(route);
        }

        private class StaticFileRouteHandler : IRouteHandler
        {
            private readonly string _carpetaFisica;

            public StaticFileRouteHandler(string carpetaFisica)
            {
                _carpetaFisica = carpetaFisica;
            }

            public IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                var filepath = requestContext.RouteData.Values["filepath"] as string ?? string.Empty;
                return new StaticFileHttpHandler(_carpetaFisica, filepath);
            }
        }

        private class StaticFileHttpHandler : IHttpHandler
        {
            private readonly string _carpetaFisica;
            private readonly string _filepath;

            public StaticFileHttpHandler(string carpetaFisica, string filepath)
            {
                _carpetaFisica = carpetaFisica;
                _filepath = filepath;
            }

            public bool IsReusable => false;

            public void ProcessRequest(HttpContext context)
            {
                try
                {
                    var relative = HttpUtility.UrlDecode(_filepath ?? string.Empty).Replace("/", Path.DirectorySeparatorChar.ToString()).Replace("\\", Path.DirectorySeparatorChar.ToString()).TrimStart(Path.DirectorySeparatorChar);

                    string fullPath = string.IsNullOrWhiteSpace(relative)
                        ? null
                        : Path.GetFullPath(Path.Combine(_carpetaFisica, relative));

                    if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
                    {
                        context.Response.StatusCode = 404;
                        context.Response.Write("Not Found");
                        return;
                    }

                    // Prevent path traversal
                    var rootFull = Path.GetFullPath(_carpetaFisica).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                    if (!fullPath.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.StatusCode = 403;
                        context.Response.Write("Forbidden");
                        return;
                    }

                    byte[] bytes = File.ReadAllBytes(fullPath);
                    string ext = Path.GetExtension(fullPath)?.ToLowerInvariant();
                    string contentType = "application/octet-stream";
                    if (ext == ".pdf") contentType = "application/pdf";
                    else if (ext == ".xml") contentType = "application/xml";

                    context.Response.Clear();
                    context.Response.ContentType = contentType;
                    context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + Path.GetFileName(fullPath) + "\"");
                    context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500;
                    context.Response.Write("Error: " + ex.Message);
                }
            }
        }
    }
}
