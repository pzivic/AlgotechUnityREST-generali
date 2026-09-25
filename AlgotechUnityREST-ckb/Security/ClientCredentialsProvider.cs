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
// 25.09.2026 - Predrag Živić - ValidateClientAuthentication prema bazi podataka
// ============================================================================

using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.Owin.Security.OAuth;

namespace AlgotechUnityREST_ckb.Security
{
    /// <summary>
    /// Klasa koja implementira OAuth2 autorizaciju koristeći Client Credentials grant tip.
    /// </summary>
    public class ClientCredentialsProvider : OAuthAuthorizationServerProvider
    {
        #region ValidateClientAuthentication old
        ///// <summary>
        ///// Proverava validnost klijentskih kredencijala (client_id i client_secret) prosleđenih u zahtevu.
        ///// </summary>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public override Task ValidateClientAuthentication(
        //    OAuthValidateClientAuthenticationContext context)
        //{
        //    string clientId;
        //    string clientSecret;

        //    // Podržavamo form-urlencoded:
        //    //
        //    // client_id=...
        //    // client_secret=...
        //    //
        //    if (!context.TryGetFormCredentials(
        //            out clientId,
        //            out clientSecret))
        //    {
        //        // Možemo podržati i HTTP Basic
        //        context.TryGetBasicCredentials(
        //            out clientId,
        //            out clientSecret);
        //    }

        //    if (string.IsNullOrEmpty(clientId) ||
        //        string.IsNullOrEmpty(clientSecret))
        //    {
        //        context.SetError(
        //            "invalid_client",
        //            "Client credentials nisu prosleđeni.");

        //        return Task.FromResult<object>(null);
        //    }

        //    if (!OAuthClientStore.Validate(
        //            clientId,
        //            clientSecret))
        //    {
        //        context.SetError(
        //            "invalid_client",
        //            "Neispravan client_id ili client_secret.");

        //        return Task.FromResult<object>(null);
        //    }

        //    context.Validated(clientId);

        //    return Task.FromResult<object>(null);
        //}
        #endregion

        #region ValidateClientAuthentication
        /// <summary>
        /// Proverava validnost klijentskih kredencijala (client_id i client_secret) prosleđenih u zahtevu.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientID;
            string clientSecret;

            if (!context.TryGetFormCredentials(out clientID, out clientSecret))
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

            if (!AlgotechUnityREST_ckb.Shared.Util.VerifyClientSecret(clientSecret, client.ClientSecretHash))
            {
                context.SetError("invalid_client", "Invalid client credentials");
                return Task.FromResult<object>(null);
            }

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
        #endregion
        }
    }
}