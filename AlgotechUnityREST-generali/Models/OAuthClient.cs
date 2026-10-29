namespace AlgotechUnityREST_generali.Models
{
    public class OAuthClient
    {
        public int OAuthClientID { get; set; }

        public string ClientID { get; set; }

        public string ClientSecretHash { get; set; }

        public bool IsActive { get; set; }

        public string Description { get; set; }

        public string AllowedIP { get; set; }
    }
}