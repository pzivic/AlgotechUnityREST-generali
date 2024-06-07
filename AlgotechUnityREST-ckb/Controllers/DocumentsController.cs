// Predrag Živić
// Verzija ckb 01.11.2023

using System;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using System.Text;

using Hyland.Unity;
using Hyland.Unity.Extensions;

using AlgotechUnityREST_ckb.Models;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using System.Linq;
using System.Globalization;

using System.Net.Http.Headers;
using System.Collections.Specialized;

using System.Threading.Tasks;

namespace AlgotechUnityREST_ckb.Controllers
{
    /// <summary>
    /// U osnovi OnBase sistema je dokument. Kotroler sadrži metode sa rad sa dokumentima.
    /// </summary>
    public class DocumentsController : ApiController
    {

        #region Variable  
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //private static HttpResponseMessage response; (new 07.06.2024)

        private static readonly object _documentLock = new object();
        #endregion

        #region DocumentImport
        /// <summary>
        /// Privatna klasa za vraćanje rezultata
        /// </summary>
        private class DocumentsImportResult
        {
            public long RequestID { get; set; }
            public string DocBarcode { get; set; }
            public string GroupBarcode { get; set; }
            public string Message { get; set; }

            // Konstruktor (new 07.06.2024)
            public DocumentsImportResult()
            {
                RequestID = 0;
                DocBarcode = string.Empty;
                GroupBarcode = string.Empty;
                Message = string.Empty;
            }
        }

