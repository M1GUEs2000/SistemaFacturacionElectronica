using System.Web.Http;
using System.Web.Http.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Facturacion.api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var cors = new EnableCorsAttribute(
                "http://sistemadefacturacion.ssm.com.ec,https://sistemadefacturacion.ssm.com.ec,http://facturacionapi.ssm.com.ec,https://facturacionapi.ssm.com.ec,http://localhost:5173",
                "*",
                "*"
            );

            config.EnableCors(cors);

            // RUTAS
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // JSON EN CAMEL CASE
            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            json.SerializerSettings.Formatting = Formatting.Indented;

            // OPCIONAL: devolver solo JSON
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}