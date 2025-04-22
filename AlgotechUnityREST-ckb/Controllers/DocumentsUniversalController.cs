// Predrag Živić
// 01.03.2025
// Univerzalni data import i univerzalno listanje dokumenata

using System;
using System.Web.Http;
using System.Text;

using Hyland.Unity;


using System.Net.Http;
using System.Net;

using AlgotechUnityREST_ckb.Models;

using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Algotech.UTL.AlgotechUnityUDF;

namespace AlgotechUnityREST.Controllers
{
    /// <summary>
    /// Univerzalnu metodu REST servisa za uvoz dokumenata koja koristi generičke meta podatke. 
    /// Metoda treba da prihvati fajl dokumenta iz opsega podržanih OnBase tipova (PDF, Image File...) zajedno sa kolekcijom ključnih reči kao ulaznim parametrima, 
    /// omogućavajući fleksibilan i prilagodljiv import u sistem.
    /// </summary>
    public class DocumentsUniversalController : ApiController
    {
        #region Klasa
        /// <summary>
        /// Dokument
        /// </summary>
        public class DocumentsImport
        {
            /// <summary>
            /// OnBase Tip dokumenta
            /// </summary>
            [Required]
            public string DocumentTypeName { get; set; }

            /// <summary>
            /// Datum dokumenta
            /// </summary>
            [Required]
            public DateTime? DocumentDate { get; set; } = DateTime.Now;

            /// <summary>
            /// OnBase Tip datoteke
            /// </summary>
            [Required]
            public string DefaultFileTypeName { get; set; } = "PDF";

            /// <summary>
            /// OnBase komentar dokumenta
            /// </summary>
            public string Comment { get; set; }

            /// <summary>
            /// Lista datoteka za import
            /// </summary>
            [Required]
            public List<string> FileName { get; set; }

            /// <summary>
            /// Lista datoteka za import
            /// </summary>
            [Required]
            public string DataSource { get; set; }
            /// <summary>
            /// Lista ključnih reči
            /// </summary>
            [Required]
            public List<Keyword> Keywords { get; set; }

            /// <summary>
            /// Jedinistveni identifikator dokumenta
            /// </summary>
            public long DocumentID { get; set; }
            /// <summary>
            /// NAziv dokumenta
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Kada je dokument skladišten
            /// </summary>
            public DateTime DateStored { get; set; }
            /// <summary>
            /// Puna putanja do datoteke
            /// </summary>
            public string DocumentFullPath { get; internal set; }
            /// <summary>
            /// Putanja do dokuenta po ID
            /// </summary>
            public string DocumentUrlByID { get; internal set; }

            /// <summary>
            /// Broj revizije
            /// </summary>
            public long RevisionID { get; set; }
            /// <summary>
            /// Datum revizije
            /// </summary>
            public DateTime RevisionDate { get; set; }


            /// <summary>
            /// Kolekcija ključnih reči pridruženih dokumentu
            /// </summary>
            public Dictionary<string, string> KeywordList { get; set; }
        }

        /// <summary>
        /// Ključne reči
        /// </summary>
        public class Keyword
        {
            /// <summary>
            /// Nazic OnBase ključne reču
            /// </summary>
            [Required]
            public string KeywordName { get; set; }
            /// <summary>
            /// Vrednost ključne reči
            /// </summary>
            [Required]
            public string KeywordValue { get; set; }

            /// <summary>
            /// Tip podatka
            /// </summary>
            public string KeywordDataType { get; set; } = "string";
        }
        #endregion

        #region Variable  
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //private static HttpResponseMessage response;
        #endregion

