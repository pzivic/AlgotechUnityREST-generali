// Korisnički nalozi
// 08.03.2024

namespace AlgotechUnityREST_generali.Models
{
    /// <summary>
    /// Model dokumenta
    /// </summary>
    public class Accounts 
    {
        #region Konstrutor
        /// <summary>
        /// Konstrutor
        /// </summary>
        public Accounts()
        {
            UserID = 0;
            UserName = string.Empty;
            DisplayName = string.Empty;
            EmailAddress = string.Empty;

        }
        #endregion

        #region Svojstva
        public long UserID { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        #endregion
    }
}