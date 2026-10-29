using System;
using System.Configuration;

using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

using Owin;

[assembly: OwinStartup(typeof(AlgotechUnityREST_ckb.Startup))]

namespace AlgotechUnityREST_ckb
{
    /// <summary>
    /// Configures application authentication using OAuth 2.0 and JWT bearer tokens.
    /// </summary>
    public class Startup
    {
        #region Configuration
        /// <summary>
        /// Konfigures the application authentication using OAuth 2.0 and JWT bearer tokens.
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            ConfigureOAuth(app);
        }
        #endregion

        #region ConfigureOAuth
        /// <summary>
        /// Konfigures OAuth 2.0 authentication and JWT bearer token handling.
        /// </summary>
        /// <param name="app"></param>
        private void ConfigureOAuth(IAppBuilder app)
        {
            var issuer = ConfigurationManager.AppSettings["OAuthIssuer"];
            var audience = ConfigurationManager.AppSettings["OAuthAudience"];
            var signingKey = ConfigurationManager.AppSettings["OAuthSigningKey"];

            var jwtFormat = new AlgotechUnityREST.Security.JwtTokenFormat(
                issuer,
                audience,
                signingKey);

            // OAuth2 token endpoint
            app.UseOAuthAuthorizationServer(
                new OAuthAuthorizationServerOptions
                {
                    //AllowInsecureHttp = false,
                    AllowInsecureHttp = true,
                    TokenEndpointPath = new PathString("/oauth2/token"),
                    // Rok trajanja pristupnog tokena (u minutama)
                    AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(
                        Convert.ToInt16(ConfigurationManager.AppSettings["OAuthAccessTokenExpire"])),
                        Provider = new AlgotechUnityREST.Security.ClientCredentialsProvider(),
                        AccessTokenFormat = jwtFormat
                });

            // Bearer token authentication
            app.UseOAuthBearerAuthentication(
                new OAuthBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                AccessTokenFormat = jwtFormat
            });
        }
        #endregion region
    }
}