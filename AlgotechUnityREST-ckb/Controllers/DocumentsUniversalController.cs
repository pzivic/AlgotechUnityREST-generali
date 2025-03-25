// Predrag Živić
// 01.03.2025
// Univerzalni data import

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
        }

        /// <summary>
        /// Klučne reči
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

                // Upisujemo ključne reči 
                foreach (var keyword in documentImport.Keywords)
                {
                    if (!string.IsNullOrEmpty(keyword.KeywordName))
                        storeDocumentProperties.AddKeyword(keyword.KeywordName, keyword.KeywordValue);
                }
                //.

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
    }
}