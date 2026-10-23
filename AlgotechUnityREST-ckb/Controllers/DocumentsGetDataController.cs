// Predrag Živić
// 01.11.2023

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Web.Http;

using Hyland.Unity;

using AlgotechUnityREST_ckb.Models;

using System.Diagnostics;
using System.Globalization;


namespace AlgotechUnityREST_ckb.Controllers
{
    /// <summary>
    /// Pretraga dokumenata
    /// </summary>
    //[Authorize]
    public class DocumentsGetDataController : ApiController
    {

        #region Variable  
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        HttpResponseMessage response;
        #endregion

        #region DocumentsList klasa
        /// <summary>
        /// Privatna klasa za listanje dokumenata
        /// </summary>
        [Newtonsoft.Json.JsonObject(Title = "DocumentsList")]
        private class DocumentsList
        {
            public string DocumentID { get; set; }
            public string Name { get; set; }
            public string DocumentTypeName { get; set; }
            public DateTime DocumentDate { get; set; }
            public DateTime DateStored { get; set; }
            public string DocumentUrlByID { get; internal set; }
            public string DocumentUrlByKeyword { get; internal set; }
            public string DocumentFullPath { get; internal set; }
            public string DefaultFileTypeName { get; set; }

            public long RevisionID { get; set; }
            public DateTime RevisionDate { get; set; }

            public string GroupType { get; set; }
            public string GroupID { get; set; }
            public string GroupBarcode { get; set; }
            public string DocID { get; set; }
            public string DocBarcode { get; set; }
            public string ClientID { get; set; }
            public string ClientName { get; set; }
            public string DataSource { get; set; }
            public DateTime? DocRegDate { get; set; }
            public string DocStatus { get; set; }

        }
        #endregion

