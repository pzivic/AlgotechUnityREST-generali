// ============================================================================
// Autor:           Predrag Živić
// Datum kreiranja: 22.09.2026
// Naziv skripte:   Skladište za OAuth klijente
//
// Opis:
// Skladište za OAuth klijente. Proverava da li su clientId i clientSecret validni.
//
// Istorija izmena:
// 22.09.2026 - Predrag Živić - Inicijalna verzija
// ============================================================================

/// <summary>
/// Skladište za OAuth klijente. Proverava da li su clientId i clientSecret validni.
/// </summary>
public static class OAuthClientStore
{
    #region Validate
    /// <summary>
    /// Proverava da li su clientId i clientSecret validni.
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="clientSecret"></param>
    /// <returns></returns>
    public static bool Validate(string clientId, string clientSecret)
    {
        // Za početak / POC
        var configuredClientId =
            System.Configuration.ConfigurationManager
                .AppSettings["OAuthClientId"];

        var configuredClientSecret =
            System.Configuration.ConfigurationManager
                .AppSettings["OAuthClientSecret"];

        return
            clientId == configuredClientId &&
            clientSecret == configuredClientSecret;
    }
    #endregion
}