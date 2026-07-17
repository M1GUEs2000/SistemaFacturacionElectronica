using System;
using System.Configuration;
using System.IO;
using System.Web.Hosting;
using Serilog;
using Serilog.Events;

namespace Facturacion.api.Logging
{
    public static class LoggingConfig
    {
        public static void Configurar()
        {
            var ruta = ResolverRutaLog(
                ConfigurationManager.AppSettings["LogPath"] ?? "~/App_Data/logs/facturacion-.log");
            var nivel = ObtenerNivel(
                ConfigurationManager.AppSettings["LogMinimumLevel"]);

            var directorio = Path.GetDirectoryName(ruta);
            if (!string.IsNullOrWhiteSpace(directorio))
                Directory.CreateDirectory(directorio);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(nivel)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    ruta,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    shared: true,
                    outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] " +
                        "{SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}")
                .CreateLogger();
        }

        public static void Cerrar()
        {
            Log.CloseAndFlush();
        }

        private static LogEventLevel ObtenerNivel(string valor)
        {
            LogEventLevel nivel;
            return Enum.TryParse(valor, true, out nivel)
                ? nivel
                : LogEventLevel.Information;
        }

        private static string ResolverRutaLog(string rutaConfigurada)
        {
            var ruta = (rutaConfigurada ?? string.Empty).Trim();
            var raiz = HostingEnvironment.MapPath("~/") ?? AppDomain.CurrentDomain.BaseDirectory;

            if (ruta.StartsWith("~/"))
                return Path.Combine(raiz, ruta.Substring(2).Replace('/', Path.DirectorySeparatorChar));

            return Path.IsPathRooted(ruta)
                ? ruta
                : Path.Combine(raiz, ruta.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
