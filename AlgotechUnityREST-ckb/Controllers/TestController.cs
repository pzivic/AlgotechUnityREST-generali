// Predrag Živić
// 16.04.2024

using System;
using System.Web.Http;
using System.Net.Http;
using System.Net;

using AlgotechUnityREST.Models;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Web.Http.Description;

using DataSystem.DAL.SqlHelper;

namespace AlgotechUnityREST_ckb.Controllers
{
    /// <summary>
    /// Test kontroler
    /// </summary>
    [ApiExplorerSettings(IgnoreApi=true)]
    public class TestController : ApiController
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
        /// Vraća listu aktivnih članova grupa 
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
                var sqlHelper = new SqlHelper("connection_string_mssql");

                // Izvrši proceduru
                var sqlParameter = new SqlParameter[2];

                sqlParameter[0] = new SqlParameter("@ClientIdentifier", clientIdentifier);
                sqlParameter[1] = new SqlParameter("@ClientType", clientType);

                var storedProcName = "uspGetGroupInfoListByName";
                //_logger.Info(string.Format("Oracle plSqlProcName: {0} param value clientIdentifier: {1} param value clientType: {2}.", plSqlProcName, clientIdentifier, clientType));
                var reader = sqlHelper.ExecuteReader(storedProcName, sqlParameter);

                var list = new List<GroupInfoResult>();
                while (reader.Read())
                {
                    var obj = new GroupInfoResult()
                    {
                        GroupID = Convert.ToString(reader["GroupID"]).Trim(),
                        Type = Convert.ToString(reader["Type"]).Trim(),
                        MBR = Convert.ToString(reader["MBR"]).Trim(),
                        TypeK = Convert.ToString(reader["TypeK"]).Trim(),
                        Name = Convert.ToString(reader["Name"]).Trim(),
                        ValidFrom = Convert.ToString(reader["ValidFrom"]).Trim(),
                        CriteriaID = Convert.ToString(reader["CriteriaID"]).Trim(),
                        CriteriaID_Description = Convert.ToString(reader["CriteriaID_Description"]).Trim(),
                        Comment = Convert.ToString(reader["Comment"]).Trim()
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
