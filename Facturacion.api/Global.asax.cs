using System.Web;
using System.Web.Http;
using Facturacion.api.Logging;
using Serilog;

namespace Facturacion.api
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            LoggingConfig.Configurar();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        protected void Application_Error()
        {
            try
            {
                var ex = Server.GetLastError();
                Log.Error(ex, "Error no controlado fuera del pipeline Web API.");
            }
            catch
            {
            }
        }

        protected void Application_End()
        {
            LoggingConfig.Cerrar();
        }
    }
}
