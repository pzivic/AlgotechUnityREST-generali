
// ============================================================================
// Autor:           Predrag Živić
// Datum kreiranja: 01.03.2025
// Naziv:           Univerzalni data import i univerzalno listanje dokumenata
//
// Opis:
// Univerzaalna metoda REST servisa za uvoz dokumenata koja koristi generičke meta podatke.
// Metoda treba da prihvati fajl dokumenta iz opsega podržanih OnBase tipova (PDF, Image File...)
// zajedno sa kolekcijom ključnih reči kao ulaznim parametrima, omogućavajući fleksibilan i prilagodljiv import u sistem.
//
// Istorija izmena:
// 30.10.2023 - Predrag Živić - Putaju uzima iz web config fajla za DocumentImport,
// upload pravi guid fajla da ne bi došlo do sudara imena fajla,
// 
// ============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

using Algotech.UTL.AlgotechUnityUDF;
using AlgotechUnityREST_generali.Models;

using Hyland.Unity;

namespace AlgotechUnityREST_generali.Controllers
{
    /// <summary>
    /// Univerzalnu metodu REST servisa za uvoz dokumenata koja koristi generičke meta podatke. 
    /// Metoda treba da prihvati fajl dokumenta iz opsega podržanih OnBase tipova (PDF, Image File...) zajedno sa kolekcijom ključnih reči kao ulaznim parametrima, 
    /// omogućavajući fleksibilan i prilagodljiv import u sistem.
    /// </summary>
    [Authorize]
    public class DocumentsUniversalController : ApiController
    {
        #region Variable  
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

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

        #region DocumentImport_
        ///// <summary>
        ///// Preuzimanje datoteke i unos u OnBase
        ///// </summary>
        ///// <param name="documentImport">Objekat Documents</param>
        ///// <returns></returns>
        //[Authorize]
        //[HttpPost, ActionName("DocumentImport_")]
        //[Route("api/DocumentsUniversal/DocumentImport")]
        //public HttpResponseMessage DocumentImport(
        //    [FromBody] DocumentsImport documentImport)
        //{
        //    return ImportDocumentCore(documentImport);
        //}
        #endregion