        #region DocumentList metoda
        /// <summary>
        /// Vraća listu dokumenata za prosleđene parametre. Parametri su opcioni, mogu se kombinovati i mora se proslediti bar jedan parametar.
        /// </summary>
        /// <param name="fromDate">Datum kreiranja dokumenta od</param>
        /// <param name="toDate">Datum kreiranja dokumenta do</param>
        /// <param name="documentTypeName">Naziv tipa dokumenta u OnBase sistemu</param>
        /// <param name="groupBarcode">Referenca grupe dokumenata</param>
        /// <param name="docBarcode">Referenca dokumenta</param>
        /// <param name="clientID">Jedinstveni identifikator klijenta u Core sistemu</param>
        /// <param name="groupType">DataSet: Tipovi proizvoda</param>
        /// <param name="groupID">Referenca grupe, istovremeno i referenca primarnog dokumenta u grupi</param>
        /// <param name="docID">Referenca dokumenata iz izvornog sistema koji vrši registraciju</param>
        /// <returns></returns>
        [HttpGet, ActionName("DocumentList")]
        public HttpResponseMessage DocumentList(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string documentTypeName = "",
            string groupBarcode = "",
            string docBarcode = "",
            string clientID = "",
            string groupType = "",
            string groupID = "",
            string docID = ""
            )
        {
            Application application = null;
            Result result;
            var dateTimeStart = DateTime.Now;

            try
            {
                // Provera ulaznih parametara pretrage
                if (!fromDate.HasValue &&
                    !toDate.HasValue &&
                    documentTypeName == string.Empty &&
                    groupBarcode == string.Empty &&
                    docBarcode == string.Empty &&
                    clientID == string.Empty &&
                    groupType == string.Empty &&
                    groupID == string.Empty &&
                    docID == string.Empty)
                {
                    throw new ArgumentException("Niste definisali kriterijum pretrage");
                }

                if (fromDate.HasValue && !toDate.HasValue)
                {
                    throw new ArgumentException("Niste definisali kriterijum datum do a definisali ste kriterijum datum od!");
                }
                if (!fromDate.HasValue && toDate.HasValue)
                {
                    throw new ArgumentException("Niste definisali kriterijum datum od a definisali ste kriterijum datum do!");
                }
                //.

                Debug.WriteLine (string.Format("Parametri pretrage fromDate: {0} toDate: {1} documentTypeName: {2} clientID: {3} docBarcode {4} groupBarcode {5}.", fromDate, toDate, documentTypeName, clientID, docBarcode, groupBarcode));
                _logger.Info(string.Format("Parametri pretrage fromDate: {0} toDate: {1} documentTypeName: {2} clientID: {3} docBarcode {4} groupBarcode {5}.", fromDate, toDate, documentTypeName, clientID, docBarcode, groupBarcode));

                // Konektuj OnBase
                _logger.Info("OnBase Connect()");
                var obj = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                application = obj.OnBaseConnect();
                if (application == null)
                {
                    _logger.Warn("Greška kod uspostavljanja konekcije sa OnBase");
                    throw new ArgumentException("Greška kod uspostavljanja konekcije sa OnBase. Kontaktirajte OnBase administratora.");
                }
                //.

                // Logijemo od momenta uspostavljanja konekcije
                dateTimeStart = DateTime.Now;
                _logger.Info("Metoda:: DocumentList start " + dateTimeStart);

                // Kreiraj DocumentQuery objekat
                DocumentQuery docQuery = application.Core.CreateDocumentQuery();

                // Datum od do
                if (fromDate.HasValue && toDate.HasValue)
                {
                    docQuery.AddDateRange(Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate));
                }

                // Dodaj dokument tip u uput
                if (documentTypeName != string.Empty)
                {
                    // Pronalazimo document type
                    DocumentType documentType = application.Core.DocumentTypes.Find(documentTypeName);
                    docQuery.AddDocumentType(documentType);
                }

                // Ključne reči kao filter
                if (groupBarcode != string.Empty)
                {
                    docQuery.AddKeyword("GroupBarcode", groupBarcode, KeywordOperator.Equal, KeywordRelation.Or);
                }
                if (docBarcode != string.Empty)
                {
                    docQuery.AddKeyword("DocBarcode", docBarcode, KeywordOperator.Equal, KeywordRelation.Or);
                }

                if (clientID != string.Empty)
                {
                    docQuery.AddKeyword("ClientID", clientID, KeywordOperator.Equal, KeywordRelation.Or);
                }
                if (groupType != string.Empty)
                {
                    docQuery.AddKeyword("GroupType", groupType, KeywordOperator.Equal, KeywordRelation.Or);
                }
                if (groupID != string.Empty)
                {
                    docQuery.AddKeyword("GroupID", groupID, KeywordOperator.Equal, KeywordRelation.Or);
                }
                if (docID != string.Empty)
                {
                    docQuery.AddKeyword("DocID", docID, KeywordOperator.Equal, KeywordRelation.Or);
                }
                //.

                // Dodajemo sort po datumu dokumenta silazno (descending)
                docQuery.AddSort(DocumentQuery.SortAttribute.DocumentDate, false);

                //docQuery.AddSort(DocumentQuery.SortAttribute.DocumentDate, false);
                //docQuery.AddSort(DocumentQuery.SortAttribute.DocumentID, false);

                // Izvrši upit dobijamo listu dokumenata
                _logger.Info(string.Format("DocumentQuery Execute start {0}.", DateTime.Now));
                int MAX_DOCUMENTS_TO_RETURN = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MAX_DOCUMENTS_TO_RETURN"]);
                DocumentList documentList = docQuery.Execute(MAX_DOCUMENTS_TO_RETURN);
                _logger.Info(string.Format("DocumentQuery Execute end {0}.", DateTime.Now));

                // Pokupi iz konfiguracije Web klijent adresu
                var webServerURL = System.Configuration.ConfigurationManager.AppSettings["OnBaseDocPop"];
                var docPopKeyword = System.Configuration.ConfigurationManager.AppSettings["OnBaseDocPopKeyword"];
                //

                // „Trči“ po listi i kreira response
                _logger.Info(string.Format("Pravimo putanju do fajla start {0}.", DateTime.Now));
                var unityUDF = new Algotech.UTL.AlgotechUnityUDF.UnityUDF();
                //var diskGroupID = Convert.ToInt64(System.Configuration.ConfigurationManager.AppSettings["DiskGroupID"]); // Bankarska dokumentacija

                var list = new List<DocumentsList>();
                foreach (var item in documentList)
                {
                    // Podaci Dokumenta
                    var objDocumentsList = new DocumentsList
                    {
                        DocumentID = item.ID.ToString(),
                        Name = item.Name,
                        DateStored = item.DateStored,
                        DocumentDate = item.DocumentDate,

                        //DocumentFullPath = unityUDF.GetFullPath(diskGroupID, item.ID),
                        DocumentFullPath = unityUDF.GetFullPath(item.ID),

                        DocumentUrlByID = webServerURL + string.Format(@"&docid={0}", item.ID),
                        DefaultFileTypeName = item.DefaultFileType.Name,
                        DocumentTypeName = item.DocumentType.Name
                    };
                    //.

                    // Podaci revizije
                    _logger.Info("Poslednja revizija dokumenta");
                    Revision latestRevision = item.LatestRevision;
                    if (latestRevision != null)
                    {
                        objDocumentsList.RevisionID = latestRevision.ID;
                        objDocumentsList.RevisionDate = latestRevision.Date;
                    }
                    
                    //.

                    // Dodajemo ključne reči
                    _logger.Info(string.Format("Dodajemo ključne reči start {0}.", DateTime.Now));
                    Dictionary<string, string> keywordList = new Dictionary<string, string>();
                    foreach (KeywordRecord keywordRecords in item.KeywordRecords)
                    {
                        foreach (Keyword keyword in keywordRecords.Keywords)
                        {
                            if (!keyword.IsBlank)
                            {
                                if (keyword.KeywordType.Name == "GroupBarcode")
                                    objDocumentsList.GroupBarcode = keyword.Value.ToString();

                                if (keyword.KeywordType.Name == "DocBarcode")
                                {
                                    objDocumentsList.DocBarcode = keyword.Value.ToString();
                                    objDocumentsList.DocumentUrlByKeyword = webServerURL + string.Format(@"&{0}_0_0_0={1}&keytype=", docPopKeyword, objDocumentsList.DocBarcode);
                                }

                                if (keyword.KeywordType.Name == "GroupType")
                                    objDocumentsList.GroupType = keyword.Value.ToString();
                                if (keyword.KeywordType.Name == "GroupID")
                                    objDocumentsList.GroupID = keyword.Value.ToString();

                                if (keyword.KeywordType.Name == "DocID")
                                    objDocumentsList.DocID = keyword.Value.ToString();

                                if (keyword.KeywordType.Name == "ClientID")
                                    objDocumentsList.ClientID = keyword.Value.ToString();
                                if (keyword.KeywordType.Name == "ClientName")
                                    objDocumentsList.ClientName = keyword.Value.ToString();

                                if (keyword.KeywordType.Name == "DataSource")
                                    objDocumentsList.DataSource = keyword.Value.ToString();
                                if (keyword.KeywordType.Name == "DocRegDate")
                                    objDocumentsList.DocRegDate = Convert.ToDateTime(keyword.Value);
                                if (keyword.KeywordType.Name == "DocStatus")
                                    objDocumentsList.DocStatus = keyword.Value.ToString();
                            }
                        }
                    }
                    _logger.Info(string.Format("Dodajemo ključne reči end {0}.", DateTime.Now));
                    //.

                    list.Add(objDocumentsList);
                }
                _logger.Info(string.Format("Pravimo putanju do fajla start {0}.", DateTime.Now));
                //.

                // Formatiranje
                var jsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
                jsonSerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                jsonSerializerSettings.Formatting = Newtonsoft.Json.Formatting.None;
                var dateFormat = System.Configuration.ConfigurationManager.AppSettings["DateFormat"];
                if (dateFormat == "IsoDateFormat")
                {
                    jsonSerializerSettings.Culture = CultureInfo.CurrentCulture;
                    jsonSerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
                    jsonSerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Unspecified;
                }
                //.

                // Serijalizacija
                //var json = JsonConvert.SerializeObject(list, Formatting.None);
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(list, jsonSerializerSettings);
                //.

                // Dodaj root
                json = "{\"documents\":" + json + "}";

                // Response bez "/" https://stackoverflow.com/questions/13833900/return-json-but-it-includes-backward-slashes-which-i-dont-want
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                //.
            }

            catch (Exception ex)
            {
                // Logger
                _logger.Error("Greška preuzimaja podataka");
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                result = new Result
                {
                    Status = string.Empty,
                    Message = ex.Message
                };
                response = Request.CreateResponse(HttpStatusCode.BadRequest, result);
            }
            finally
            {
                if (application != null)
                {
                    _logger.Info("OnBase Disconnect()");
                    var obj = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                    obj.OnBaseDisconnect(application);
                }

                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
                _logger.Info("Metoda:: DocumentList finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());
            }
            return response;
        }
        #endregion
    }
}
