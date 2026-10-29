using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AlgotechUnityREST_ckb
{
    /// <summary>
    /// WebApiApplication 
    /// </summary>
    public class WebApiApplication : System.Web.HttpApplication
    {
        #region Variable  
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        /// <summary>
        /// Start
        /// </summary>
        protected void Application_Start()
        {
            _logger.Info("Metoda (start):: Application_Start");

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            _logger.Info("GlobalConfiguration");

            // Konfiguracija globalnih postavki JSON serializacije za REST servis https://chat.openai.com/c/63ba055c-c24c-429a-b8ca-173e8b7dc362
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified; // Opciono, ako želite da sve datume tretirate kao UTC
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new IsoDateTimeConverter());
            //.

            _logger.Info("DateFormatHandling: " + GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling);
            _logger.Info("DateTimeZoneHandling: " + GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling);
            _logger.Info("Metoda (end):: Application_Start");
        }

        /// <summary>
        /// Inicijalizacija sesije
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Session_Start(object sender, EventArgs e)
        {
            // Inicijalizacije sesije
            //Session["SessionID"] = HttpSessionState.SessionID;
        }

    }
}
