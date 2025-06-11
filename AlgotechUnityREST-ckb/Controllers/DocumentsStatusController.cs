// Predrag Živić
// 11.06.2025

using System;
using System.Web.Http;

using Hyland.Unity;

using System.Linq;
using System.Net.Http;
using System.Net;


namespace AlgotechUnityREST_ckb.Controllers
{
    /// <summary>
    /// Status dokumenata
    /// </summary>
    public class DocumentsStatusController : ApiController
    {
        #region Variable  
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //HttpResponseMessage response;
        #endregion

        # region GetApprovalStatusResult
        private class GetApprovalStatusResult
        {
            public string ApprovalStatus { get; set; }
            public GetApprovalStatusResult()
            {
                ApprovalStatus = string.Empty;
            }
        }
        #endregion

        #region GetApprovalStatus
        /// <summary>
        /// Za ulazne parametre vraća status odobravanja
        /// </summary>
        /// <param name="corporateContractID">Broj ugovora</param>
        /// <param name="corporateAnnexMG">Broj anexa</param>
        /// <param name="corporateCreditControlPhase">Naziv faze</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        //[HttpGet, ActionName("GetApprovalStatus")]
        [HttpGet]
        [Route("GetApprovalStatus")]
        public HttpResponseMessage GetApprovalStatus(string corporateContractID, string corporateAnnexMG, string corporateCreditControlPhase)
        {
            HttpResponseMessage responseMessage;
            Application application = null;

            string json;
            var httpStatusCode = HttpStatusCode.Forbidden;
            var dateTimeStart = DateTime.Now;

            var result = new GetApprovalStatusResult(); // konstruktor radi svoje
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

                if (corporateContractID == string.Empty) { throw new ArgumentException("Parametar corporateContractID mora da bude prosleđen"); }
                if (corporateAnnexMG == string.Empty) { throw new ArgumentException("Parametar corporateAnnexMG mora da bude prosleđen"); }
                if (corporateCreditControlPhase == string.Empty) { throw new ArgumentException("Parametar corporateCreditControlPhase mora da bude prosleđen"); }

                _logger.Debug("Početak GetApprovalStatus: " + dateTimeStart);

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

                DocumentQuery docQuery = application.Core.CreateDocumentQuery();

                // 
                _logger.Debug("Provera da li imamo DocumentType");
                DocumentType documentType = application.Core.DocumentTypes.Find(documentTypeName);
                if (documentType == null)
                {
                    _logger.Error(string.Format("Nije pronađen DocumentType '{0}'.", documentTypeName));
                    throw new Exception(string.Format("Nije pronađen DocumentType '{0}'.", documentTypeName));
                }
                docQuery.AddDocumentType(documentType);
                docQuery.AddKeyword("CorporateContractID", corporateContractID, KeywordOperator.Equal, KeywordRelation.Or);
                docQuery.AddKeyword("CorporateAnnexMG", corporateAnnexMG, KeywordOperator.Equal, KeywordRelation.Or);
                docQuery.AddKeyword("CorporateCreditControlPhase", corporateCreditControlPhase, KeywordOperator.Equal, KeywordRelation.Or);

                DocumentList documents = docQuery.Execute(10);
                if (documents.Count == 0)
                {
                    _logger.Error(string.Format("Br.ugovora: {0} anex: {1} faza: {2} nije pronađen za DocumentType '{1}'!", corporateContractID, corporateAnnexMG, corporateCreditControlPhase, documentTypeName));
                    throw new Exception(string.Format("Br.ugovora: {0} anex: {1} faza: {2} nije pronađen za DocumentType '{1}'!", corporateContractID, corporateAnnexMG, corporateCreditControlPhase, documentTypeName));
                }
                if (documents.Count > 1)
                {
                    _logger.Error(string.Format("Br.ugovora: {0} anex: {1} faza: {2} nije pronađen za DocumentType '{1}'!", corporateContractID, corporateAnnexMG, corporateCreditControlPhase, documentTypeName));
                    throw new Exception(string.Format("Br.ugovora: {0} anex: {1} faza: {2} nije pronađen za DocumentType '{1}'!", corporateContractID, corporateAnnexMG, corporateCreditControlPhase, documentTypeName));
                }

                // Pronađi vrednost ključne reči "Request Status" iz prvog dokumenta koji je sadrži
                var requestStatusKeywordValue = string.Empty;
                foreach (var item in documents)
                {
                    var requestStatusKeyword = item.KeywordRecords?
                        .SelectMany(kr => kr.Keywords ?? Enumerable.Empty<Keyword>())
                        .OrderBy(k => k.KeywordType?.KeysetID)
                        .FirstOrDefault(k => k.KeywordType?.Name == "Request Status");

                    if (requestStatusKeyword != null)
                    {
                        requestStatusKeywordValue = requestStatusKeyword.Value?.ToString();
                        break;
                    }
                }
                var actualPhaseValue = string.Empty;
                foreach (var item in documents)
                {
                    var actualPhaseKeyword = item.KeywordRecords?
                        .SelectMany(kr => kr.Keywords ?? Enumerable.Empty<Keyword>())
                        .OrderBy(k => k.KeywordType?.KeysetID)
                        .FirstOrDefault(k => k.KeywordType?.Name == "CorporateCreditControlPhase");

                    if (actualPhaseValue != null)
                    {
                        actualPhaseValue = actualPhaseKeyword.Value?.ToString();
                        break;
                    }
                }

                var getApprovalStatusResult = new GetApprovalStatusResult();
                getApprovalStatusResult.ApprovalStatus = GetFinalStatus(corporateCreditControlPhase, actualPhaseValue, requestStatusKeywordValue);
                //..

                // Rezultat objekat koji vraćamo serijalizacijom
                json = Newtonsoft.Json.JsonConvert.SerializeObject(getApprovalStatusResult.ApprovalStatus);

                if (json != string.Empty)
                {
                    responseMessage = Request.CreateResponse(HttpStatusCode.OK, result);
                    responseMessage.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                }
            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error("Greška kod preuzimaja podataka o klijentu iz Core sistema.");
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                responseMessage = Request.CreateResponse(httpStatusCode, result);
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
        private string GetFinalStatus(string requestedPhase, string actualPhase, string requestStatus)
        {
            //switch (requestedPhase)
            //{
            //    case "FAZA I":
            //        switch (requestStatus)
            //        {
            //            case "PROVERA RAZLOGA ZA VRACANJE OD STRANE BO TIMA":
            //            case "PRVA FAZA KREDITNE KONTROLE - DOPUNA":
            //            case "DOPUNA DOSIJEA - PRVA FAZA RM":
            //                return "VRAĆEN NA DOPUNU";

            //            case "PRIPREMA DOKUMENTACIJE ZA UGOVARANJE BO":
            //            case "POTPISIVANJE UGOVORNE DOKUMENTACIJE RM":
            //                return "ODOBRENO";

            //            case "POVUČEN PLASMAN":
            //                return "ODBIJENO";

            //            default:
            //                return null;
            //        }

            //    case "FAZA II":
            //        switch (requestStatus)
            //        {
            //            case "PROVERA RAZLOGA ZA VRACANJE BO TIM - FAZA II":
            //            case "DOPUNA DOSIJEA - DRUGA FAZA":
            //            case "DRUGA FAZA KREDITNE KONTROLE - DOPUNA":
            //                return "VRACEN NA DOPUNU";

            //            case "AKTIVACIJA KREDITA - ADMINISTRACIJA":
            //            case "AKTIVACIJA KREDITA - REALIZACIJA MANAGER":
            //            case "AKTIVACIJA KREDITA - REALIZACIJA REFERENT":
            //            case "AKTIVACIJA KREDITA - REALIZACIJA CORPSUPPORT":
            //            case "REALIZOVAN PLASMAN":
            //                return "ODOBRENO";

            //            case "POVUČEN PLASMAN":
            //                return "ODBIJENO";

            //            default:
            //                return null;
            //        }

            //    default:
            //        return null;

            var finalStatus = string.Empty;
            var strSQL = string.Format("SELECT FinalStatus FROM DocumentStatusMapping WHERE RequestedPhase = '{0}' AND ActualPhase = '{1}' AND RequestStatus = '{2}'", requestedPhase, actualPhase, requestStatus);

            return finalStatus;
        }
        #endregion

    }
}