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
    /// Konfiguracija Swagger-a
    /// </summary>
    public class SwaggerConfig
    {
        /// <summary>
        /// Registracija Swagger-a
        /// </summary>
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "Algotech Unity REST API")
                        .Description(@"Integracioni REST servis za razmenu podataka između OnBase dokument menadžment sistema i Core bankarskog sistema Crnogorske komercijalne banke (CKB), članice OTP grupe.

Ovaj servis omogućava dvosmernu komunikaciju između dva sistema, uključujući pristup podacima o klijentima i grupama klijenata iz Core sistema, upravljanje dokumentima (metapodaci, ključne reči, sadržaj), kao i otpremanje dokumenata u OnBase iz eksternih izvora.

Servis koristi REST arhitekturu i JSON format za razmenu podataka, sa podrškom za autentifikaciju i logovanje svih poziva unutar OnBase sistema kroz centralnu administraciju.")
                        //.TermsOfService("https://www.ckb.me/uslovi-koriscenja")
                        .Contact(cc => cc
                            .Name("Algotech d.o.o.")
                            .Email("OnBaseSupport-owner@algotech.rs")
                            .Url("https://www.algotech.rs"))
                        .License(lc => lc
                            .Name("Proprietary - Internal Use Only")
                            .Url("https://www.ckb.me"));

                    // Uključi XML komentare
                    c.IncludeXmlComments(string.Format(@"{0}\bin\AlgotechUnityREST-ckb.xml", AppDomain.CurrentDomain.BaseDirectory));

                    // Reši konfliktne rute ako postoje
                    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                    //// JWT podrška (ako koristiš)
                    //c.ApiKey("Authorization")
                    // .Description("JWT token. Primer: Bearer {token}")
                    // .Name("Authorization")
                    // .In("header");
                })
                .EnableSwaggerUi(c =>
                {
                    c.DisableValidator();
                });
        }
    }
}
