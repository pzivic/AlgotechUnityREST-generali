namespace AlgotechUnityREST_generali.Models
{
    /// <summary>
    /// KLučne reči za prava pristupa
    /// </summary>
    public class SecurityInfo
    {
        #region Konstruktor
        /// <summary>
        /// Konstruktor
        /// </summary>
        public SecurityInfo()
        {
            OrganizationalUnit = string.Empty;
            UserName = string.Empty;
        }
        #endregion

        /// <summary>
        /// Organizaciona jedinica gde je matičen
        /// </summary>
        public string OrganizationalUnit { get; set; }

        /// <summary>
        /// Korisnik sistema
        /// </summary>
        public string UserName { get; set; }
    }
}