// Predrag Živić
// 20.02.2024
// Generator sekvenci

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using AlgotechUnityREST_ckb.Models;

/*
-- Predrag Živić
-- 05.06.2024

USE OnBaseUTL
GO

CREATE SEQUENCE dbo.RefNoMBP AS int START WITH 1 INCREMENT BY 1;
GO
GRANT UPDATE ON dbo.RefNoMBP TO web_access
GO

-- Resetovanje sekvence na početku godine 01.01.2026
ALTER SEQUENCE dbo.RefNoMBP RESTART WITH 1
 */

namespace AlgotechUnityREST_ckb.Controllers
{
    /// <summary>
    /// Generator sekvenci
    /// </summary>
    [Authorize]
    [ApiExplorerSettings(IgnoreApi=false)]
    public class SequencerController : ApiController
    {
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        HttpResponseMessage response;

        #region SequencerGetValue
        /// <summary>
        /// Kreiranje sekvencera
        /// </summary>
        private class SequencerRespons
        {
            public string RefNo { get; set; }
        }

        /// <summary>
        /// Vraća sekvencer
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <returns></returns>
        [HttpGet, ActionName("GetValue")]
        public HttpResponseMessage GetValue(string prefix)
        {
            Result result;
            string json;

            try
            {
                _logger.Info("Metoda GetValue");
                _logger.Info("Konektujem Sql");
                var sqlHelper = new DataSystem.DAL.SqlHelper.SqlHelper("connection_string_mssql");

                //// Napravi sql string
                //var sqlString = string.Format(@"SELECT CONCAT(FORMAT(NEXT VALUE FOR dbo.RefNo{0}, '{0}-#'), '/{1}') AS ref_no", prefix, DateTime.Now.Year);
                //_logger.Info(string.Format("SQL string: {0}", sqlString));
                //// Pukni upit
                //var reader = sqlHelper.ExecuteReader(sqlString);
                //// napuni objekat 
                //var obj = new SequencerRespons();
                //if (reader.Read())
                //{
                //    obj.RefNo = Convert.ToString(reader["ref_no"]).Trim();
                //}
                //reader.Close();
                //reader.Dispose();
                ////.

                // Napravi sql string
                var sqlString = string.Format(@"SELECT NEXT VALUE FOR RefNo{0} AS seq", prefix);

                _logger.Info(string.Format("SQL string: {0}", sqlString));

                // Pukni upit
                var reader = sqlHelper.ExecuteReader(sqlString);
                // napuni objekat 
                var obj = new SequencerRespons();
                long seq = 0;
                //var getModul97 = string.Empty;
                if (reader.Read())
                {
                    seq = Convert.ToInt64(reader["seq"]);
                }
                reader.Close();
                reader.Dispose();
                //.

                //getModul97 = AlgotechUnityUDF.UnityUDF.GetModul97(prefix + seq + DateTime.Now.Year);
                //obj.RefNo = string.Format("{0}-{1}{2}/{3}", prefix, seq, getModul97, DateTime.Now.Year);
                obj.RefNo = string.Format("{0}-{1}/{2}", prefix, seq, DateTime.Now.Year);

                // Rezultat objekat koji vraćamo serijalizacijom
                json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

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
                _logger.Error("Greška kod preuzimaja podataka o sekvenci.");
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
            return response;

        }
        #endregion

    }
}