        /// <summary>
        /// Preuzimanje (import) datoteke u OnBase i upisivanje meta podataka (ključnih reči). 
        /// </summary>
        /// <param name="documentImport">Objekat Documents</param>
        /// <param name="checkingValues">Da li da se vrši provera ulaznih parametara</param>
        /// <returns></returns>
        [HttpPost, ActionName("DocumentImport")]
        public async Task<HttpResponseMessage> DocumentImport(Documents documentImport, bool checkingValues = true)
        {
            HttpResponseMessage responseMessage;
            Application application = null;
            DocumentsImportResult result;

            var dateTimeStart = DateTime.Now;

            // Defalt error code
            var httpStatusCode = HttpStatusCode.Forbidden;
            var sessionID = Guid.NewGuid().ToString();
            long requestID = 0;

            // Inicijalizacija promenljive za rezultat (new 07.06.2024))
            result = new DocumentsImportResult(); // konstruktor radi svoje
            responseMessage = Request.CreateResponse(httpStatusCode, result);
            //.

            try
            {
                // Asinhrono
                await Task.Run(() =>
                {
                    //lock (_documentLock)
                    //{
                        _logger.Info("Metoda:: DocumentImport Asinhrono");
                        _logger.Info(string.Format("DocumentImport Sesija: [{0}].", sessionID));

                        var unityUDF = new Algotech.UTL.AlgotechUnityUDF.UnityUDF();

                        // Sledeće parametre moramo da imamo
                        //if (documentImport.RequestID == null) { throw new ArgumentException("Niste definisali RequestID!"); }

                        if (documentImport.DocumentTypeName == string.Empty) { throw new ArgumentException("Niste definisali DocumentTypeName!"); }
                        if (documentImport.DataSource == string.Empty) { throw new ArgumentException("Niste definisali vrednost DataSource!"); }

                        var match = documentImport.FileList.FirstOrDefault(stringToCheck => stringToCheck.Contains(string.Empty));
                        if (match == null) { throw new ArgumentException("Niste definisali vrednost FileList!"); }
                        //.

                        // Provera da li su vrednosti prosleđene
                        if (checkingValues == true && documentImport.DocumentTypeName != "File Cover Page")
                        {
                            _logger.Debug("Validacija dokumenata.");
                            //httpStatusCode = HttpStatusCode.BadRequest;
                            httpStatusCode = HttpStatusCode.Forbidden;

                            if (documentImport.GroupType == string.Empty) { throw new ArgumentException("Niste definisali GroupType!"); }
                            if (documentImport.DataSource == string.Empty) { throw new ArgumentException("Niste definisali vrednost DataSource!"); }
                            if (documentImport.OrganizationalUnit == string.Empty) { throw new ArgumentException("Niste definisali OrganizationalUnit!"); }
                            if (documentImport.UserName == string.Empty) { throw new ArgumentException("Niste definisali UserName!"); }

                            if (!unityUDF.IsValidGroupType(documentImport.DocumentTypeName, documentImport.GroupType))
                            {
                                throw new ArgumentException(
                                    string.Format("Kombinacija DocumentTypeName '{0}' i GroupType '{1}' nije dozovoljena!", documentImport.DocumentTypeName, documentImport.GroupType));
                            }

                            // Dadatna validacija
                            if (documentImport.GroupID == string.Empty) { throw new ArgumentException("Niste definisali GroupID!"); }
                            if (documentImport.DocID == string.Empty) { throw new ArgumentException("Niste definisali DocID!"); }
                            if (documentImport.ClientID == string.Empty) { throw new ArgumentException("Niste definisali ClientID!"); }
                            if (documentImport.ClientName == string.Empty) { throw new ArgumentException("Niste definisali ClientName!"); }
                        }
                        //.

                        // Ako je File Cover Page nećemo raditi validaciju u kontekstu dokumenata
                        if (checkingValues == true && documentImport.DocumentTypeName == "File Cover Page")
                        {
                            _logger.Debug("Validacija File Cover Page.");
                            httpStatusCode = HttpStatusCode.NotFound;
                            if (documentImport.GroupID == string.Empty) { throw new ArgumentException("Niste definisali GroupID!"); }
                            if (documentImport.GroupType == string.Empty) { throw new ArgumentException("Niste definisali GroupType!"); }
                        }
                        //.

                        // Format alternativnog ključa
                        var documentAK_LenghtPrefiks = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["DocumentAK_LenghtPrefiks"]);
                        var documentAK_LenghtBase = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["DocumentAK_LenghtBase"]);
                        var documentAK_LenghtSufix = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["DocumentAK_LenghtSufix"]);
                        var documentAK_Separator = System.Configuration.ConfigurationManager.AppSettings["DocumentAK_Separator"];

                        var userName = documentImport.UserName;

                        _logger.Debug(string.Format("DocumentTypeName: {0}", documentImport.DocumentTypeName));
                        _logger.Debug(string.Format("DataSource: {0}", documentImport.DataSource));
                        _logger.Debug(string.Format("DocStatus: {0}", documentImport.DocStatus));

                        _logger.Debug(string.Format("DocID: {0}", documentImport.DocID));
                        _logger.Debug(string.Format("ClientID: {0}", documentImport.ClientID));
                        _logger.Debug(string.Format("ClientName: {0}", documentImport.ClientName));

                        _logger.Debug(string.Format("OrganizationalUnit: {0}", documentImport.OrganizationalUnit));
                        _logger.Debug(string.Format("UserName: {0}", documentImport.UserName));

                        // Konekcija na OnBase
                        _logger.Info("DocumentImport OnBase Connect()");
                        var obj = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                        application = obj.OnBaseConnect();
                        if (application == null)
                        {
                            _logger.Warn("Greška kod uspostavljanja konekcije sa OnBase");
                            throw new ArgumentException("Greška kod uspostavljanja konekcije sa OnBase. Kontaktirajte OnBase administratora.");
                        }
                        //.

                        //requestID = (long)documentImport.RequestID;
                        requestID = documentImport.RequestID == null ? 0 : (long)documentImport.RequestID;

                        // Logijemo od momenta uspostavljanja konekcije
                        dateTimeStart = DateTime.Now;
                        _logger.Info(string.Format("DocumentImport session: [{0}]; onbase sesion [{1}] request: [{2}]; start: {3}.",
                            sessionID, application.SessionID, requestID, dateTimeStart));

                        // Validacija Tipa dokumenta
                        DocumentType documentType =
                            application.Core.DocumentTypes.Find(documentImport.DocumentTypeName) ?? throw new Exception(string.Format("Tip dokumenta '{0}' nije pronađen ili nisu podešena prava pristupa servisnom nalogu.", documentImport.DocumentTypeName));
                        FileType fileType =
                            application.Core.FileTypes.Find(documentImport.DefaultFileTypeName) ?? throw new Exception(string.Format("Tip datoteke '{0}' nije pronađen ili nisu podešena prava pristupa servisnom nalogu.", documentImport.DefaultFileTypeName));
                        //.

                        // Svojstva dokumenta
                        StoreNewDocumentProperties storeDocumentProperties =
                            application.Core.Storage.CreateStoreNewDocumentProperties(documentType, fileType);

                        // Svojstva 
                        storeDocumentProperties.TextEncoding = Encoding.Unicode;
                        storeDocumentProperties.DocumentDate = documentImport.DocumentDate;
                        storeDocumentProperties.Comment = documentImport.Comment;
                        //storeDocumentProperties.Options = StoreDocumentOptions.SkipWorkflow;
                        //..

                        // Standard keyword type, koncept je da proverimo da li je prosleđena vrednost ključne reči i ako jeste upisujemo.
                        // Ne proveravamo da li postoji definicija ključne reč u tipu dokumenta, želimo da baci exeption i da se proveri konfiguracija.

                        // Podaci o dokumentu, jedinstveni identifikator dokumenta nećemo unositi već ćemo je generisati i upisati
                        //if (documentImport.DocBarcode != string.Empty)
                        //    storeDocumentProperties.AddKeyword("DocBarcode", documentImport.DocBarcode);

                        // Securiry keyword
                        if (!string.IsNullOrEmpty(documentImport.OrganizationalUnit))
                            storeDocumentProperties.AddKeyword("OrganizationalUnit", documentImport.OrganizationalUnit);

                        if (!string.IsNullOrEmpty(documentImport.UserName))
                            storeDocumentProperties.AddKeyword("UserName", documentImport.UserName);
                        //storeDocumentProperties.AddKeyword("CKB_UserName", documentImport.UserName);

                        // Ostali keyword
                        if (!string.IsNullOrEmpty(documentImport.GroupType))
                            storeDocumentProperties.AddKeyword("GroupType", documentImport.GroupType);

                        if (!string.IsNullOrEmpty(documentImport.GroupID))
                            storeDocumentProperties.AddKeyword("GroupID", documentImport.GroupID);

                        if (!string.IsNullOrEmpty(documentImport.GroupBarcode))
                            storeDocumentProperties.AddKeyword("GroupBarcode", documentImport.GroupBarcode);

                        if (!string.IsNullOrEmpty(documentImport.DocID))
                            storeDocumentProperties.AddKeyword("DocID", documentImport.DocID);

                        if (!string.IsNullOrEmpty(documentImport.ClientID))
                            storeDocumentProperties.AddKeyword("ClientID", documentImport.ClientID);

                        if (!string.IsNullOrEmpty(documentImport.ClientName))
                            storeDocumentProperties.AddKeyword("ClientName", documentImport.ClientName);
                        //.
                        if (!string.IsNullOrEmpty(documentImport.DataSource))
                            storeDocumentProperties.AddKeyword("DataSource", documentImport.DataSource);

                        if (documentImport.DocRegDate != DateTime.MinValue)
                            storeDocumentProperties.AddKeyword("DocRegDate", documentImport.DocRegDate);

                        if (!string.IsNullOrEmpty(documentImport.DocStatus))
                            storeDocumentProperties.AddKeyword("DocStatus", documentImport.DocStatus);

                        //if (documentImport.RequestID != null)
                        //storeDocumentProperties.AddKeyword("RequestID", (long)documentImport.RequestID);
                        if (requestID != 0)
                            storeDocumentProperties.AddKeyword("RequestID", requestID);
                        //.

                        var akValue = string.Empty;

                        #region Izmeni ključnu reč za Alternativni ključ koncept 2. (unapred da imamo seqkvencu)
                        //var keywordNameAK = "DocBarcode";
                        //if (documentImport.DocumentTypeName == "File Cover Page")
                        //    keywordNameAK = "GroupBarcode";

                        //_logger.Debug(string.Format("Kreiramo vrednost za alternativni kljuc [{0}].", keywordNameAK));

                        //unityUDF = new Algotech.UTL.AlgotechUnityUDF.UnityUDF();
                        //var documentTypeID = unityUDF.GetDocumentTypeID(application, documentImport.DocumentTypeName);
                        ////var newSeqValue = unityUDF.GetSequenceValue("DocBarcodeGroupBarcode");
                        //var newSeqValue = requestID;
                        //akValue = string.Format("{1}{0}{2}{0}{3}", documentAK_Separator,
                        //    documentImport.DataSource, documentTypeID, newSeqValue.ToString().PadLeft(documentAK_LenghtSufix, Convert.ToChar("0")));
                        //// Upiši ak
                        //storeDocumentProperties.AddKeyword(keywordNameAK, akValue);

                        //_logger.Debug(string.Format("Upisaćemo: KeywordName {0}, KeywordValue={1}.", keywordNameAK, akValue));
                        ////.
                        #endregion

                        var status = string.Empty;
                        var message = string.Empty;

                        // Skladišti novi dokument / Save the new document
                        _logger.Debug(string.Format("Skladišti novi dokument: Sesija [{0}]; OnBase konekcija [{1}].", sessionID, application.SessionID));
                        Document newDocument =
                            application.Core.Storage.StoreNewDocument(documentImport.FileList, storeDocumentProperties);
                        long newDocumentID = newDocument.ID; // Uzmi ga odmah
                        _logger.Debug(string.Format("Usladištili smo novi dokument: Sesija [{0}]; OnBase konekcija [{1}]; DocumentID [{2}].", sessionID, application.SessionID, newDocumentID));
                        //.

                        #region Izmeni ključnu reč za Alternativni ključ koncept 1 (osnovni sa newDocument.ID)
                        // Izmeni ključnu reč za Alternativni ključ koncept 1. 
                        var keywordNameAK = "DocBarcode";
                        if (documentImport.DocumentTypeName == "File Cover Page")
                        {
                            keywordNameAK = "GroupBarcode";
                        }

                        _logger.Debug(string.Format("Kreiramo vrednost za alternativni kljuc [{0}].", keywordNameAK));

                        unityUDF = new Algotech.UTL.AlgotechUnityUDF.UnityUDF();
                        var documentTypeID = unityUDF.GetDocumentTypeID(application, documentImport.DocumentTypeName);
                        var documentKeyword = new DocumentKeywords
                        {
                            DocumentTypeName = documentImport.DocumentTypeName,
                            KeywordName = keywordNameAK,
                            // Definiši vrednost newDocument popuni nulama dužine zadane u DocumentAK_LenghtSufix
                            KeywordValue = string.Format("{1}{0}{2}{0}{3}", documentAK_Separator,
                                documentImport.DataSource, documentTypeID, newDocumentID.ToString().PadLeft(documentAK_LenghtSufix, Convert.ToChar("0"))),
                            DataSource = documentImport.DataSource,
                            UserName = documentImport.UserName,
                            //.
                        };
                        _logger.Debug(string.Format("DocumentKeywordSetValue Sesija [{0}]; OnBase konekcija [{1}].", sessionID, application.SessionID));

                        // Kritični momenat
                        DocumentKeywordSetValue(application, newDocument, documentKeyword);
                        //.

                        _logger.Debug(string.Format("Izmenjeno je: KeywordName {0}, KeywordValue={1}.", documentKeyword.KeywordName, documentKeyword.KeywordValue));
                        //.

                        akValue = documentKeyword.KeywordValue;
                        //message = string.Format("Dokument je uspešno sačuvan, DocBarcode: {0}.", status);
                        //.
                        #endregion

                        status = akValue.ToString();

                        if (status != string.Empty)
                        {
                            // Simulacija lošeg responsa umesto prave vrednosti vratiće-1
                            if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["IsEnvPROD"]) == false)
                            {
                                if (requestID % 2 != 0)
                                {
                                    //Neparne vrednosi kvarimo
                                    requestID--;
                                }
                            }
                            //.

