// Predrag Živić
// 12.04.2024

using System;
using System.Web.Http;
using System.Net.Http;
using System.Net;

using System.Data;
using System.Collections.Generic;
using System.Web.Http.Description;

using Oracle.ManagedDataAccess.Client;

using AlgotechUnityREST_ckb.Models;

namespace AlgotechUnityREST_ckb.Controllers
{
    /// <summary>
    /// Preuzimaje podataka iz Core sistema o grupama
    /// </summary>
    [ApiExplorerSettings(IgnoreApi=false)]
    public class CoreSystemGroupController : ApiController
    {
        #region Variable 
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        HttpResponseMessage response;
        #endregion

        #region GroupInfo
        /// <summary>
        /// Privatna klasa za vraćanje rezultata
        /// </summary>
        private class GroupInfoResult
        {
            public string GroupID { get; set; }
            public string Type { get; set; }
            public string MBR { get; set; }
            public string TypeK { get; set; }
            public string Name { get; set; }
            public string ValidFrom { get; set; }
            public string CriteriaID { get; set; }
            public string CriteriaID_Description { get; set; }
            public string Comment { get; set; }
        }

        /// <summary>
        /// Vraća listu aktivnih članova grupa (nosilac ili član) kojoj pripada identifikator klijenta.
        /// </summary>
        /// <param name="clientType">Tip klijenta</param>
        /// <param name="clientIdentifier">Identifikatgor klijenta npr. PIB</param>
        /// <returns></returns>
        [HttpGet, ActionName("GroupInfo")]
        public HttpResponseMessage GroupInfo(string clientIdentifier, short clientType)
        {
            Result result;
            string json;

            // Defalt error code
            var httpStatusCode = HttpStatusCode.Forbidden;

            try
            {
                _logger.Info("Metoda GroupInfo");
                _logger.Info("Konektujem Oracle");
                var sqlHelper = new Algotech.DAL.OracleHelper.OracleHelper("connection_string_oracle");

                // Izvrši proceduru
                var oracleParameter = new OracleParameter[3];

                oracleParameter[0] = new OracleParameter("Result", OracleDbType.RefCursor, ParameterDirection.ReturnValue);
                oracleParameter[1] = new OracleParameter("cClient_Identifier", clientIdentifier);
                oracleParameter[2] = new OracleParameter("nClient_Type", clientType);

                var plSqlProcName = "Dms.dms_tobiis_getdata.get_group_info";
                _logger.Info(string.Format("Oracle plSqlProcName: {0} param value clientIdentifier: {1} param value clientType: {2}.", plSqlProcName, clientIdentifier, clientType));
                var reader = sqlHelper.ExecuteReader(plSqlProcName, oracleParameter);

                var list = new List<GroupInfoResult>();
                while (reader.Read())
                {
                    var obj = new GroupInfoResult()
                    {
                        GroupID = Convert.ToString(reader["grupa_id"]).Trim(),
                        Type = Convert.ToString(reader["tip"]).Trim(),
                        MBR = Convert.ToString(reader["mbr"]).Trim(),
                        TypeK = Convert.ToString(reader["tipk"]).Trim(),
                        Name = Convert.ToString(reader["naziv"]).Trim(),
                        ValidFrom = Convert.ToString(reader["vazi_od"]).Trim(),
                        CriteriaID = Convert.ToString(reader["criteria_id"]).Trim(),
                        CriteriaID_Description = Convert.ToString(reader["criteria_id_opis"]).Trim(),
                        Comment = Convert.ToString(reader["komentar"]).Trim()
                    };
                    list.Add(obj);
                }
                reader.Close();
                reader.Dispose();
                //.

                // Rezultat objekat koji vraćamo serijalizacijom
                json = Newtonsoft.Json.JsonConvert.SerializeObject(list);

                if (json != string.Empty)
                {
                    result = new Result
                    {
                        Status = json,
                        Message = "Podacu su uspešno preuzeti."
                    };

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