using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Facturacion.api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Toda petición recibe un identificador trazable, incluso cuando falla.
            config.MessageHandlers.Add(new Seguridad.TraceIdHandler());

            // Las excepciones no controladas se devuelven como JSON seguro.
            config.Services.Replace(
                typeof(System.Web.Http.ExceptionHandling.IExceptionHandler),
                new Seguridad.GlobalExceptionHandler());
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Never;

            // Valida las solicitudes con FluentValidation antes de la logica de negocio.
            config.Filters.Add(new Validators.ValidacionActionFilter());

            Seguridad.CorsConfig.Registrar(config);

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
