// Predrag Živić
// 06.03.2024

using System;
using System.Web.Http;
using System.Net.Http;
using System.Net;

using Oracle.ManagedDataAccess.Client;
using System.Web.Http.Description;

using AlgotechUnityREST_ckb.Models;
using System.Data;

namespace AlgotechUnityREST_ckb.Controllers
{
    /// <summary>
    /// Preuzimaje podataka iz Core sistema o klijentima
    /// </summary>
    [Authorize]
    [ApiExplorerSettings(IgnoreApi=false)]
    public class CoreSystemClientController : ApiController
    {
        #region Variable 
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        HttpResponseMessage response;
        #endregion

        #region ClientInfo
        /// <summary>
        /// Privatna klasa za vraćanje rezultata
        /// </summary>
        private class ClientInfoResult
        {
            public string ClientID { get; set; }
            public string ClientName { get; set; }
            public string CorporateSegment { get; set; }
            public string CorporateRating { get; set; }
        }

        /// <summary>
        /// Vraća naziv klijenta za proslijeđeni par Identifikatgor klijenta-Tip klijenta
        /// </summary>
        /// <param name="clientIdentifier">Identifikatgor klijenta npr. PIB</param>
        /// <param name="clientType">Tip klijenta</param>
        /// <returns></returns>
        [HttpGet, ActionName("ClientInfo")]
        public HttpResponseMessage ClientInfo(string clientIdentifier, short clientType)
        {
            Result result;
            string json;

            // Defalt error code
            var httpStatusCode = HttpStatusCode.Forbidden;

            try
            {
                _logger.Info("Metoda ClientInfo");
                _logger.Info("Konektujem Oracle");
                var sqlHelper = new Algotech.DAL.OracleHelper.OracleHelper("connection_string_oracle");

                // Izvrši proceduru
                var oracleParameter = new OracleParameter[3];

                oracleParameter[0] = new OracleParameter("Result", OracleDbType.RefCursor, ParameterDirection.ReturnValue);
                oracleParameter[1] = new OracleParameter("cClient_Identifier", clientIdentifier);
                oracleParameter[2] = new OracleParameter("nClient_Type", clientType);

                var plSqlProcName = "Dms.dms_tobiis_getdata.get_client_info";
                _logger.Info(string.Format("Oracle plSqlProcName: {0} param value clientIdentifier: {1} param value clientType: {2}.", plSqlProcName, clientIdentifier, clientType));
                var reader = sqlHelper.ExecuteReader(plSqlProcName, oracleParameter);

                var obj = new ClientInfoResult();
                if (reader.Read())
                {
                    _logger.Info("Pokušaj čitanja readera");
                    obj.ClientID = string.Format("{0}-{1}", clientIdentifier, clientType);
                    obj.ClientName = Convert.ToString(reader["NAZIV"]).Trim();
                    obj.CorporateSegment = Convert.ToString(reader["SEGMENTATION_INTERNAL"]).Trim();
                    obj.CorporateRating = reader.HasColumn("REJTING") ? Convert.ToString(reader["REJTING"]).Trim() : null;
                }
                reader.Close();
                reader.Dispose();
                //.

                // Rezultat objekat koji vraćamo serijalizacijom
                json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

                if (json != string.Empty)
                {
                    result = new Result
                    {
                        Status = json,
                        Message = "Podacu su uspešno preuzeti."
                    };

                    _logger.Info("Resposne OK " + result);
                    response = Request.CreateResponse(HttpStatusCode.OK, result);
                    response.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                }

            }
            catch (Exception ex)
            {

                // Logger
                _logger.Error("Greška kod preuzimaja podataka o klijentu iz Core sistema.");
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                result = new Result
                {
                    Status = string.Empty,
                    Message = ex.Message
                };
                response = Request.CreateResponse(httpStatusCode, result);
            }
            return response;

        }
        #endregion
    }
}