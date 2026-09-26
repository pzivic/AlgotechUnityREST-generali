// ============================================================================
// Autor:           Predrag Živić
// Datum kreiranja: 22.09.2026
// Naziv skripte:   Autorizacija servisa
//
// Opis: Klasa koja implementira OAuth2 autorizaciju koristeći Client Credentials grant tip.
// Omogućava validaciju klijentskih kredencijala i izdavanje tokena za pristup REST API-ju.
//
// Istorija izmena:
// 22.09.2026 - Predrag Živić - Inicijalna verzija
// 25.09.2026 - Predrag Živić - Dodat logger
// ============================================================================

using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.Owin.Security.OAuth;

namespace AlgotechUnityREST.Security
{
    /// <summary>
    /// Klasa koja implementira OAuth2 autorizaciju koristeći Client Credentials grant tip.
    /// </summary>
    public class ClientCredentialsProvider : OAuthAuthorizationServerProvider
    {
        #region Variable  
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region ValidateClientAuthentication
        /// <summary>
        /// Proverava validnost klijentskih kredencijala (client_id i client_secret) prosleđenih u zahtevu.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            _logger.Info("Proverava validnost klijentskih kredencijala prosleđenih u zahtevu.");

            if (!context.TryGetFormCredentials(out string clientID, out string clientSecret))
            {
                context.SetError("invalid_client", "client_id/client_secret missing");
                return Task.FromResult<object>(null);
            }

            var client = OAuth.GetOAuthClient(clientID);
            if (client == null)
            {
                context.SetError("invalid_client", "Invalid client credentials");
                return Task.FromResult<object>(null);
            }

            if (!AlgotechUnityREST.Shared.Util.VerifyClientSecret(clientSecret, client.ClientSecretHash))
            {
                context.SetError("invalid_client", "Invalid client credentials");
                return Task.FromResult<object>(null);
            }

            _logger.Info(string.Format ("Proverava validnost klijentskih kredencijala (client_id {0} i client_secret) prosleđenih u zahtevu.", clientID));
            context.Validated(clientID);

            return Task.FromResult<object>(null);
        }
        #endregion

        #region GrantClientCredentials
        /// <summary>
        /// Dodeljuje token za pristup REST API-ju nakon uspešne validacije klijentskih kredencijala.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task GrantClientCredentials(
            OAuthGrantClientCredentialsContext context)
        {
            var identity =
                new ClaimsIdentity(
                    context.Options.AuthenticationType);

            identity.AddClaim(
                new Claim(
                    ClaimTypes.Name,
                    context.ClientId));

            identity.AddClaim(
                new Claim(
                    "client_id",
                    context.ClientId));

            context.Validated(identity);

            return Task.FromResult<object>(null);
        }
        #endregion
    }
}