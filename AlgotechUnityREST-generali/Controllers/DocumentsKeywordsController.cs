// ============================================================================
// Autor:           Predrag Živić
// Datum kreiranja: 01.03.2025
// Naziv:           Rad sa klučnim rečima dokumenata u OnBase sistemu putem REST servisa
//
// Opis:
//
// Istorija izmena:
// 
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Hyland.Unity;
using AlgotechUnityREST_generali.Models;

namespace AlgotechUnityREST_generali.Controllers
{
    /// <summary>
    /// Operacije nad ključnim rečima dokumenata.
    /// </summary>
    [Authorize]
    public class DocumentsKeywordsController : ApiController
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Serijalizuje brisanja u okviru ovog procesa.
        private static readonly object _documentLock = new object();

        #region DocumentKeywords
        /// <summary>
        /// Vraća ključne reči dokumenta.
        /// </summary>
        /// <param name="documentID">Identifikator dokumenta.</param>
        /// <returns>JSON objekat čija su svojstva nazivi tipova ključnih reči.</returns>
        [HttpGet, ActionName("DocumentKeywords")]
        public HttpResponseMessage DocumentKeywords(long documentID)
        {
            if (documentID <= 0)
            {
                return CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    "DocumentID mora biti veći od nule.");
            }

            Application application = null;

            try
            {
                _logger.Info("Povezivanje sa OnBase.");
                var connection = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                application = connection.OnBaseConnect();

                if (application == null)
                {
                    _logger.Warn("OnBase konekcija nije uspostavljena.");
                    return CreateErrorResponse(
                        HttpStatusCode.ServiceUnavailable,
                        "OnBase servis trenutno nije dostupan.");
                }

                _logger.InfoFormat(
                    "Preuzimanje ključnih reči dokumenta DocumentID={0}.",
                    documentID);

                Document document = application.Core.GetDocumentByID(documentID);
                if (document == null)
                {
                    _logger.WarnFormat(
                        "Dokument DocumentID={0} nije pronađen.",
                        documentID);

                    return CreateErrorResponse(
                        HttpStatusCode.NotFound,
                        "Dokument nije pronađen.");
                }

                var keywordList = new Dictionary<string, string>();

                foreach (KeywordRecord keywordRecord in document.KeywordRecords)
                {
                    foreach (Keyword keyword in keywordRecord.Keywords)
                    {
                        string keywordName = keyword.KeywordType.Name;
                        string keywordValue = Convert.ToString(
                            keyword.Value,
                            CultureInfo.InvariantCulture);

                        // Postojeći API model podržava samo jednu vrednost po nazivu.
                        // Vrati jasnu grešku umesto da prepiše ili izgubi duplu vrednost.
                        if (keywordList.ContainsKey(keywordName))
                        {
                            _logger.WarnFormat(
                                "Dokument DocumentID={0} sadrži više vrednosti za keyword type '{1}'.",
                                documentID,
                                keywordName);

                            return CreateErrorResponse(
                                HttpStatusCode.Conflict,
                                "Dokument sadrži više vrednosti za istu ključnu reč, a ovaj API podržava jednu vrednost po nazivu.");
                        }

                        keywordList.Add(keywordName, keywordValue);
                    }
                }

                // Zadržava postojeći uspešni ugovor: JSON objekat sa nazivima i
                // vrednostima ključnih reči, bez Result omotača.
                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    keywordList,
                    Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    string.Format(
                        "Greška pri preuzimanju ključnih reči za DocumentID={0}.",
                        documentID),
                    ex);

                return CreateErrorResponse(
                    HttpStatusCode.InternalServerError,
                    "Došlo je do greške pri preuzimanju ključnih reči dokumenta.");
            }
            finally
            {
                DisconnectFromOnBase(application);
            }
        }
        #endregion

        #region DocumentKeywordsRemove
        /// <summary>
        /// Briše keyword record iz dokumenta prema nazivu tipa ključne reči.
        /// </summary>
        /// <param name="documentID">Identifikator dokumenta.</param>
        /// <param name="keywordTypeName">Naziv tipa ključne reči koji se briše.</param>
        /// <returns>Rezultat brisanja.</returns>
        [HttpDelete, ActionName("DocumentKeywordsRemove")]
        public HttpResponseMessage DocumentKeywordsRemove(
            long documentID,
            string keywordTypeName)
        {
            if (documentID <= 0)
            {
                return CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    "DocumentID mora biti veći od nule.");
            }

            if (string.IsNullOrWhiteSpace(keywordTypeName))
            {
                return CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    "KeywordTypeName je obavezan.");
            }

            Application application = null;

            try
            {
                lock (_documentLock)
                {
                    _logger.Info("Povezivanje sa OnBase radi brisanja ključne reči.");
                    var connection = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                    application = connection.OnBaseConnect();

                    if (application == null)
                    {
                        _logger.Warn("OnBase konekcija nije uspostavljena.");
                        return CreateErrorResponse(
                            HttpStatusCode.ServiceUnavailable,
                            "OnBase servis trenutno nije dostupan.");
                    }

                    Document document = application.Core.GetDocumentByID(documentID);
                    if (document == null)
                    {
                        _logger.WarnFormat(
                            "Dokument DocumentID={0} nije pronađen.",
                            documentID);

                        return CreateErrorResponse(
                            HttpStatusCode.NotFound,
                            "Dokument nije pronađen.");
                    }

                    var unityUdf = new Algotech.UTL.AlgotechUnityUDF.UnityUDF();
                    bool removed = unityUdf.RemoveKeywordRecord(
                        application,
                        document,
                        keywordTypeName);

                    if (removed)
                    {
                        return Request.CreateResponse(
                            HttpStatusCode.OK,
                            new Result
                            {
                                Status = "True",
                                Message = "Keyword record je uspešno obrisan."
                            });
                    }

                    // Continue (100) nije odgovarajući status za rezultat DELETE zahteva.
                    return Request.CreateResponse(
                        HttpStatusCode.NotFound,
                        new Result
                        {
                            Status = "False",
                            Message = "Keyword record nije pronađen ili nije obrisan."
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    string.Format(
                        "Greška pri brisanju keyword record-a '{0}' za DocumentID={1}.",
                        keywordTypeName,
                        documentID),
                    ex);

                return CreateErrorResponse(
                    HttpStatusCode.InternalServerError,
                    "Došlo je do greške pri brisanju ključne reči dokumenta.");
            }
            finally
            {
                DisconnectFromOnBase(application);
            }
        }
        #endregion

        #region CreateErrorResponse
        private HttpResponseMessage CreateErrorResponse(
            HttpStatusCode statusCode,
            string message)
        {
            return Request.CreateResponse(
                statusCode,
                new Result
                {
                    Status = string.Empty,
                    Message = message
                });
        }
        #endregion

        #region DisconnectFromOnBase
        private static void DisconnectFromOnBase(Application application)
        {
            if (application == null)
            {
                return;
            }

            try
            {
                _logger.Info("Prekidanje OnBase konekcije.");
                var connection = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                connection.OnBaseDisconnect(application);
            }
            catch (Exception ex)
            {
                // Greška pri disconnect-u ne treba da zameni već pripremljen HTTP odgovor.
                _logger.Error("Greška pri prekidanju OnBase konekcije.", ex);
            }
        }
        #endregion
    }
}
