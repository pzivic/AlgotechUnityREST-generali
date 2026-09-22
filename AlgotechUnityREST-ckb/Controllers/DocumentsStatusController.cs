// Predrag Živić
// 11.06.2025

using System;
using System.Web.Http;

using Hyland.Unity;

using System.Linq;
using System.Net.Http;
using System.Net;

using DataSystem.DAL.SqlHelper;
using AlgotechUnityREST_ckb.Models;

namespace AlgotechUnityREST_ckb.Controllers
{
    /// <summary>
    /// Status dokumenata
    /// </summary>
    [Authorize]
    public class DocumentsStatusController : ApiController
    {
        #region Variable  
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region GetApprovalStatus
        /// <summary>
        /// Za ulazne parametre vraća status odobravanja
        /// </summary>
        /// <param name="corporateCreditControlPhase">Naziv faze</param>
        /// <param name="corporateContractID">Broj ugovora</param>
        /// <param name="corporateAnnexMG">Broj anexa</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        //[HttpGet, ActionName("GetApprovalStatus")]
        [HttpGet]
        [Route("api/DocumentsStatus/GetApprovalStatus")]
        public HttpResponseMessage GetApprovalStatus(string corporateCreditControlPhase, string corporateContractID, string corporateAnnexMG = "")
        {
            HttpResponseMessage responseMessage;
            Application application = null;

            string json;
            var httpStatusCode = HttpStatusCode.Forbidden;
            var dateTimeStart = DateTime.Now;

            var result = new Result();
            responseMessage = Request.CreateResponse(httpStatusCode, result);

            if (corporateCreditControlPhase == "1")
            {
                corporateCreditControlPhase = "FAZA I";
            }
            else if (corporateCreditControlPhase == "2")
            {
                corporateCreditControlPhase = "FAZA II";
            }

            try
            {
                var documentTypeName = "Zahtev za Ugovor Corporate - exZAK";

                if (corporateCreditControlPhase == string.Empty) { throw new ArgumentException("Parametar corporateCreditControlPhase mora da bude prosleđen"); }
                if (corporateContractID == string.Empty) { throw new ArgumentException("Parametar corporateContractID mora da bude prosleđen"); }
                //if (corporateAnnexMG == string.Empty) { throw new ArgumentException("Parametar corporateAnnexMG mora da bude prosleđen"); }

                _logger.Debug("Početak GetApprovalStatus: " + dateTimeStart);

                // Konekcija na OnBase
                _logger.Info("GetApprovalStatus OnBase Connect()");
                var obj = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                application = obj.OnBaseConnect();
                if (application == null)
                {
                    _logger.Warn("Greška kod uspostavljanja konekcije sa OnBase");
                    throw new ArgumentException("Greška kod uspostavljanja konekcije sa OnBase. Kontaktirajte OnBase administratora.");
                }
                //.

                DocumentQuery docQuery = application.Core.CreateDocumentQuery();
                // 
                _logger.Debug("Provera da li imamo DocumentType");
                DocumentType documentType = application.Core.DocumentTypes.Find(documentTypeName);
                if (documentType == null)
                {
                    _logger.Error(string.Format("Nije pronađen DocumentType '{0}'.", documentTypeName));
                    throw new Exception(string.Format("Nije pronađen DocumentType '{0}'.", documentTypeName));
                }

                // Pravimo filter sa klučnim rečima
                docQuery.AddDocumentType(documentType);

                docQuery.AddKeyword("CorporateContractID", corporateContractID);
                if (!string.IsNullOrWhiteSpace(corporateAnnexMG))
                {
                    docQuery.AddKeyword("CorporateAnnexMG", corporateAnnexMG, KeywordOperator.Equal, KeywordRelation.And);
                }

                //if (!string.IsNullOrWhiteSpace(corporateAnnexMG) && corporateAnnexMG.Trim() != "0")
                //{
                //    docQuery.AddKeyword("CorporateAnnexMG", corporateAnnexMG.Trim(), KeywordOperator.Equal, KeywordRelation.And);
                //}

                docQuery.AddKeyword("CorporatePlacementType", "LIZING", KeywordOperator.Equal, KeywordRelation.Or);
                docQuery.AddKeyword("CorporatePlacementType", "ANEKS LIZINGA", KeywordOperator.Equal, KeywordRelation.Or);

                //// Ispis za debug
                //foreach (var filter in docQuery.add)
                //{
                //    _logger.Debug("Filter dodat: " + filter);
                //}

                DocumentList documents = docQuery.Execute(10);
                if (documents.Count == 0)
                {
                    _logger.Error(string.Format("Br.ugovora: '{0}' anex: '{1}' nije pronađen za DocumentType '{2}'!", corporateContractID, corporateAnnexMG, documentTypeName));
                    throw new Exception(string.Format("Br.ugovora: '{0}' anex: '{1}' nije pronađen za DocumentType '{2}'!", corporateContractID, corporateAnnexMG, documentTypeName));
                }
                if (documents.Count > 1)
                {
                    _logger.Error(string.Format("Br.ugovora: '{0}' anex: '{1}' nije jedinstven za DocumentType '{2}'!", corporateContractID, corporateAnnexMG, documentTypeName));
                    throw new Exception(string.Format("Br.ugovora: '{0}' anex: '{1}' nije jedinstven za DocumentType '{2}'!", corporateContractID, corporateAnnexMG, documentTypeName));
                }

                // Pronađi vrednost ključne reči "Request Status" 
                _logger.Debug("Pronađi vrednost ključne reči Request Status");
                var requestStatusKeywordValue = string.Empty;
                foreach (var item in documents)
                {
                    var requestStatusKeyword = item.KeywordRecords?
                        .SelectMany(kr => kr.Keywords ?? Enumerable.Empty<Keyword>())
                        .OrderBy(k => k.KeywordType?.KeysetID)
                        .FirstOrDefault(k => k.KeywordType?.Name == "Request Status");

                    if (requestStatusKeyword != null)
                    {
                        requestStatusKeywordValue = requestStatusKeyword.Value?.ToString() ?? string.Empty;
                        break;
                    }
                }
                _logger.Debug("Request Status: " + requestStatusKeywordValue);

                // Pronađi vrednost ključne reči "CorporateCreditControlPhase" 
                _logger.Debug("Pronađi vrednost ključne reči CorporateCreditControlPhase");
                var actualPhaseKeywordValue = string.Empty;
                foreach (var item in documents)
                {
                    var actualPhaseKeyword = item.KeywordRecords?
                        .SelectMany(kr => kr.Keywords ?? Enumerable.Empty<Keyword>())
                        .OrderBy(k => k.KeywordType?.KeysetID)
                        .FirstOrDefault(k => k.KeywordType?.Name == "CorporateCreditControlPhase");

                    if (actualPhaseKeyword != null)
                    {
                        actualPhaseKeywordValue = actualPhaseKeyword.Value?.ToString() ?? string.Empty;
                        break;
                    }
                }
                _logger.Debug("CorporateCreditControlPhase: " + actualPhaseKeywordValue);

                _logger.Debug("Uradi mapiranje statusa");
                 var approvalStatus = GetFinalStatus(corporateCreditControlPhase, actualPhaseKeywordValue, requestStatusKeywordValue);
                _logger.Debug("Vraćamo: " + approvalStatus);

                string[] parts = approvalStatus.Split('–');

                // Formiraj response
                result = new Result
                {
                    Status = parts.Length > 1 ? parts[0].Trim() : approvalStatus.Trim(),
                    Message = approvalStatus
                };
                responseMessage = Request.CreateResponse(httpStatusCode, result);
                //.

                // Rezultat objekat koji vraćamo serijalizacijom
                json = Newtonsoft.Json.JsonConvert.SerializeObject(result);

                if (json != string.Empty)
                {
                    responseMessage = Request.CreateResponse(HttpStatusCode.OK, result);
                    responseMessage.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                }
            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error("Greška kod preuzimaja podataka o statusu dokumenta.");
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                // Formiraj response
                var resultValue = new Result
                {
                    Status = httpStatusCode.ToString(),
                    Message = ex.Message
                };
                responseMessage = Request.CreateResponse(httpStatusCode, resultValue);
                //.
            }

            finally
            {
                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
                _logger.Debug(string.Format("GetApprovalStatus: finally {0} in sec {1}.", dateTimeEnd, diffInSeconds));

                if (application != null)
                {
                    _logger.Info(string.Format("GetApprovalStatus OnBase Disconnect() Time: {0}; OnBase SessionID: {1}.", dateTimeEnd, application.SessionID));
                    var obj = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                    obj.OnBaseDisconnect(application);
                }
            }
            return responseMessage;
        }
        #endregion

