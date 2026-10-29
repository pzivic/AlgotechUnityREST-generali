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

using Microsoft.Owin.Security.OAuth;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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
            var dateTimeStart = DateTime.Now;

            try
            {
                _logger.Info("ValidateClientAuthentication start.");

                if (!context.TryGetFormCredentials(out string clientID, out string clientSecret))
                {
                    _logger.Warn("Client credentials nisu prosleđeni.");
                    context.SetError("invalid_client", "client_id/client_secret missing");
                    return Task.FromResult<object>(null);
                }

                _logger.Info(string.Format("Preuzimanje OAuth klijenta. ClientID: {0}.", clientID));

                var client = OAuth.GetOAuthClient(clientID);

                if (client == null)
                {
                    _logger.Warn(string.Format("OAuth klijent nije pronađen ili nije aktivan. ClientID: {0}.", clientID));
                    context.SetError("invalid_client", "Invalid client credentials");
                    return Task.FromResult<object>(null);
                }

                _logger.Info(string.Format("OAuth klijent pronađen. ClientID: {0}, OAuthClientID: {1}.", client.ClientID, client.OAuthClientID));

                if (!AlgotechUnityREST.Shared.Util.VerifyClientSecret(clientSecret, client.ClientSecretHash)) 
                {
                    _logger.Warn(string.Format("Neispravan client_secret. ClientID: {0}.", clientID));
                    context.SetError( "invalid_client", "Invalid client credentials");
                    return Task.FromResult<object>(null);
                }

                _logger.Info(string.Format("Client secret uspešno validiran. ClientID: {0}.", clientID));

                // Provera dozvoljene IP adrese
                if (!string.IsNullOrWhiteSpace(client.AllowedIP))
                {
                    string remoteIP = context.Request.RemoteIpAddress;
                    _logger.Info(string.Format("IP provera. ClientID: {0}, RemoteIP: {1}, AllowedIP: {2}.", clientID, remoteIP, client.AllowedIP));
                    _logger.Info(string.Format("RemoteIP: {0}, X-Forwarded-For: {1}.", context.Request.RemoteIpAddress, context.Request.Headers.Get("X-Forwarded-For")));

                    string[] allowedIPs = client.AllowedIP.Split(',');

                    bool isAllowed = allowedIPs
                        .Select(x => x.Trim())
                        .Any(x => string.Equals(
                            x,
                            remoteIP,
                            StringComparison.OrdinalIgnoreCase));

                    if (!isAllowed)
                    {
                        _logger.Warn(string.Format("Pristup odbijen zbog IP ograničenja. ClientID: {0}, RemoteIP: {1}, AllowedIP: {2}.", clientID, remoteIP, client.AllowedIP));
                        context.SetError("invalid_client", "Client IP address is not allowed.");

                        return Task.FromResult<object>(null);
                    }

                    _logger.Info(string.Format("IP adresa uspešno validirana. ClientID: {0}, RemoteIP: {1}.", clientID, remoteIP));
                }
                else
                {
                    _logger.Info(string.Format("IP ograničenje nije definisano. ClientID: {0}.", clientID));
                }

                context.Validated(clientID);

                _logger.Info(string.Format("OAuth client uspešno autentifikovan. ClientID: {0}.", clientID));

                return Task.FromResult<object>(null);
            }
            catch (Exception ex)
            {
                _logger.Error("Greška u ValidateClientAuthentication.", ex);

                throw;
            }
            finally
            {
                var dateTimeEnd = DateTime.Now;
                var diffInMilliseconds =
                    (dateTimeEnd - dateTimeStart).TotalMilliseconds;

                _logger.Info(string.Format(
                    "ValidateClientAuthentication finally {0} ms.",
                    diffInMilliseconds));
            }
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