using System;
using System.ComponentModel.DataAnnotations;

namespace AlgotechUnityREST_ckb.Models
{
    /// <summary>
    /// Test kontroler
    /// </summary>
    public class TestDocuments
    {
        #region Konstrutor
        /// <summary>
        /// Konstrutor
        /// </summary>
        public TestDocuments()
        {
            DocumentID = 0;
            DocumentName = string.Empty;
            DocumentDate = DateTime.MinValue;
        }
        #endregion

        #region Svojstva
        /// <summary>
        /// Jedinstveni idektifikator dokumenta dodeljen u momentu kreiranja.
        /// </summary>
        [Key]
        [Range(1, 10000)]
        public long DocumentID { get; set; }
        /// <summary>
        /// Naziv dokumenta koji kreira OnBase prema konvenciji za automatsko nazivanje dokumenata.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string DocumentName { get; set; }

        /// <summary>
        /// Datum i vreme dokumenta.
        /// </summary>
        [Required]
        public DateTime DocumentDate { get; set; }

        ///// <summary>
        ///// ETag 
        ///// </summary>
        //[ConcurrencyCheck]
        //public Guid ETag { get; set; }
        #endregion
    }
}