        #region DocumentImport
        /// <summary>
        /// Preuzimanje datoteke i unos u OnBase
        /// </summary>
        /// <param name="documentImport">Objekat Documents</param>
        /// <returns></returns>
        [HttpPost, ActionName("DocumentImport")]
        public HttpResponseMessage DocumentImport([FromBody] DocumentsImport documentImport)
        {
            HttpResponseMessage responseMessage;
            Application application = null;

            var dateTimeStart = DateTime.Now;

            var httpStatusCode = HttpStatusCode.BadRequest;
            var result = new Result();
            responseMessage = Request.CreateResponse(httpStatusCode, result);

            try
            {
                _logger.Info("Metoda:: DocumentImport Univerzalni");

                var status = string.Empty;
                var message = string.Empty;

                // Provera da li su vrednosti prosleđene
                if (documentImport == null) { throw new ArgumentException("Niste definisali Dokument!"); }
                //if (!documentImport.DocumentDate.HasValue) { throw new ArgumentException("Niste definisali datum dokumenta!");               }

                if (string.IsNullOrEmpty(documentImport.DocumentTypeName)) { throw new ArgumentException("Niste definisali OnBase tip dokumenta!"); }
                if (string.IsNullOrEmpty(documentImport.DefaultFileTypeName)) { throw new ArgumentException("Niste definisali tip datoteke!"); }
                if (string.IsNullOrEmpty(documentImport.DataSource)) { throw new ArgumentException("Niste definisali izvor podatka!"); }

                if (documentImport.FileName == null) { throw new ArgumentException("Niste definisali datoteku!"); }
                if (documentImport.Keywords == null) { throw new ArgumentException("Niste definisali ključne reči!"); }
                //.

                // Ključne reči definicija
                Console.WriteLine($"Received document: {documentImport.DocumentTypeName}");
                foreach (var keyword in documentImport.Keywords)
                {
                    Debug.WriteLine($"KeywordName: {keyword.KeywordName} Value: {keyword.KeywordValue} DataType: {keyword.KeywordDataType}");
                    _logger.Info($"KeywordName: {keyword.KeywordName} Value: {keyword.KeywordValue} DataType: {keyword.KeywordDataType}");
                }
                //.

                // Konekcija ona OnBase
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
                _logger.Info("DocumentImport start " + dateTimeStart);

                // Validacija Tipa dokumenta
                DocumentType documentType =
                    application.Core.DocumentTypes.Find(documentImport.DocumentTypeName) ?? throw new Exception(string.Format("Tip dokumenta '{0}' nije pronađen ili nisu podešena prava pristupa servisnom nalogu.", documentImport.DocumentTypeName));
                FileType fileType =
                    application.Core.FileTypes.Find(documentImport.DefaultFileTypeName) ?? throw new Exception(string.Format("Tip datoteke '{0}' nije pronađen ili nisu podešena prava pristupa servisnom nalogu.", documentImport.DefaultFileTypeName));
                //.

                // Svojstva dokumenta
                StoreNewDocumentProperties storeDocumentProperties =
                    application.Core.Storage.CreateStoreNewDocumentProperties(documentType, fileType);

                storeDocumentProperties.TextEncoding = Encoding.Unicode;
                storeDocumentProperties.DocumentDate = documentImport.DocumentDate;
                storeDocumentProperties.Comment = documentImport.Comment;
                //storeDocumentProperties.Options = StoreDocumentOptions.SkipWorkflow;
                //.

                // Pridružujemo datasource kolekciji ključnih reči 
                documentImport.Keywords.Add(new Keyword { KeywordName = "DataSource", KeywordValue = documentImport.DataSource });

                //// Upisujemo ključne reči 
                //foreach (var keyword in documentImport.Keywords)
                //{
                //    if (!string.IsNullOrEmpty(keyword.KeywordName))
                //        storeDocumentProperties.AddKeyword(keyword.KeywordName, keyword.KeywordValue);
                //}
                ////.

                // Upisujemo ključne reči 
                foreach (var keyword in documentImport.Keywords)
                {
                    if (!string.IsNullOrEmpty(keyword.KeywordName))
                    {
                        // Treba nam id ključne reči i tip podatka
                        //var keywordTypeNameID = UnityUDF.GetKeywordTypeID(application, keyword.KeywordName);
                        var keywordDataType = UnityUDF.GetKeywordDataType(application, keyword.KeywordName);

                        // Konvertovati Hyland.Unity data type u C#
                        switch (keywordDataType)
                        {
                            case KeywordDataType.Numeric9:
                                storeDocumentProperties.AddKeyword(keyword.KeywordName, Convert.ToInt32(keyword.KeywordValue));
                                break;

                            case KeywordDataType.Numeric20:
                                storeDocumentProperties.AddKeyword(keyword.KeywordName, Convert.ToInt64(keyword.KeywordValue));
                                break;

                            case KeywordDataType.AlphaNumeric:
                                storeDocumentProperties.AddKeyword(keyword.KeywordName, Convert.ToString(keyword.KeywordValue));
                                break;

                            case KeywordDataType.Currency:
                                storeDocumentProperties.AddKeyword(keyword.KeywordName, Convert.ToDecimal(keyword.KeywordValue));
                                break;

                            case KeywordDataType.Date:
                                storeDocumentProperties.AddKeyword(keyword.KeywordName, Convert.ToDateTime(keyword.KeywordValue));
                                break;

                            case KeywordDataType.DateTime:
                                storeDocumentProperties.AddKeyword(keyword.KeywordName, Convert.ToDateTime(keyword.KeywordValue));
                                break;

                            case KeywordDataType.FloatingPoint:
                                storeDocumentProperties.AddKeyword(keyword.KeywordName, Convert.ToDecimal(keyword.KeywordValue));
                                break;

                            default:
                                storeDocumentProperties.AddKeyword(keyword.KeywordName, Convert.ToString(keyword.KeywordValue));
                                break;
                        }
                        Debug.WriteLine("Kriterijum pretrage Keyword type.");
                    }
                }

                // Skladišti novi dokument
                var newDocument = application.Core.Storage.StoreNewDocument(documentImport.FileName, storeDocumentProperties);
                //.

                // Vrati id unetog dokumenta / Return the id of the entered document
                status = newDocument.ID.ToString();
                message = string.Format("Dokument je uspešno sačuvan DocumentID #{0}.", status);

                //.
                if (status != string.Empty)
                {
                    result = new Result
                    {
                        Status = status,
                        Message = message
                    };

                    responseMessage = Request.CreateResponse(HttpStatusCode.OK, result);
                    _logger.Info(result.Message);
                }
            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error(string.Format("Greška kod importovanja dokumenta DocumentTypeName: {0}", documentImport.DocumentTypeName));
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                // Formiraj response
                result = new Result
                {
                    Status = string.Empty,
                    Message = ex.Message
                };
                responseMessage = Request.CreateResponse(httpStatusCode, result);
                //.
            }
            finally
            {
                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
                _logger.Info("DocumentImport finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());
                if (application != null)
                {
                    _logger.Info("OnBase Disconnect()");
                    var obj = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                    obj.OnBaseDisconnect(application);
                }
            }
            return responseMessage;
        }
        #endregion

        #region DocumentList
        /// <summary>
        /// Vraća listu dokumenata za prosleđene parametre. Parametri su opcioni, mogu se kombinovati i mora se proslediti bar jedan parametar.
        /// </summary>
        /// <param name="fromDate">Datum kreiranja dokumenta od</param>
        /// <param name="toDate">Datum kreiranja dokumenta do</param>
        /// <param name="documentTypeName">Naziv tipa dokumenta u OnBase sistemu</param>
        /// <param name="keywordName">Naziv ključne reči za pretragu</param>
        /// <param name="keywordValue">Vrednost klučne reči za pretragu</param>
        /// <returns></returns>
        [HttpGet, ActionName("DocumentList")]
        public HttpResponseMessage DocumentList(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string documentTypeName = "",
            string keywordName = "",
            string keywordValue = ""
            )
        {
            Application application = null;
            Result result;
            HttpResponseMessage response;

            var dateTimeStart = DateTime.Now;

            try
            {
                // Provera ulaznih parametara pretrage
                if (!fromDate.HasValue &&
                    !toDate.HasValue &&
                    documentTypeName == string.Empty &&
                    keywordName == string.Empty &&
                    keywordValue == string.Empty)
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

                //Debug.WriteLine(string.Format("Parametri pretrage fromDate: {0} toDate: {1} documentTypeName: {2} clientID: {3} docBarcode {4} groupBarcode {5}.", fromDate, toDate, documentTypeName, clientID, docBarcode, groupBarcode));
                //_logger.Info(string.Format("Parametri pretrage fromDate: {0} toDate: {1} documentTypeName: {2} clientID: {3} docBarcode {4} groupBarcode {5}.", fromDate, toDate, documentTypeName, clientID, docBarcode, groupBarcode));

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
                if (keywordName != string.Empty)
                {
                    docQuery.AddKeyword(keywordName, keywordValue, KeywordOperator.Equal, KeywordRelation.Or);
                }
                //.

                // Dodajemo sort po datumu dokumenta silazno (descending)
                docQuery.AddSort(DocumentQuery.SortAttribute.DocumentDate, false);


                // Izvrši upit dobijamo listu dokumenata
                _logger.Info(string.Format("DocumentQuery Execute start {0}.", DateTime.Now));
                int MAX_DOCUMENTS_TO_RETURN = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MAX_DOCUMENTS_TO_RETURN"]);
                DocumentList documentList = docQuery.Execute(MAX_DOCUMENTS_TO_RETURN);
                _logger.Info(string.Format("DocumentQuery Execute end {0}.", DateTime.Now));

                // Pokupi iz konfiguracije Web klijent adresu
                var webServerURL = System.Configuration.ConfigurationManager.AppSettings["OnBaseDocPop"];
                var docPopKeyword = System.Configuration.ConfigurationManager.AppSettings["OnBaseDocPopKeyword"];
                //

                // Trči po listi i kreira response
                _logger.Info(string.Format("Pravimo putanju do fajla start {0}.", DateTime.Now));
                var unityUDF = new Algotech.UTL.AlgotechUnityUDF.UnityUDF();
                var diskGroupID = Convert.ToInt64(System.Configuration.ConfigurationManager.AppSettings["DiskGroupID"]); // Bankarska dokumentacija

                var list = new List<DocumentsImport>();
                foreach (var item in documentList)
                {
                    // Podaci Dokumenta
                    var objDocumentsList = new DocumentsImport
                    {
                        DocumentID = item.ID,
                        Name = item.Name,
                        DateStored = item.DateStored,
                        DocumentDate = item.DocumentDate,
                        DocumentFullPath = unityUDF.GetFullPath(diskGroupID, item.ID),
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
                        foreach (Hyland.Unity.Keyword keyword in keywordRecords.Keywords)
                        {
                            if (!keyword.IsBlank)
                            {
                                //if (keyword.KeywordType.Name == "DataSource")
                                //    objDocumentsList.DataSource = keyword.Value.ToString();

                                // Konvertovati Hyland.Unity data type u C# trebaće nam možda ovakav kod zbog formatiranja vrednosti
                                switch (keyword.KeywordType.DataType)
                                {
                                    //case KeywordDataType.Numeric9:
                                    //    keywordList.Add(keyword.KeywordType.Name, keyword.Numeric9Value.ToString());
                                    //    break;

                                    //case KeywordDataType.Numeric20:
                                    //    keywordList.Add(keyword.KeywordType.Name, keyword.Numeric20Value.ToString());
                                    //    break;

                                    //case KeywordDataType.AlphaNumeric:
                                    //    keywordList.Add(keyword.KeywordType.Name, keyword.AlphaNumericValue.ToString());
                                    //    break;

                                    //case KeywordDataType.Currency:
                                    //    keywordList.Add(keyword.KeywordType.Name, keyword.CurrencyValue.ToString());
                                    //    break;

                                    case KeywordDataType.Date:
                                        //keywordList.Add(keyword.KeywordType.Name, keyword.DateTimeValue.ToString("G", CultureInfo.CurrentCulture));
                                        keywordList.Add(keyword.KeywordType.Name, keyword.DateTimeValue.ToString("s"));
                                        break;

                                    case KeywordDataType.DateTime:
                                        //keywordList.Add(keyword.KeywordType.Name, keyword.DateTimeValue.ToString("G", CultureInfo.CurrentCulture));
                                        keywordList.Add(keyword.KeywordType.Name, keyword.DateTimeValue.ToString("s", CultureInfo.CurrentCulture));

                                        _logger.Debug(string.Format(
                                            "Datum: {0}, " +
                                            "ToString('o'): {1}, " +
                                            "ToString('s'): {2}, " +
                                            "ToString('G'): {3}, " +
                                            "ToString('yyyy-MM-ddTHH:mm:ssK'): {4}, " +
                                            "ToString(): {5} " +
                                            "ToLocalTime: {6},  " +
                                            "DateTimeFormatInfo.CurrentInfo: {7}",
                                            keyword.DateTimeValue,
                                            keyword.DateTimeValue.ToString("o"),
                                            keyword.DateTimeValue.ToString("s"),
                                            keyword.DateTimeValue.ToString("G"),
                                            keyword.DateTimeValue.ToString("yyyy-MM-ddTHH:mm:ssK"),
                                            keyword.DateTimeValue.ToString(),
                                            keyword.DateTimeValue.ToLocalTime(),
                                            DateTimeFormatInfo.CurrentInfo.ToString())); ;
                                        break;

                                    //case KeywordDataType.FloatingPoint:
                                    //    keywordList.Add(keyword.KeywordType.Name, keyword.FloatingPointValue.ToString());
                                    //    break;

                                    default:
                                        if (keywordList.ContainsKey(keyword.KeywordType.Name))
                                        {
                                            // Ako već postoji ukloni da bi dodali poeslenju vrednost
                                            keywordList.Remove(keyword.KeywordType.Name);
                                        }
                                        // Dodajemo
                                        keywordList.Add(keyword.KeywordType.Name, keyword.Value.ToString());
                                        break;
                                }
                                //..
                            }


                        }
                    }
                    _logger.Info(string.Format("Dodajemo ključne reči end {0}.", DateTime.Now));
                    //.
                    objDocumentsList.KeywordList = keywordList;

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