        #region DocumentImport
        /// <summary>
        /// DocumentImportWithUpload
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost, ActionName("DocumentImport")]
        [Route("api/DocumentsUniversal/DocumentImport")]
        public HttpResponseMessage DocumentImport()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Files.Count == 0)
                {
                    return Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new Result
                        {
                            Status = string.Empty,
                            Message = "Nije poslata nijedna datoteka."
                        });
                }

                // Klijent šalje JSON kao tekstualno multipart polje "DocumentImport".
                var json = httpRequest.Form["DocumentImport"];

                if (string.IsNullOrWhiteSpace(json))
                {
                    return Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new Result
                        {
                            Status = string.Empty,
                            Message = "Nisu poslati podaci za import."
                        });
                }

                var documentImport =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<DocumentsImport>(json);

                if (documentImport == null)
                {
                    return Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new Result
                        {
                            Status = string.Empty,
                            Message = "Podaci za import nisu ispravni."
                        });
                }

                // Upload se izvršava prvo. Vraćaju se samo jedinstveni nazivi fajlova,
                // bez fizičkih putanja servera.
                documentImport.FileName = SaveUploadedFiles(httpRequest.Files);

                // Poziva istu import logiku koju koristi i postojeći JSON endpoint.
                return ImportDocumentCore(documentImport);
            }
            catch (Exception ex)
            {
                _logger.Error("Greška u orkestraciji uploada i importa.");
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);

                return Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    new Result
                    {
                        Status = string.Empty,
                        Message = ex.Message
                    });
            }
        }
        #endregion

        #region DocumentList
        /// <summary>
        /// Vraća listu dokumenata za prosleđene parametre. Parametri su opcioni, mogu se kombinovati i mora se proslediti bar jedan parametar.
        /// </summary>
        /// <param name="fromDate">Datum kreiranja dokumenta od</param>
        /// <param name="toDate">Datum kreiranja dokumenta do</param>
        /// <param name="documentTypeName">Naziv tipa dokumenta u OnBase sistemu</param>
        /// <param name="dataSource">Izvor podatka</param>
        /// <param name="keywordName">Naziv ključne reči za pretragu</param>
        /// <param name="keywordValue">Vrednost klučne reči za pretragu</param>
        /// <returns></returns>
        [Authorize]
        [HttpGet, ActionName("DocumentList")]
        [Route("api/DocumentsUniversal/DocumentList")]
        public HttpResponseMessage DocumentList(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string documentTypeName = "",
            string dataSource = "",
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
                    throw new ArgumentException("Niste definisali kriterijum pretrage. Definišite [fromDate] i [toDate].");
                }

                if (fromDate.HasValue && !toDate.HasValue)
                {
                    throw new ArgumentException("Niste definisali kriterijum [toDate] a definisali ste kriterijum [fromDate]!");
                }
                if (!fromDate.HasValue && toDate.HasValue)
                {
                    throw new ArgumentException("Niste definisali kriterijum [fromDate] a definisali ste kriterijum [toDate]!");
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
                    DocumentType documentType = 
                        application.Core.DocumentTypes.Find(documentTypeName) ?? 
                        throw new Exception(string.Format("Tip dokumenta '{0}' nije pronađen ili nisu podešena prava pristupa servisnom nalogu.", 
                        documentTypeName));
                    docQuery.AddDocumentType(documentType);
                }

                // Dodaj data source fixno
                if (dataSource != string.Empty)
                {
                    docQuery.AddKeyword("DataSource", dataSource, KeywordOperator.Equal, KeywordRelation.Or);
                }

                // Ključne reči kao filter
                if (keywordName != string.Empty)
                {
                    //docQuery.AddKeyword(keywordName, keywordValue, KeywordOperator.Equal, KeywordRelation.Or);
                    var keywordDataType = UnityUDF.GetKeywordDataType(application, keywordName);
                    // Konvertovati Hyland.Unity data type u C# 
                    switch (keywordDataType)
                    {
                        case KeywordDataType.Numeric9:
                            docQuery.AddKeyword(keywordName, Convert.ToInt32(keywordValue), KeywordOperator.Equal, KeywordRelation.Or);
                            break;

                        case KeywordDataType.Numeric20:
                            docQuery.AddKeyword(keywordName, Convert.ToInt64(keywordValue), KeywordOperator.Equal, KeywordRelation.Or);
                            break;

                        case KeywordDataType.AlphaNumeric:
                            docQuery.AddKeyword(keywordName, Convert.ToString(keywordValue), KeywordOperator.Equal, KeywordRelation.Or);
                            break;

                        case KeywordDataType.Currency:
                            docQuery.AddKeyword(keywordName, Convert.ToDecimal(keywordValue), KeywordOperator.Equal, KeywordRelation.Or);
                            break;

                        case KeywordDataType.SpecificCurrency:
                            docQuery.AddKeyword(keywordName, Convert.ToDecimal(keywordValue), KeywordOperator.Equal, KeywordRelation.Or);
                            break;

                        case KeywordDataType.Date:
                            docQuery.AddKeyword(keywordName, Convert.ToDateTime(keywordValue), KeywordOperator.Equal, KeywordRelation.Or);
                            break;

                        case KeywordDataType.DateTime:
                            docQuery.AddKeyword(keywordName, Convert.ToDateTime(keywordValue), KeywordOperator.Equal, KeywordRelation.Or);
                            break;

                        case KeywordDataType.FloatingPoint:
                            docQuery.AddKeyword(keywordName, Convert.ToDecimal(keywordValue), KeywordOperator.Equal, KeywordRelation.Or);
                            break;

                        default:
                            docQuery.AddKeyword(keywordName, Convert.ToString(keywordValue), KeywordOperator.Equal, KeywordRelation.Or);
                            break;
                    }
                    //..
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
                //var diskGroupID = Convert.ToInt64(System.Configuration.ConfigurationManager.AppSettings["DiskGroupID"]); // Bankarska dokumentacija

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
                        foreach (Hyland.Unity.Keyword keyword in keywordRecords.Keywords)
                        {
                            if (!keyword.IsBlank)
                            {
                                //if (keyword.KeywordType.Name == "DataSource")
                                //    objDocumentsList.DataSource = keyword.Value.ToString();

                                // Konvertovati Hyland.Unity data type u C# trebaće nam možda ovakav kod zbog formatiranja vrednosti
                                switch (keyword.KeywordType.DataType)
                                {
                                    // LIsta je string ne mogu orginilanu vrednost da mu dam 
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

                                    //case KeywordDataType.SpecificCurrency:
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

                                    case KeywordDataType.FloatingPoint:
                                        keywordList.Add(keyword.KeywordType.Name, keyword.FloatingPointValue.ToString());
                                        break;

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

        #region SaveUploadedFiles
        private List<string> SaveUploadedFiles(HttpFileCollection files)
        {
            var uploadedFolder =
                System.Configuration.ConfigurationManager.AppSettings["UploadFolder"];

            if (string.IsNullOrWhiteSpace(uploadedFolder))
            {
                throw new InvalidOperationException(
                    "Nije podešena lokacija za privremeni upload.");
            }

            Directory.CreateDirectory(uploadedFolder);

            var uploadedFileNames = new List<string>();

            foreach (string key in files)
            {
                var postedFile = files[key];

                if (postedFile == null || string.IsNullOrWhiteSpace(postedFile.FileName))
                {
                    throw new ArgumentException("Naziv poslate datoteke nije ispravan.");
                }

                var originalFileName = Path.GetFileName(postedFile.FileName);
                var extension = Path.GetExtension(originalFileName);

                var uniqueFileName =
                    $"{Path.GetFileNameWithoutExtension(originalFileName)}_" +
                    $"{Guid.NewGuid():N}{extension}";

                var filePath = Path.Combine(uploadedFolder, uniqueFileName);

                postedFile.SaveAs(filePath);
                uploadedFileNames.Add(uniqueFileName);
            }

            return uploadedFileNames;
        }
        #endregion

        #region ImportDocumentCore
        private HttpResponseMessage ImportDocumentCore(DocumentsImport documentImport)
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

                if (documentImport == null)
                {
                    throw new ArgumentException("Niste definisali dokument!");
                }

                if (string.IsNullOrEmpty(documentImport.DocumentTypeName))
                {
                    throw new ArgumentException("Niste definisali OnBase tip dokumenta!");
                }

                if (string.IsNullOrEmpty(documentImport.DefaultFileTypeName))
                {
                    throw new ArgumentException("Niste definisali tip datoteke!");
                }

                if (string.IsNullOrEmpty(documentImport.DataSource))
                {
                    throw new ArgumentException("Niste definisali izvor podatka!");
                }

                if (documentImport.FileName == null || documentImport.FileName.Count == 0)
                {
                    throw new ArgumentException("Niste definisali datoteku.");
                }

                if (documentImport.Keywords == null)
                {
                    throw new ArgumentException("Niste definisali ključne reči!");
                }

                // Formiranje pune putanje za privremeno prenete datoteke
                var uploadedFolder =
                    System.Configuration.ConfigurationManager.AppSettings["UploadFolder"];

                if (string.IsNullOrWhiteSpace(uploadedFolder))
                {
                    httpStatusCode = HttpStatusCode.InternalServerError;

                    throw new InvalidOperationException(
                        "Nije podešena lokacija za privremeni upload.");
                }

                var fullFilePaths = new List<string>();

                foreach (var uploadedFileName in documentImport.FileName)
                {
                    var fileName = Path.GetFileName(uploadedFileName);

                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        throw new ArgumentException(
                            "Naziv prenete datoteke nije ispravan.");
                    }

                    var fullPath = Path.Combine(uploadedFolder, fileName);

                    if (!System.IO.File.Exists(fullPath))
                    {
                        throw new FileNotFoundException(
                            $"Preneta datoteka '{fileName}' nije pronađena.");
                    }

                    fullFilePaths.Add(fullPath);
                }

                documentImport.FileName = fullFilePaths;

                // Beleženje prosleđenih ključnih reči
                Console.WriteLine($"Received document: {documentImport.DocumentTypeName}");

                foreach (var keyword in documentImport.Keywords)
                {
                    if (keyword == null)
                    {
                        continue;
                    }

                    Debug.WriteLine(
                        $"KeywordName: {keyword.KeywordName} " +
                        $"Value: {keyword.KeywordValue} " +
                        $"DataType: {keyword.KeywordDataType}");

                    _logger.Info(
                        $"KeywordName: {keyword.KeywordName} " +
                        $"Value: {keyword.KeywordValue} " +
                        $"DataType: {keyword.KeywordDataType}");
                }

                // Konekcija na OnBase
                _logger.Info("OnBase Connect()");

                var connection = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                application = connection.OnBaseConnect();

                if (application == null)
                {
                    _logger.Warn("Greška kod uspostavljanja konekcije sa OnBase");

                    throw new ArgumentException(
                        "Greška kod uspostavljanja konekcije sa OnBase. " +
                        "Kontaktirajte OnBase administratora.");
                }

                dateTimeStart = DateTime.Now;
                _logger.Info("DocumentImport start " + dateTimeStart);

                // Validacija tipa dokumenta i tipa datoteke
                DocumentType documentType =
                    application.Core.DocumentTypes.Find(documentImport.DocumentTypeName)
                    ?? throw new Exception(
                        string.Format(
                            "Tip dokumenta '{0}' nije pronađen ili nisu podešena prava pristupa servisnom nalogu.",
                            documentImport.DocumentTypeName));

                FileType fileType =
                    application.Core.FileTypes.Find(documentImport.DefaultFileTypeName)
                    ?? throw new Exception(
                        string.Format(
                            "Tip datoteke '{0}' nije pronađen ili nisu podešena prava pristupa servisnom nalogu.",
                            documentImport.DefaultFileTypeName));

                // Svojstva dokumenta
                StoreNewDocumentProperties storeDocumentProperties =
                    application.Core.Storage.CreateStoreNewDocumentProperties(
                        documentType,
                        fileType);

                storeDocumentProperties.TextEncoding = Encoding.Unicode;
                storeDocumentProperties.DocumentDate = documentImport.DocumentDate;
                storeDocumentProperties.Comment = documentImport.Comment;

                // Upis ostalih ključnih reči.
                // DataSource se obrađuje posebno, da bi se uvek upisala vrednost
                // iz polja documentImport.DataSource i da ne bi bilo duplikata.
                foreach (var keyword in documentImport.Keywords)
                {
                    if (keyword == null ||
                        string.IsNullOrWhiteSpace(keyword.KeywordName) ||
                        string.Equals(
                            keyword.KeywordName,
                            "DataSource",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var keywordDataType =
                        UnityUDF.GetKeywordDataType(
                            application,
                            keyword.KeywordName);

                    switch (keywordDataType)
                    {
                        case KeywordDataType.Numeric9:
                            storeDocumentProperties.AddKeyword(
                                keyword.KeywordName,
                                Convert.ToInt32(keyword.KeywordValue));
                            break;

                        case KeywordDataType.Numeric20:
                            storeDocumentProperties.AddKeyword(
                                keyword.KeywordName,
                                Convert.ToInt64(keyword.KeywordValue));
                            break;

                        case KeywordDataType.AlphaNumeric:
                            storeDocumentProperties.AddKeyword(
                                keyword.KeywordName,
                                Convert.ToString(keyword.KeywordValue));
                            break;

                        case KeywordDataType.Currency:
                        case KeywordDataType.SpecificCurrency:
                        case KeywordDataType.FloatingPoint:
                            storeDocumentProperties.AddKeyword(
                                keyword.KeywordName,
                                Convert.ToDecimal(keyword.KeywordValue));
                            break;

                        case KeywordDataType.Date:
                        case KeywordDataType.DateTime:
                            storeDocumentProperties.AddKeyword(
                                keyword.KeywordName,
                                Convert.ToDateTime(keyword.KeywordValue));
                            break;

                        default:
                            storeDocumentProperties.AddKeyword(
                                keyword.KeywordName,
                                Convert.ToString(keyword.KeywordValue));
                            break;
                    }
                }

                // Dodavanje DataSource tačno jednom, iz posebnog polja zahteva
                storeDocumentProperties.AddKeyword(
                    "DataSource",
                    documentImport.DataSource);

                // Skladištenje novog dokumenta
                var newDocument = application.Core.Storage.StoreNewDocument(
                    documentImport.FileName,
                    storeDocumentProperties);

                var status = newDocument.ID.ToString();
                var message =
                    string.Format(
                        "Dokument je uspešno sačuvan DocumentID #{0}.",
                        status);

                result = new Result
                {
                    Status = status,
                    Message = message
                };

                responseMessage = Request.CreateResponse(
                    HttpStatusCode.OK,
                    result);

                _logger.Info(result.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    string.Format(
                        "Greška kod importovanja dokumenta DocumentTypeName: {0}",
                        documentImport?.DocumentTypeName ?? "(nije prosleđen)"));

                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);

                result = new Result
                {
                    Status = string.Empty,
                    Message = ex.Message
                };

                responseMessage = Request.CreateResponse(
                    httpStatusCode,
                    result);
            }
            finally
            {
                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;

                _logger.Info(
                    "DocumentImport finally " +
                    dateTimeEnd +
                    " sec " +
                    diffInSeconds);

                if (application != null)
                {
                    _logger.Info("OnBase Disconnect()");

                    var connection =
                        new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();

                    connection.OnBaseDisconnect(application);
                }
            }

            return responseMessage;
        }
        #endregion

        #region DocumentProperties
        /// <summary>
        /// Privatna klasa za prikaz podataka o dokumentu
        /// </summary>
        [Newtonsoft.Json.JsonObject(Title = "DocumentProperites")]
        
        private class DocumentProperites
        {
            //[JsonProperty(PropertyName = "id")]
            public long DocumentID { get; set; }
            public string Name { get; set; }
            public DateTime DocumentDate { get; set; }
            public DateTime DateStored { get; set; }
            public string DocumentTypeName { get; set; }
            public string DefaultFileTypeName { get; set; }
            public string Status { get; set; }
            public string CreatedBy { get; set; }

            public string DocumentUrlByID { get; set; }
            public string DocumentUrlByKeyword { get; set; }
            public string DocumentFullPath { get; set; }
            //public byte[] DocumentFileStream { get; set; }

            public long RevisionID { get; set; }
            public DateTime RevisionDate { get; set; }
            public string RevisionComment { get; set; }
            public string RevisionCreatedByName { get; set; }

            public Dictionary<string, string> KeywordList { get; set; }
        }

        /// <summary>
        /// Za prosleđeni documentID vraća set podataka o dokumentu
        /// </summary>
        /// <param name="documentID">Identifikator dokumenta u OnBase sistemu</param>
        /// <param name="docBarcode">Alternativni ključ dokumenta</param>
        /// <returns></returns>
        /// <remarks>
        ///</remarks>
        /// <response code="201">Vraća objekat Documents</response>
        /// <response code="400">Ako je element null</response>
        [HttpGet, ActionName("DocumentProperties")]
        [Route("api/DocumentsUniversal/DocumentProperties")]
        public HttpResponseMessage DocumentProperties(long documentID = 0, string docBarcode = "")
        {
            HttpResponseMessage responseMessage;
            Application application = null;

            // Defalt error code
            var httpStatusCode = HttpStatusCode.Forbidden;

            // Inicijalizacija promenljive za rezultat (new 07.06.2024))
            var result = new Result();
            responseMessage = Request.CreateResponse(httpStatusCode, result);
            //.

            try
            {
                //if (docBarcode == string.Empty) { throw new ArgumentException("Parametar docBarcode mora da bude prosleđen"); }

                // Konektuj OnBase
                _logger.Debug("OnBase Connect()");
                var obj = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                application = obj.OnBaseConnect();
                if (application == null)
                {
                    httpStatusCode = HttpStatusCode.InternalServerError;
                    _logger.Warn("Greška kod uspostavljanja konekcije sa OnBase");
                    throw new ArgumentException("Greška kod uspostavljanja konekcije sa OnBase. Kontaktirajte OnBase administratora.");
                }
                //.

                _logger.Debug("Metoda:: DocumentProperties");

                var unityUDF = new Algotech.UTL.AlgotechUnityUDF.UnityUDF();

                if (documentID == 0)
                {
                    // Može da bude NotFound a može i BadRequest u DocBarcode2DocumentID
                    //httpStatusCode = HttpStatusCode.NotFound;
                    documentID = unityUDF.DocBarcode2DocumentID(application, docBarcode);
                }

                _logger.Debug(string.Format("Pokušaj preuzimanja svojstava dokumenta DocumentID: {0}", documentID));

                Document document = application.Core.GetDocumentByID(documentID);
                if (document == null)
                {
                    httpStatusCode = HttpStatusCode.NotFound;
                    _logger.Warn(string.Format("Dokument DocumentID: {0} nije pronađen!", documentID));
                    throw new Exception("Dokument nije pronađen");
                }

                //// Zaključavanje dokumenta (u principu nema potrebe da bih samo gledao file)
                //var forceLockDocument = System.Configuration.ConfigurationManager.AppSettings["ForceLockDocument"] == "True";
                //if (forceLockDocument == true)
                //{
                //    _logger.Debug(string.Format("Zaključavanje dokumenta : {0}.", documentID));
                //    using (DocumentLock documentLock = document.LockDocument())
                //    {
                //        if (documentLock.Status != DocumentLockStatus.LockObtained)
                //        {

                //            _logger.Warn(string.Format("Zaključavanje dokumenta DocumentID: {0} nije uspelo!", documentID));
                //            throw new Exception("Failed to lock document");
                //        }
                //    }
                //}
                ////.

                // 1. Sistemska svojstva dokumenta
                _logger.Debug("Sistemska svojstva dokumenta");
                var objDocuments = new DocumentProperites()
                {
                    DocumentID = document.ID,
                    Name = document.Name,
                    DocumentDate = document.DocumentDate,
                    DateStored = document.DateStored,
                    DocumentTypeName = document.DocumentType.Name,
                    DefaultFileTypeName = document.DefaultFileType.Name,
                    CreatedBy = document.CreatedBy.Name,
                    Status = document.Status.ToString(),
                };

                _logger.Debug("Dodatna svojstva");
                var webServerURL = System.Configuration.ConfigurationManager.AppSettings["OnBaseDocPop"];
                objDocuments.DocumentUrlByID = webServerURL + string.Format(@"&docid={0}", objDocuments.DocumentID);

                // Ne znamo u kojoj je disk grupi da bi vratili putanju diskgrupe
                //var diskGroupID = Convert.ToInt64(System.Configuration.ConfigurationManager.AppSettings["DiskGroupID"]);

                //_logger.Debug(string.Format("Pokušaćemo da dohvatilo putanju do datoteke diskGroupID: {0}, documentID: {1}", diskGroupID, documentID));
                _logger.Debug(string.Format("Pokušaćemo da dohvatilo putanju do datoteke documentID: {0}", documentID));
                //objDocuments.DocumentFullPath = unityUDF.GetFullPath(diskGroupID, documentID);
                objDocuments.DocumentFullPath = unityUDF.GetFullPath(documentID);

                // Idemo bez liste fajlova
                //List<string> documentFileList = new List<string>();
                //Dictionary<string, string> documentFileList = new Dictionary<string, string>();
                //documentFileList = unityUDF.GetFilesFullPath(diskGroupID, documentID);
                //objDocuments.DocumentFileList = documentFileList;
                //.

                //
                if (docBarcode != string.Empty)
                {
                    var docPopKeyword = System.Configuration.ConfigurationManager.AppSettings["OnBaseDocPopKeyword"];
                    objDocuments.DocumentUrlByKeyword = webServerURL + string.Format(@"&{0}_0_0_0={1}&keytype=", docPopKeyword, docBarcode);
                }
                //.

                //2. Poslednja revizija dokumenta
                _logger.Debug("Poslednja revizija dokumenta");
                Revision latestRevision = document.LatestRevision;
                if (latestRevision != null)
                {
                    objDocuments.RevisionID = latestRevision.ID;
                    objDocuments.RevisionDate = latestRevision.Date;
                    objDocuments.RevisionComment = latestRevision.Comment;
                    objDocuments.RevisionCreatedByName = latestRevision.CreatedBy.Name;

                    _logger.Debug(string.Format("Dokument revizije id: {0} datum: {1}.", objDocuments.RevisionID, objDocuments.RevisionDate));
                }
                else
                {
                    _logger.Debug("Dokument nema revizije");
                }
                //.

                // 3. Ključne reči izabranog dokumenta
                _logger.Debug("Ključne reči dokumenta");
                Dictionary<string, string> keywordList = new Dictionary<string, string>();
                // Daj nam keyword izabranog dokumenta
                foreach (KeywordRecord keywordRecords in document.KeywordRecords)
                {
                    foreach (Hyland.Unity.Keyword keyword in keywordRecords.Keywords.OrderBy(k => k.KeywordType.KeysetID))
                    {
                        if (!keyword.IsBlank)
                        {
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
                    //.

                    objDocuments.KeywordList = keywordList;

                    //// 1. Kreiramo JsonSerializerSettings objekat za podešavanje serijalizacije
                    //var jsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
                    //jsonSerializerSettings.Culture = CultureInfo.CurrentCulture;
                    //jsonSerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local;
                    //jsonSerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;

                    //var currentCulture = CultureInfo.CurrentCulture;
                    //jsonSerializerSettings.DateFormatString =
                    //    jsonSerializerSettings.Culture.DateTimeFormat.ShortDatePattern + " " + jsonSerializerSettings.Culture.DateTimeFormat.LongTimePattern;
                    ////..


                    //// 2. Konfiguracija globalnih postavki za serijalizaciju
                    //JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
                    //{
                    //    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    //    DateTimeZoneHandling = DateTimeZoneHandling.Local, // Opciono, ako želite da se datumi tretiraju kao UTC
                    //    //Converters = { new IsoDateTimeConverter() }
                    //};

                    //JsonSerializerSettings
                    var jsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
                    {
                        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                        Formatting = Newtonsoft.Json.Formatting.Indented
                    };
                    //.

                    // Formatiranje datuma
                    //3. http://tostring.it/2012/07/18/customize-json-result-in-web-api/
                    var dateFormat = System.Configuration.ConfigurationManager.AppSettings["DateFormat"];
                    if (dateFormat == "IsoDateFormat")
                    {
                        jsonSerializerSettings.Culture = CultureInfo.CurrentCulture;
                        jsonSerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
                        jsonSerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Unspecified;
                    }
                    //.

                    // 4. Stream
                    //DefaultDataProvider defaultDataProvider = application.Core.Retrieval.Default;
                    //PageData pageData = defaultDataProvider.GetDocument(document.DefaultRenditionOfLatestRevision);
                    //var fileStream = pageData.Stream;
                    //byte[] b;
                    //using (BinaryReader br = new BinaryReader(fileStream))
                    //{
                    //    b = br.ReadBytes((int)fileStream.Length);
                    //}
                    //objDocuments.DocumentFileStream = b;
                    //.

                    //var json = JsonConvert.SerializeObject(objDocuments);
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(objDocuments, jsonSerializerSettings);

                    if (json != string.Empty)
                    {
                        result = new Result
                        {
                            Status = json,
                            Message = "Podacu su uspešno preuzeti."
                        };

                        responseMessage = Request.CreateResponse(HttpStatusCode.OK, result);
                        responseMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                        _logger.Debug(result.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error(string.Format("Greška kod preuzimanja podataka dokumenta DocumentID: {0}", documentID));
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                result = new Result
                {
                    Status = string.Empty,
                    Message = ex.Message
                };
                responseMessage = Request.CreateResponse(httpStatusCode, result);
            }
            finally
            {
                if (application != null)
                {
                    _logger.Debug("OnBase Disconnect()");
                    var obj = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                    obj.OnBaseDisconnect(application);
                }
            }

            return responseMessage;
        }
        #endregion
    }
}