        #region GetFinalStatus
        /// <summary>
        /// Preuzima final staus iz baze podataka
        /// </summary>
        /// <param name="requestedPhase"></param>
        /// <param name="actualPhase"></param>
        /// <param name="requestStatus"></param>
        /// <returns></returns>
        private string GetFinalStatus(string requestedPhase, string actualPhase, string requestStatus)
        {
            _logger.Debug("GetFinalStatus START");

            var sqlHelper = new SqlHelper("connection_string_mssql");
            try
            {
                var sqlString = string.Format(
                    "SELECT FinalStatus FROM DocumentStatusMapping WHERE RequestedPhase=N'{0}' AND ActualPhase=N'{1}' AND RequestStatus=N'{2}';",
                    requestedPhase, actualPhase, requestStatus
                );

                _logger.Debug($"SQL string: {sqlString}");

                var obj = sqlHelper.ExecuteScalar(sqlString);

                var finalStatus = (obj == null || obj == DBNull.Value)
                    ? string.Empty
                    : obj.ToString();

                if (string.IsNullOrEmpty(finalStatus))
                {
                    _logger.Debug($"Nemamo kombinaciju: RequestedPhase={requestedPhase}; ActualPhase={actualPhase}; RequestStatus={requestStatus}!");

                    if ((requestedPhase == "FAZA I" && actualPhase == "FAZA I") ||
                        (requestedPhase == "FAZA II" && actualPhase == "FAZA II"))
                    {
                        return "N/A";
                    }
                    if (requestedPhase == "FAZA I" && actualPhase == "FAZA II")
                    {
                        return "ODOBRENO - Kredit je prošao fazu KK iz tačke 3";
                    }
                }

                return finalStatus;

            }
            catch (Exception ex)
            {
                _logger.Error("Greška u GetFinalStatus metodi", ex);
                return string.Empty;
            }
            finally
            {
                _logger.Debug("GetFinalStatus END");
            }
        }
        #endregion
    }
}