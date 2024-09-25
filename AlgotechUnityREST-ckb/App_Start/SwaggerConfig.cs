// Predrag Živić
// 01.11.2023

using System.Web.Http;
using WebActivatorEx;
using AlgotechUnityREST_ckb;
using Swashbuckle.Application;
using System.Linq;
using System;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace AlgotechUnityREST_ckb
{
    /// <summary>
    /// Konfiguracija
    /// </summary>
    public class SwaggerConfig
    {

        /// <summary>
        /// Registracija
        /// </summary>
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "AlgotechUnityREST_ckb");
                    c.IncludeXmlComments(string.Format(@"{0}\bin\AlgotechUnityREST-ckb.xml", AppDomain.CurrentDomain.BaseDirectory));
                    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                })
                .EnableSwaggerUi(c =>
                {
                    c.DisableValidator();
                });
        }
    }
}
