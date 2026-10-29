// 31.10.2023
// Meta podaci ključne reči dokumenata kod korisnika

using System;

namespace AlgotechUnityREST_generali.Models
{
    /// <summary>
    /// Ključne reči
    /// </summary>
    public class Keywords
    {
        #region Konstruktor
        /// <summary>
        /// Konstruktor
        /// </summary>
        public Keywords()
        {
            GroupType = string.Empty;
            GroupID = string.Empty;
            GroupBarcode = string.Empty;
            DocID = string.Empty;
            DocBarcode = string.Empty;
            ClientID = string.Empty;
            ClientName = string.Empty;
            DataSource = string.Empty;

            // Idemo sa minvalue za default
            DocRegDate = DateTime.MinValue;

            // Idemo da podrazumevana vrednost bude tekući datum
            //DocRegDate = DateTime.Now;

            DocStatus = "2 - SKENIRANO";

            OrganizationalUnit = string.Empty;
            UserName = string.Empty;
        }
        #endregion

        #region Svojstva
        /// <summary>
        /// Jedinstveni identifikator klijenta u core sistemu
        /// </summary>
        /// <example>1510739631881</example>
        //[IgnoreDataMember]
        //[System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
        public string ClientID { get; set; }

        /// <summary>
        /// Naziv pravnog lica (ako je fizičko lice može da se popunjva konatanacijo ime i prezime)
        /// </summary>
        /// <example></example>
        public string ClientName { get; set; }

        /// <summary>
        /// DataSet: Grupa dokumenata
        /// </summary>
        /// [IgnoreDataMember]
        public string GroupType { get; set; }
        /// <summary>
        /// Referenca grupe, istovremeno i referenca primarnog dokumenta u grupi
        /// </summary>
        public string GroupID { get; set; }
        /// <summary>
        /// Štampana referenca grupe, ne prijavljuje se za sve dokumente u grupi
        /// </summary>
        public string GroupBarcode { get; set; }
        /// <summary>
        /// Referenca dokumenata iz izvornog sistema koji vrši registraciju
        /// </summary>
        public string DocID { get; set; }
        /// <summary>
        /// Referenca dokumenta, Mora da bude jedinstven. [DataSource] – [Document Type] - [Document Handle].
        /// </summary>

        public string DocBarcode { get; set; }
        /// <summary>
        /// Izvor podatka DataSet: BIIS, MB…
        /// </summary>
        
        public string DataSource { get; set; }

        /// <summary>
        /// Datum kada je dokument registrovan u starom sistemu. 
        /// </summary>
        public DateTime DocRegDate { get; set; }

        /// <summary>
        /// DataSet: 1 - NEMA DOKUMENTA; 2 - SKENIRANO; 3 - IMPORTOVANO
        /// </summary>
        public string DocStatus { get; set; }

        /// <summary>
        /// Naziv organizacione jedinice
        /// </summary>
        /// <example>FILIJALA SOMBOR</example>
         public string OrganizationalUnit { get; set; }

        /// <summary>
        /// Korisničko ime sa AD-a
        /// </summary>
        /// <example>algotech\\demo_user</example>
        /// 
         public string UserName { get; set; }
        #endregion
    }
}