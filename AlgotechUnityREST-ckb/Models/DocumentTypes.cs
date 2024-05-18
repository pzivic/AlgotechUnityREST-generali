// Predrag Živić
// 23.05.2023
// Tipovi dokumenata

namespace AlgotechUnityREST.Models
{
    /// <summary>
    /// OnBase tipovi dokumenata
    /// </summary>
    public class DocumentTypes
    {
        #region Konstrutor
        /// <summary>
        /// Konstrutor
        /// </summary>
        public DocumentTypes()
        {
            DocumentTypeName = string.Empty;
        }
        #endregion

        #region Svojstva
        /// <summary>
        /// Identifikator tipa dokumenta
        /// </summary>
        //public long DocumentTypeID { get; set; }
        ///// <summary>
        ///// Grupa
        ///// </summary>
        //public string DocumentTypeGroupName { get; set; }
        /// <summary>
        /// Naziv dokument tipa
        /// </summary>
        public string DocumentTypeName { get; set; }
        #endregion

    }
}