                            if (documentImport.DocumentTypeName == "File Cover Page")
                            {
                                if (requestID == 0)
                                {
                                    message = string.Format("File Cover Page je uspešno sačuvan, GroupBarcode: {0}.", status);
                                    result = new DocumentsImportResult
                                    {
                                        GroupBarcode = status,
                                        Message = message
                                    };
                                }
                                else
                                {
                                    message = string.Format("Za zahtev broj #{0} 'File Cover Page' je uspešno sačuvan, GroupBarcode: {1}.", requestID, status);
                                    result = new DocumentsImportResult
                                    {
                                        RequestID = requestID,
                                        GroupBarcode = status,
                                        Message = message
                                    };
                                }

                            }
                            else
                            {
                                if (requestID == 0)
                                {
                                    message = string.Format("Dokument je uspešno sačuvan, DocBarcode: {0}.", status);
                                    result = new DocumentsImportResult
                                    {
                                        DocBarcode = status,
                                        Message = message
                                    };
                                }
                                else
                                {
                                    message = string.Format("Za zahtev broj #{0} dokument je uspešno sačuvan, DocBarcode: {1}.", requestID, status);
                                    result = new DocumentsImportResult
                                    {
                                        RequestID = requestID,
                                        DocBarcode = status,
                                        Message = message
                                    };
                                }
                            }

