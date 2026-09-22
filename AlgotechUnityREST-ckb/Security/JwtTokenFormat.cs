// ============================================================================
// Autor:           Predrag Živić
// Datum kreiranja: 22.09.2026
// Naziv skripte:   Autorizacija servisa
//
// Opis: Klasa koja implementira OAuth2 autorizaciju koristeći Client Credentials grant tip.
//
// Istorija izmena:
// 22.09.2026 - Predrag Živić - Inicijalna verzija
// ============================================================================

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;

namespace AlgotechUnityREST
{
    /// <summary>
    /// Token format za JWT (JSON Web Token) koji implementira ISecureDataFormat interfejs.
    /// </summary>
    public class JwtTokenFormat :
        ISecureDataFormat<AuthenticationTicket>
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly SymmetricSecurityKey _key;

        #region JwtTokenFormat
        /// <summary>
        /// JwtTokenFormat konstruktor koji inicijalizuje izdavača, publiku i ključ za potpisivanje tokena.
        /// </summary>
        /// <param name="issuer"></param>
        /// <param name="audience"></param>
        /// <param name="signingKey"></param>
        public JwtTokenFormat(
            string issuer,
            string audience,
            string signingKey)
        {
            _issuer = issuer;
            _audience = audience;

            _key = new SymmetricSecurityKey(
                Convert.FromBase64String(signingKey));
        }
        #endregion

        #region Protect
        /// <summary>
        /// Zaštićuje AuthenticationTicket i generiše JWT token.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string Protect(AuthenticationTicket data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var issued =
                data.Properties.IssuedUtc ??
                DateTimeOffset.UtcNow;

            var expires =
                data.Properties.ExpiresUtc ??
                issued.AddMinutes(30);

            var credentials =
                new SigningCredentials(
                    _key,
                    SecurityAlgorithms.HmacSha256);

            var token =
                new JwtSecurityToken(
                    issuer: _issuer,
                    audience: _audience,
                    claims: data.Identity.Claims,
                    notBefore: issued.UtcDateTime,
                    expires: expires.UtcDateTime,
                    signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
        #endregion
        
        #region Unprotect
        public AuthenticationTicket Unprotect(
            string protectedText)
        {
            if (string.IsNullOrEmpty(protectedText))
                return null;

            try
            {
                var handler =
                    new JwtSecurityTokenHandler();

                var parameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = _issuer,

                        ValidateAudience = true,
                        ValidAudience = _audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = _key,

                        ValidateLifetime = true,

                        ClockSkew =
                            TimeSpan.FromMinutes(1),

                        AuthenticationType = "Bearer"
                    };

                SecurityToken validatedToken;

                var principal =
                    handler.ValidateToken(
                        protectedText,
                        parameters,
                        out validatedToken);

                var identity =
                    principal.Identity as ClaimsIdentity;

                if (identity == null)
                    return null;

                return new AuthenticationTicket(
                    identity,
                    new AuthenticationProperties());
            }
            catch
            {
                return null;
            }
        }
        #endregion
    }
}