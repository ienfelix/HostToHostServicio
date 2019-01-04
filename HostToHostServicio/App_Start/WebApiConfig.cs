using Newtonsoft.Json.Serialization;
using System.Web.Http;

namespace HostToHostServicio
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional, @namespace = "api" }
            );

            config.Routes.MapHttpRoute(
                name: "ServicioApi",
                routeTemplate: "api/{controller}/{idSociedad}/{anio}/{momentoOrden}/{idEstadoOrden}/{idSap}/{usuario}"
            );

            var jsonFormatter = config.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            jsonFormatter.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
        }
    }
}