                            responseMessage = Request.CreateResponse(HttpStatusCode.OK, result);

                            //Kolačić
                            //https://www.c-sharpcorner.com/UploadFile/2b481f/how-to-set-cookies-in-Asp-Net-web-api/
                            var nalueCollection = new NameValueCollection
                            {
                                ["ID"] = sessionID,
                                ["AK"] = status,
                                ["RequestID"] = requestID.ToString()
                            };
                            var cookie = new CookieHeaderValue("Session", nalueCollection)
                            {
                                Expires = DateTimeOffset.Now.AddDays(1), //AddMilliseconds(1000)
                                Domain = Request.RequestUri.Host,
                                Path = "/"
                            };
                            responseMessage.Headers.AddCookies(new CookieHeaderValue[] { cookie });
                            //.

                            // Logger
                            _logger.Info(string.Format("Response: {0}; UserName: {1}; OrganizationalUnit {2}; SessionID: {3}; OnBase SessionID: {4} RequestID: {5}.",
                                result.Message, documentImport.UserName, documentImport.OrganizationalUnit, sessionID, application.SessionID, requestID));
                            //.
                        }
                  //  }
                });

            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error(
                    string.Format("Greška kod importovanja dokumenta DocumentTypeName: {0} UserName: {1} RequestID: {2}.", documentImport.DocumentTypeName, documentImport.UserName, requestID));
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                result = new DocumentsImportResult
                {
                    DocBarcode = string.Empty,
                    Message = ex.Message
                };
                responseMessage = Request.CreateResponse(httpStatusCode, result);
            }
            finally
            {
                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
                _logger.Debug(string.Format("DocumentImport: session [{0}] finally {1} in sec {2}.", sessionID, dateTimeEnd, diffInSeconds));

                if (application != null)
                {
                    _logger.Info(string.Format("DocumentImport OnBase Disconnect() Time: {0}; OnBase SessionID: {1}.", dateTimeEnd, application.SessionID));
                    var obj = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                    obj.OnBaseDisconnect(application);
                }
            }
            return responseMessage;
        }
        #endregion

        #region DocumentDelete
        /// <summary>
        /// Za prosleđeni documentID (OnBase Document Handle) vrši logičko brisanje dokumenta tako što promeni status dokumenta u Deleted=16.
        /// </summary>
        /// <param name="documentID">Jedinstveni identifikator dokumenta. OnBase Document Handle.</param>
        [HttpDelete, ActionName("DocumentDelete")]
        public HttpResponseMessage DocumentDelete(long documentID)
        {
            HttpResponseMessage responseMessage;
            Application application = null;
            Result objResult;
            string result;

            try
            {
                // Konekcija
                _logger.Debug("OnBase Connect()");
                var obj = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                application = obj.OnBaseConnect();
                if (application == null)
                {
                    _logger.Warn("Greška kod uspostavljanja konekcije sa OnBase");
                    throw new ArgumentException("Greška kod uspostavljanja konekcije sa OnBase. Kontaktirajte OnBase administratora.");
                }
                //.

                Storage storage = application.Core.Storage;

                Debug.WriteLine("Attempting to delete document...");

                Document document = application.Core.GetDocumentByID(documentID);
                if (document == null)
                {
                    throw new Exception("Document was not found");
                }

                using (DocumentLock documentLock = document.LockDocument())
                {
                    if (documentLock.Status != DocumentLockStatus.LockObtained)
                    {
                        throw new Exception("Failed to lock document");
                    }
                    application.Core.Storage.DeleteDocument(document);
                }

                Debug.WriteLine("Document was deleted.");

                result = "True";

                objResult = new Result
                {
                    Status = result,
                    Message = "Dokument je uspešno obrisan."
                };
                responseMessage = Request.CreateResponse(HttpStatusCode.OK, objResult);

            }
            catch (Exception ex)
            {
                objResult = new Result
                {
                    Status = string.Empty,
                    Message = ex.Message
                };
                responseMessage = Request.CreateResponse(HttpStatusCode.BadRequest, objResult);
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
                var diskGroupID = Convert.ToInt64(System.Configuration.ConfigurationManager.AppSettings["DiskGroupID"]); // Bankarska dokumentacija                                                                             
                objDocuments.DocumentFullPath = unityUDF.GetFullPath(diskGroupID, documentID);

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
                    foreach (Keyword keyword in keywordRecords.Keywords.OrderBy(k => k.KeywordType.KeysetID))
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
                    var jsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
                    jsonSerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                    jsonSerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
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

        #region DocumentExport
        /// <summary>
        /// Export dokumenta
        /// </summary>
        /// <param name="documentID">Identifikator dokumenta</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        [HttpPost, ActionName("DocumentExport")]
        //[Route("Api/Documents/DocumentExport/{documentID:long}")]
        public HttpResponseMessage DocumentExport(long documentID)
        {
            HttpResponseMessage responseMessage;
            Application application = null;

            var dateTimeStart = DateTime.Now;

            // Defalt error code
            var httpStatusCode = HttpStatusCode.Forbidden;

            // Inicijalizacija promenljive za rezultat (new 07.06.2024))
            var result = new Result();
            responseMessage = Request.CreateResponse(httpStatusCode, result);
            //.
            try
            {
                _logger.Debug("Metoda:: DocumentExport");
                //_logger.Debug(string.Format("documentID: {0} {1}.", documentID, forceLockDocument));

                if (documentID == 0) { throw new ArgumentException("Morate da prosledite documentiD."); }
                _logger.Debug("OnBase Connect()");
                var obj = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                application = obj.OnBaseConnect();
                if (application == null)
                {
                    _logger.Warn("Greška kod uspostavljanja konekcije sa OnBase");
                    throw new ArgumentException("Greška kod uspostavljanja konekcije sa OnBase. Kontaktirajte OnBase administratora.");
                }

                // Logijemo od momenta uspostavljanja konekcije
                dateTimeStart = DateTime.Now;

                _logger.Debug("DocumentImport start " + dateTimeStart);

                var appFileUploadPath = System.Configuration.ConfigurationManager.AppSettings["ExportFolder"];
                Document document = application.Core.GetDocumentByID(documentID) ?? throw new Exception("Document was not found");

                var status = string.Empty;
                var message = string.Empty;
                var fileName = string.Empty;
                using (DocumentLock documentLock = document.LockDocument())
                {

                    var forceLockDocument = System.Configuration.ConfigurationManager.AppSettings["ForceLockDocument"] == "True";
                    if (forceLockDocument)
                    {
                        if (documentLock.Status != DocumentLockStatus.LockObtained)
                        {
                            _logger.Warn(string.Format("Zaključavanje dokumenta DocumentID: {0} nije uspelo!", documentID));
                            throw new Exception("Failed to lock document");
                        }
                    }
                    //.

                    // Export file
                    var pageDataExtension = "";
                    DefaultDataProvider defaultDataProvider = application.Core.Retrieval.Default;
                    using (PageData pageData = defaultDataProvider.GetDocument(document.DefaultRenditionOfLatestRevision))
                    {
                        using (Stream stream = pageData.Stream)
                        {
                            // naziv datoteke
                            fileName = string.Format(appFileUploadPath + @"\{0}.{1}", Guid.NewGuid().ToString(), pageData.Extension);
                            Utility.WriteStreamToFile(stream, fileName);
                            pageDataExtension = pageData.Extension;
                        }
                    }
                    //.
                }

                status = fileName;
                message = string.Format("Dokument je uspešno exportovan na privremenu lokaciju.");
                _logger.Debug(String.Format("Dokument documentID={0} je uspešno exportovan na privremenu lokaciju.", documentID));

                if (status != string.Empty)
                {
                    result = new Result
                    {
                        Status = status,
                        Message = message
                    };
                    responseMessage = Request.CreateResponse(HttpStatusCode.OK, result);

                    // Logger
                    _logger.Debug(result.Message);
                    //.
                }
            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error(
                    string.Format("Greška kod exportovanja dokumenta documentID: {0} ", documentID));
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
                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
                _logger.Debug("DocumentImport finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());
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

        #region DocumentKeywordSetValue 1
        //        /// <summary>
        //        /// Upisuje vrednost ključne reči u document za prosleđenim documentID. Objekat DocumentKeywords sadrži informaciju o tipu dokumenta, nazivu i vrednosti ključne reči
        //        /// </summary>
        //        /// <param name="application"></param>
        //        /// <param name="documentKeyword">Objekat documentKeyword.</param>
        //        /// <param name="documentID">Broj zahteva.</param>
        //        //[HttpPost, ActionName("DocumentKeywordSetValue")]
        //        private HttpResponseMessage DocumentKeywordSetValue(Application application, DocumentKeywords documentKeyword, long documentID)
        //        {
        //            Result result;
        //            var dateTimeStart = DateTime.Now;

        //            if (documentID == 0) { throw new ArgumentException("Parametar documentID mora da bude definisan."); }

        //            // Defalt error code
        //            var httpStatusCode = HttpStatusCode.Forbidden;
        //            try
        //            {
        //                //_logger.Debug("Metoda:: SetDocumentKeyword");
        //                _logger.Debug("Metoda:: DocumentKeywordSetValue");
        //                _logger.Debug(string.Format("DataSource: {0}", documentKeyword.DataSource));
        //                _logger.Debug(string.Format("UserName: {0}", documentKeyword.UserName));

        //                if (documentKeyword.DocumentTypeName == string.Empty) { throw new ArgumentException("Svojstvo DocumentTypeName mora da bude definisano."); }
        //                if (documentKeyword.KeywordName == string.Empty) { throw new ArgumentException("Svojstvo KeywordName mora da bude definisano."); }
        //                if (documentKeyword.KeywordValue == string.Empty) { throw new ArgumentException("Svojstvo KeywordValue mora da bude definisano."); }

        //                var documentTypeName = documentKeyword.DocumentTypeName;
        //                var keywordName = documentKeyword.KeywordName;
        //                var keywordValue = documentKeyword.KeywordValue;

        //                dateTimeStart = DateTime.Now;
        //                _logger.Debug("SetDocumentKeyword start " + dateTimeStart);

        //                // Validacija Tipa dokumenta
        //                var documentTypeValidate = application.Core.DocumentTypes.Find(documentTypeName) ?? throw new Exception(string.Format("Tip dokumenta '{0}' nije pronađen.", documentTypeName));
        //                //.

        //                // Nađi dokument preko ID 
        //                var document = application.Core.GetDocumentByID(documentID);
        //                if (document == null)
        //                {
        //                    httpStatusCode = HttpStatusCode.NotFound;
        //                    _logger.Error(string.Format("Dokument za DocumentType '{0}' nije pronađen!", documentTypeName));
        //                    throw new Exception(string.Format("Dokument za DocumentType '{0}' nije pronađen!", documentTypeName));
        //                }
        //                //. 

        //                // Daj tip dokumenta iz objekta dokumenta.
        //                var documentType = document.DocumentType;

        //                //var keywordTypeAccountNum = documentType.KeywordRecordTypes.FindKeywordType("AccountNum");
        //                var keywordTypeName = documentType.KeywordRecordTypes.FindKeywordType(keywordName);
        //                if (keywordTypeName == null)
        //                {
        //                    _logger.Error(string.Format("Tip ključne reči '{0}' nije pronađen ili se ne nalazi na tipu dokumenta sa imenom: {1}.", keywordName, documentType.Name));
        //                    throw new Exception(string.Format("Tip ključne reči '{0}' nije pronađen ili se ne nalazi na tipu dokumenta sa imenom: {1}.", keywordName, documentType.Name));
        //                }
        //                if (!keywordTypeName.TryCreateKeyword(keywordValue, out Keyword newKeywordValue))
        //                {
        //                    _logger.Error(string.Format("Vrednost {1} za tip ključne reči {0} nije nije uspešno upisana.", keywordValue, keywordName));
        //                    throw new Exception(string.Format("Vrednost {1} za tip ključne reči {0} nije nije uspešno upisana.", keywordValue, keywordName));
        //                }

        //                // Create the KeywordModifier from the document
        //                KeywordModifier keywordModifier = document.CreateKeywordModifier();
        //                // Find the keyword record that contains our keyword type.
        //                KeywordRecord keywordRecord = document.KeywordRecords.Find(keywordTypeName);
        //                if (keywordRecord != null)
        //                {
        //                    // Find the old keyword from the record of the keyword type.
        //                    var oldKeywordValue = keywordRecord.Keywords.Find(keywordTypeName);
        //                    keywordModifier.UpdateKeyword(oldKeywordValue, newKeywordValue);
        //                }
        //                else
        //                {
        //                    // else if the keyword record is null
        //                    keywordModifier.AddKeyword(newKeywordValue);
        //                }

        //                var forceLockDocument = System.Configuration.ConfigurationManager.AppSettings["ForceLockDocument"] == "True";
        //                if (forceLockDocument == true)
        //                {
        //                    // Zaključavanje dokumenta
        //                    _logger.Debug(string.Format("Zaključavanje dokumenta : {0}", documentID));
        //                    using (DocumentLock documentLock = document.LockDocument())
        //                    {
        //                        // Check if the lock status is Already Locked
        //                        if (documentLock.Status == DocumentLockStatus.AlreadyLocked)
        //                        {
        //                            httpStatusCode = HttpStatusCode.Conflict;
        //                            _logger.Error(string.Format("Zaključavanje dokumenta nije uspelo (AlreadyLocked) : {0} status zaključavanja {2}", documentID, documentLock.Status));
        //                            throw new Exception("Zaključavanje dokumenta nije bilo moguće, zaključavanje je izvšio drugi korisnik ili proces.");
        //                        }
        //                        // Check if the lock status is LockObtained
        //                        if (documentLock.Status == DocumentLockStatus.LockObtained)
        //                        {
        //                            // If not locked, apply the changes to the database.
        //                            keywordModifier.ApplyChanges();
        //                            _logger.Debug(string.Format("Okinuli smo ApplyChanges sa zaključavanjem dokumenta DocumentID: {0}; OnBase SessionID: [{1}].", documentID, application.SessionID));
        //                        }
        //                    }
        //                    //.
        //                }
        //                else
        //                {
        //                    keywordModifier.ApplyChanges();
        //                    _logger.Debug(string.Format("Okinuli smo ApplyChanges bez zaključavanja dokumentaza DocumentID: {0}; OnBase SessionID: [{1}].", documentID, application.SessionID));
        //                }

        //                result = new Result
        //                {
        //                    Status = newKeywordValue.ToString(),
        //                    Message = "Uspešno je upisana vrednost ključne reči."
        //                };

        //#if !DEBUG
        //                response = Request.CreateResponse(HttpStatusCode.OK, result);
        //#endif

        //                // Logger
        //                _logger.Debug(result.Message);
        //                //.
        //            }
        //            catch (Exception ex)
        //            {
        //                // Logger
        //                _logger.Error(
        //                    string.Format("Greska kod upisa vrednosti kljucne reci dokumenta: {0}", documentKeyword.DocumentTypeName));
        //                _logger.Error(ex.Message);
        //                _logger.Error(ex.StackTrace);
        //                //.

        //                result = new Result
        //                {
        //                    Status = string.Empty,
        //                    Message = "ERROR - " + ex.Message
        //                };
        //                response = Request.CreateResponse(httpStatusCode, result);
        //            }

        //            finally
        //            {
        //                var dateTimeEnd = DateTime.Now;
        //                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
        //                _logger.Debug("DocumentKeywordSetValue finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());

        //           }
        //            return response;
        //        }
        #endregion

        #region DocumentKeywordSetValue 2
        /// <summary>
        /// Upisuje vrednost ključne reči u dokument za prosleđenim objekat Document. DocumentKeywords objekat sadrži informaciju o tipu dokumenta, nazivu i vrednosti ključne reči
        /// </summary>
        /// <param name="application">OnBase konekcija na app server</param>
        /// <param name="document">Dokument kojem menjamo ključne reči</param>
        /// <param name="documentKeyword">Objekat documentKeyword.</param>
        private bool DocumentKeywordSetValue(Application application, Document document, DocumentKeywords documentKeyword)
        {
            var dateTimeStart = DateTime.Now;
            bool retValue = false;

            //if (documentID == 0) { throw new ArgumentException("Parametar documentID mora da bude definisan."); }

            try
            {
                _logger.Debug("Metoda:: DocumentKeywordSetValue"); // (preko Document document)");
                _logger.Debug(string.Format("DataSource: {0}", documentKeyword.DataSource));
                _logger.Debug(string.Format("UserName: {0}", documentKeyword.UserName));

                if (documentKeyword.DocumentTypeName == string.Empty) { throw new ArgumentException("Svojstvo DocumentTypeName mora da bude definisano."); }
                if (documentKeyword.KeywordName == string.Empty) { throw new ArgumentException("Svojstvo KeywordName mora da bude definisano."); }
                if (documentKeyword.KeywordValue == string.Empty) { throw new ArgumentException("Svojstvo KeywordValue mora da bude definisano."); }

                var documentTypeName = documentKeyword.DocumentTypeName;
                var keywordName = documentKeyword.KeywordName;
                var keywordValue = documentKeyword.KeywordValue;

                dateTimeStart = DateTime.Now;
                _logger.Debug("SetDocumentKeyword start " + dateTimeStart);

                //// Validacija Tipa dokumenta
                //var documentTypeValidate = application.Core.DocumentTypes.Find(documentTypeName) ?? throw new Exception(string.Format("Tip dokumenta '{0}' nije pronađen.", documentTypeName));
                ////.

                //// Nađi dokument preko ID 
                //var document = application.Core.GetDocumentByID(documentID);
                //if (document == null)
                //{
                //    _logger.Error(string.Format("Dokument za DocumentType '{0}' nije pronađen!", documentTypeName));
                //    throw new Exception(string.Format("Dokument za DocumentType '{0}' nije pronađen!", documentTypeName));
                //}
                ////. 

                // Daj tip dokumenta iz objekta dokumenta.
                var documentType = document.DocumentType;
                var keywordTypeName = documentType.KeywordRecordTypes.FindKeywordType(keywordName);
                if (keywordTypeName == null)
                {
                    _logger.Error(string.Format("Tip ključne reči '{0}' nije pronađen ili se ne nalazi na tipu dokumenta sa imenom: {1}.", keywordName, documentType.Name));
                    throw new Exception(string.Format("Tip ključne reči '{0}' nije pronađen ili se ne nalazi na tipu dokumenta sa imenom: {1}.", keywordName, documentType.Name));
                }
                if (!keywordTypeName.TryCreateKeyword(keywordValue, out Keyword newKeywordValue))
                {
                    _logger.Error(string.Format("Vrednost {1} za tip ključne reči {0} nije nije uspešno upisana.", keywordValue, keywordName));
                    throw new Exception(string.Format("Vrednost {1} za tip ključne reči {0} nije nije uspešno upisana.", keywordValue, keywordName));
                }

                // Create the KeywordModifier from the document
                KeywordModifier keywordModifier = document.CreateKeywordModifier();
                // Find the keyword record that contains our keyword type.
                KeywordRecord keywordRecord = document.KeywordRecords.Find(keywordTypeName);
                if (keywordRecord != null)
                {
                    // Find the old keyword from the record of the keyword type.
                    var oldKeywordValue = keywordRecord.Keywords.Find(keywordTypeName);
                    keywordModifier.UpdateKeyword(oldKeywordValue, newKeywordValue);
                }
                else
                {
                    // else if the keyword record is null
                    keywordModifier.AddKeyword(newKeywordValue);
                }

                // Zaklučavanje u upisivanje
                var forceLockDocument = System.Configuration.ConfigurationManager.AppSettings["ForceLockDocument"] == "True";
                if (forceLockDocument == true)
                {
                    // Zaključavanje dokumenta
                    _logger.Debug(string.Format("Zaključavanje dokumenta: {0}", document.ID));
                    using (DocumentLock documentLock = document.LockDocument())
                    {
                        // Check if the lock status is Already Locked
                        if (documentLock.Status == DocumentLockStatus.AlreadyLocked)
                        {
                            retValue = false;
                            _logger.Error(string.Format("Zaključavanje dokumenta nije uspelo (AlreadyLocked) : {0} status zaključavanja {2}", document.ID, documentLock.Status));
                            throw new Exception("Zaključavanje dokumenta nije bilo moguće, zaključavanje je izvšio drugi korisnik ili proces.");
                        }
                        // Check if the lock status is LockObtained
                        if (documentLock.Status == DocumentLockStatus.LockObtained)
                        {
                            // If not locked, apply the changes to the database.
                            keywordModifier.ApplyChanges();
                            _logger.Debug(string.Format("Okinuli smo ApplyChanges sa zaključavanjem dokumenta DocumentID: {0}; OnBase SessionID: [{1}].", document.ID, application.SessionID));
                        }
                    }
                    //.
                }
                else
                {
                    keywordModifier.ApplyChanges();
                    _logger.Debug(string.Format("Okinuli smo ApplyChanges bez zaključavanja dokumenta za DocumentID: {0}; OnBase SessionID: [{1}].", document.ID, application.SessionID));
                }

                retValue = true;
            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error(string.Format("Greska kod upisivanja kljucne reci dokumenta ID={0}.", document.ID));
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                throw new Exception(ex.Message);
            }

            finally
            {
                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
                //_logger.Debug("DocumentKeywordSetValue (preko Document document) finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());
                _logger.Debug("DocumentKeywordSetValue finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());

            }
            return retValue;
        }
        #endregion

    }
}

