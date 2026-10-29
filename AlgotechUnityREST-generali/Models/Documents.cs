// 16.09.2022

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AlgotechUnityREST_generali.Models
{
    /// <summary>
    /// Model dokumenta
    /// </summary>
    public class Documents : Keywords
    {

        #region Konstrutor
        /// <summary>
        /// Konstrutor
        /// </summary>
        public Documents()
        {
            RequestID = null;
            DocumentTypeName = string.Empty;
            DefaultFileTypeName = "Image File Format";
            DateStored = DateTime.MinValue;
            Comment = string.Empty;
        }
        #endregion

        #region Svojstva
        /// <summary>
        /// Jedinstveni idektifikator zahteva po kom se kreira dokument
        /// </summary>
        public long? RequestID { get; set; }

        /// <summary>
        /// Jedinstveni idektifikator dokumenta dodeljen u momentu kreiranja.
        /// </summary>
        public long DocumentID { get; set; }

        /// <summary>
        /// Naziv dokumenta koji kreira OnBase prema konvenciji za automatsko nazivanje dokumenata.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Datum i vreme dokumenta.
        /// </summary>
        [Required]
        public DateTime DocumentDate { get; set; }

        /// <summary>
        /// Datum i vreme kada je dokument kreiran ili uvezen u OnBase.
        /// </summary>
        public DateTime DateStored { get; set; }

        /// <summary>
        /// OnBase Tip dokumenta
        /// </summary>
        [Required]
        public string DocumentTypeName { get; set; }

        /// <summary>
        /// FileTipe konfigurisan u okviru DocumentType konfiguracije.
        /// </summary>
        public string DefaultFileTypeName { get; set; }

        /// <summary>
        /// Vraća OnBase korisnika koji je kreirao ili uvezao dokument.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Trenutni status dokumenta sa OnBase-om. Hyland.Unity enum DocumentStatus Active=0 Unindexed=1 Deleted=16
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Komentar dokumenta
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Description("Lista datoteka za import")
        /// </summary>
        [Required]
        public List<string> FileList { get; set; }

        /// <summary>
        /// FileTipe tip datoteke
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// Lista ključnih reči sa vrednostima
        /// </summary>
        public Dictionary<string, string> KeywordList { get; set; }

        /// <summary>
        /// DocumentUrl za DocPop (prikaz dokumenta u Internet pregledaču. Nije OnBase svojstvo.
        /// </summary>
        public string DocumentUrl { get; set; }

        /// <summary>
        /// Fizička putanja
        /// </summary>
        public string FullPath { get; set; }
        #endregion
    }
}