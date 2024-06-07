using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Hyland.Unity;
using Newtonsoft.Json;

using AlgotechUnityREST_ckb.Models;

namespace AlgotechUnityREST_ckb.Controllers
{
    /// <summary>
    /// Ključne reči
    /// </summary>
    public class DocumentsKeywordsController : ApiController
    {

        #region Variable  
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object _documentLock = new object();
        HttpResponseMessage response;
        #endregion

        #region DocumentResult
        /// <summary>
        /// Privatna klasa za listanje dokumenata
        /// </summary>
        [JsonObject(Title = "DocumentResult")]
        private class DocumentsList
        {
            //[JsonProperty(PropertyName = "id")]
            public string DocumentID { get; set; }
            //[JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
            //[JsonProperty(PropertyName = "created-on")]
            public DateTime DocumentDate { get; set; }
            public DateTime DateStored { get; set; }
            //[JsonProperty(PropertyName = "path")]
            public string DocumentUrl { get; internal set; }
            //[JsonProperty(PropertyName = "media-type")]
            public string DefaultFileTypeName { get; set; }

            //public string AccountNum { get; set; }

            public string GroupType { get; set; }
            public string GroupID { get; set; }
            public string GroupBarcode { get; set; }
            public string DocID { get; set; }
            public string DocBarcode { get; set; }

            public string ClientID { get; set; }
            public string ClientName { get; set; }

            public string DataSource { get; set; }
            public DateTime DocRegDate { get; set; }
            public string DocStatus { get; set; }

        }
        #endregion

        #region DocumentKeywords
        /// <summary>
        /// Vraća ključne reči određenog dokumenta
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        [HttpGet, ActionName("DocumentKeywords")]
        public HttpResponseMessage DocumentKeywords(long documentID)
        {
            Application application = null;
            Result objResult;
            string result;

            try
            {
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

                _logger.Info("Pokušaj preuzimanja svojstava dokumenta...");
                Document document = application.Core.GetDocumentByID(documentID);
                if (document == null)
                {
                    _logger.Error(string.Format("Dokument za DocumentID={0} nije pronađen!", documentID));
                    throw new Exception("Dokument nije pronađen!");
                }

                using (DocumentLock documentLock = document.LockDocument())
                {
                    var forceLockDocument = System.Configuration.ConfigurationManager.AppSettings["ForceLockDocument"] == "True";
                    if (forceLockDocument == true)
                    {
                        _logger.Error(string.Format("Pokušaj da zaključaš dokument DocumentID={0}", documentID));
                        if (documentLock.Status != DocumentLockStatus.LockObtained)
                        {
                            _logger.Error("Zaključavanje dokumenta nije uspelo, molimo pokušajte kasnije.");
                            throw new Exception("Zaključavanje dokumenta nije uspelo, molimo pokušajte kasnije.");
                        }
                    }

                    // Ključne reči izabranog dokumenta
                    Dictionary<string, string> keywordList = new Dictionary<string, string>();
                    // 
                    _logger.Info("Daj nam keyword izabranog dokumenta");
                    foreach (KeywordRecord keywordRecords in document.KeywordRecords)
                    {
                        foreach (Keyword keyword in keywordRecords.Keywords)
                        {
                            //objKeyword.KeywordTypeName = keyword.KeywordType.Name;
                            //objKeyword.KeywordValue = keyword.Value.ToString();

                            keywordList.Add(keyword.KeywordType.Name, keyword.Value.ToString());
                        }
                    }
                    //var objKeyword = new Keywords()
                    //{
                    //    KeywordList = keywordList
                    //};
                    //.

                    result = Newtonsoft.Json.JsonConvert.SerializeObject(keywordList);
                }

                if (result != string.Empty)
                {
                    objResult = new Result
                    {
                        Status = result,
                        Message = "Podacu su uspešno preuzeti."
                    };

                    response = Request.CreateResponse(HttpStatusCode.OK, objResult);
                    response.Content = new StringContent(result, System.Text.Encoding.UTF8, "application/json");
                }

            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error(
                    string.Format("Greška kod preuzimanja podataka documentID: {0}", documentID));
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                objResult = new Result
                {
                    Status = string.Empty,
                    Message = ex.Message
                };
                response = Request.CreateResponse(HttpStatusCode.BadRequest, objResult);
            }
            finally
            {
                if (application != null)
                {
                    _logger.Info("OnBase Disconnect()");
                    var obj = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                    obj.OnBaseDisconnect(application);
                }
            }

            return response;
        }
        #endregion

        #region DocumentKeywordsRemove
        /// <summary>
        /// Vraća ključne reči određenog dokumenta
        /// </summary>
        /// <param name="documentID">Identifikator dokumenta</param>
        /// <param name="keywordTypeName">Naziv ključne reči koju uklanjamo</param>
        /// <returns></returns>
        [HttpDelete, ActionName("DocumentKeywordsRemove")]
        public HttpResponseMessage DocumentKeywordsRemove(long documentID, string keywordTypeName)
        {
            Application application = null;
            Result objResult;

            try
            {
                //_logger.Info("Ide zaključavaje");
                lock (_documentLock)
                {
                    {
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

                        _logger.Info("Pokušaj preuzimanja svojstava dokumenta...");
                        Document document = application.Core.GetDocumentByID(documentID);
                        if (document == null)
                        {
                            _logger.Error(string.Format("Dokument za DocumentID={0} nije pronađen!", documentID));
                            throw new Exception("Dokument nije pronađen!");
                        }

                        var objUDF = new Algotech.UTL.AlgotechUnityUDF.UnityUDF();
                        if (objUDF.RemoveKeywordRecord(application, document, keywordTypeName) == true)
                        {
                            objResult = new Result
                            {
                                Status = "True",
                                Message = "Ključna reč je uspešno obrisana."
                            };
                            response = Request.CreateResponse(HttpStatusCode.OK, objResult);
                        }
                        else
                        {
                            objResult = new Result
                            {
                                Status = "False",
                                Message = "Ključna reč nije uspešno obrisana. Vrednost ključne reči nije postojala."
                            };
                            // Ako pokuša da obriše vrednost ključne reči koje nema nema ni greške
                            // response = Request.CreateResponse(HttpStatusCode.BadRequest, objResult);
                            // Equivalent to HTTP status 100.
                            // Continue indicates that the client can continue with its request.Equivalent to HTTP status 201.
                            // Created indicates that the request resulted in a new resource created before the response was sent.
                            response = Request.CreateResponse(HttpStatusCode.Continue, objResult);
                        }
                    }
                    _logger.Info("Završeno zaključavaje");
                }
            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error(string.Format("Greška kod brisanja podataka o ključnoj reči za documentID: {0}", documentID));
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                objResult = new Result
                {
                    Status = string.Empty,
                    Message = ex.Message
                };
                response = Request.CreateResponse(HttpStatusCode.BadRequest, objResult);
            }
            finally
            {
                if (application != null)
                {
                    _logger.Info("OnBase Disconnect()");
                    var obj = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                    obj.OnBaseDisconnect(application);
                }
            }

            return response;
        }
        #endregion
    }
}
