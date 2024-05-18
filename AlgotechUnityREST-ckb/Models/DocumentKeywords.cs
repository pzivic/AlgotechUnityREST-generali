// Predrag Živić
// 08.11.2022

namespace AlgotechUnityREST.Models
{
    /// <summary>
    /// Dokumenta i ključne reči u opštem SMISLU
    /// </summary>
    public class DocumentKeywords
    {
        #region Konstrutor
        /// <summary>
        /// Konstrutor
        /// </summary>
        public DocumentKeywords()
        {
            DocumentTypeName = string.Empty;
            KeywordName = string.Empty;
            DataSource = "default";
            UserName = "default";
        }
        #endregion

        /// <summary>
        /// Naziv OnBase tipa dokumenta
        /// </summary>
        public string DocumentTypeName { get; set; }
        /// <summary>
        /// Naziv klučne reči
        /// </summary>
        public string KeywordName { get; set; }
        /// <summary>
        /// Vrednost klučne reči
        /// </summary>
        public string KeywordValue { get; set; }

        /// <summary>
        /// Za log Izvor podataka
        /// </summary>
        public string DataSource { get; set; }
        /// <summary>
        /// Za log Korisničko ime
        /// </summary>
        public string UserName { get; set; }

    }
}