using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Web.Http;

namespace AlgotechUnityREST_ckb
{
    /// <summary>
    /// WebApiConfig
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Registracija
        /// </summary>
        /// <param name="config"></param>
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            //// Konfigurisanje globalnih postavki JSON serijalizacije
            //config.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            //config.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc; // Opciono, ako želite da sve datume tretirate kao UTC
            //config.Formatters.JsonFormatter.SerializerSettings.DateFormatString = "dd.mm.yyyy HH:mm:ss";
            //////config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new IsoDateTimeConverter());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }

            );
        }
    }
}
