using System;
using System.IO;
using System.Web;
using System.Web.Http;

namespace Facturacion.api
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        protected void Application_Error()
        {
            try
            {
                var ex = Server.GetLastError();
                var path = Server.MapPath("~/App_Data/error.log");

                File.AppendAllText(
                    path,
                    "==============================" + Environment.NewLine +
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine +
                    ex + Environment.NewLine + Environment.NewLine
                );
            }
            catch
            {
            }
        }
    }
}