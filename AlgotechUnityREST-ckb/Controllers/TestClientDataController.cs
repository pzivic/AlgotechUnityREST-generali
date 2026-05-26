using AlgotechUnityREST_ckb.Models;

using System;

using System.Net;
using System.Net.Http;

using System.Web.Http;


namespace AlgotechUnityREST_ckb.Controllers
{
    /// <summary>
    /// Test kontroler
    /// </summary>
    public class TestClientDataController : ApiController
    {
        #region Variable 
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region ClientData_Response
        /// <summary>
        /// Da pojednostavimo response
        /// </summary>
        public class ClientData_Response
        {
            /// <summary>
            /// PIB
            /// </summary>
            public string PIB { get; set; }
            /// <summary>
            /// Matični broj
            /// </summary>
            public string MBR { get; set; }
            /// <summary>
            /// Naziv klijenta
            /// </summary>
            public string ClientName { get; set; }
            
            /// <summary>
            /// Konstruktor
            /// </summary>
            public ClientData_Response()
            {
                PIB = string.Empty;
                MBR = string.Empty;
                ClientName = string.Empty;
            }
        }
        #endregion

        //#region ClientData
        ///// <summary>
        ///// Za requestID vraća tekuću fazu radnog toka
        ///// </summary>
        ///// <param name="PIB">Identifikacioni broj</param>
        ///// <returns></returns>
        ///// <remarks>835657</remarks>
        //[HttpGet, ActionName("ClientData")]
        //public HttpResponseMessage ClientData(string PIB)
        //{
        //    HttpResponseMessage response;
        //    Result result;
        //    string json;
        //    var dateTimeStart = DateTime.Now;

        //    // Defalt error code
        //    var httpStatusCode = HttpStatusCode.BadRequest;
        //    result = new Result
        //    {
        //        Status = string.Empty,
        //        Message = string.Empty,
        //    };
        //    response = Request.CreateResponse(httpStatusCode, result);
        //    //.

        //    try
        //    {
        //        // Metrika
        //        _logger.Info("Metoda:: ClientData start " + dateTimeStart);

        //        var sqlHelper = new DataSystem.DAL.SqlHelper.SqlHelper("connection_string_mssql");

        //        // Napravi sql string
        //        var sqlString =
        //        string.Format(@"SELECT EntityLegal.EntityID, EntityLegal.MBR, EntityLegal.Name, EntityLegal.EmailAddress, EntityLegal.TelephoneNo, EntityLegal.Address, EntityLegal.City, EntityLegal.ZIP, EntityLegal.OrgUnitID FROM Entity INNER JOIN EntityLegal ON Entity.EntityID = EntityLegal.EntityID WHERE EntityLegal.PIB={0}", PIB);

        //        _logger.Info(string.Format("SQL string: {0}", sqlString));

        //        // Pukni upit
        //        var reader = sqlHelper.ExecuteReader(sqlString);

        //        // Napravi Response
        //        var obj = new ClientData_Response();
        //        if (reader.Read())
        //        {
        //            obj.PIB = Convert.ToString(reader["PIB"]).Trim();
        //            obj.MBR = Convert.ToString(reader["MBR"]).Trim();
        //            obj.ClientName = Convert.ToString(reader["Name"]).Trim();
        //        }
        //        reader.Close();
        //        reader.Dispose();
        //        //.

        //        // Rezultat objekat koji vraćamo serijalizacijom
        //        json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

        //        if (json != string.Empty)
        //        {
        //            result = new Result
        //            {
        //                Status = json,
        //                Message = "Podacu su uspešno preuzeti."
        //            };

        //            _logger.Info("Resposne OK " + result);
        //            response = Request.CreateResponse(HttpStatusCode.OK, result);
        //            response.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        // Logger
        //        _logger.Error("Greška kod preuzimaja podataka o klijentu iz Core sistema.");
        //        _logger.Error(ex.Message);
        //        _logger.Error(ex.StackTrace);
        //        //.

        //        result = new Result
        //        {
        //            Status = "Greška",
        //            Message = ex.Message
        //        };
        //        response = Request.CreateResponse(httpStatusCode, result);
        //    }
        //    finally
        //    {
        //        // Metrika end
        //        var dateTimeEnd = DateTime.Now;
        //        var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
        //        _logger.Info("Metoda:: ClientData finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());
        //    }
        //    return response;
        //}
        //#endregion

        #region ClientDataSearch
        /// <summary>
        /// Pretraga evidencije klijenata
        /// </summary>
        /// <param name="clientSearch">Informacija za pretragu</param>
        /// <param name="searchMode">Način pretrage</param>
        /// <returns></returns>
        /// <remarks>101833416</remarks>
        [HttpGet, ActionName("ClientDataSearch")]
        public HttpResponseMessage ClientDataSearch(string clientSearch, int searchMode = 1)
        {
            HttpResponseMessage response;
            Result result;
            string json;
            var dateTimeStart = DateTime.Now;

            // Defalt error code
            var httpStatusCode = HttpStatusCode.BadRequest;
            result = new Result
            {
                Status = string.Empty,
                Message = string.Empty,
            };
            response = Request.CreateResponse(httpStatusCode, result);
            //.

            try
            {
                // Metrika
                _logger.Info("Metoda:: ClientDataSearch start " + dateTimeStart);

                short maxClients = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["MAX_CLIENTS_TO_RETURN"]);
                if (maxClients == 0)
                {
                    maxClients = 10;
                }

                var sqlHelper = new DataSystem.DAL.SqlHelper.SqlHelper("connection_string_mssql");

                // Napravi sql string
                var sqlString = string.Empty;
                switch (searchMode)
                {
                    case 1:
                        //                        sqlString = string.Format(@"SELECT acName2 AS EntityName, acCode AS PIB, acRegNo AS MBR, UPPER(acSubject) AS EntityNameShort, UPPER(acAddress) AS Adress, acPost AS Post, acCountry AS Country FROM tHE_SetSubj
                        //WHERE  (LEN(acCode) = 9) AND (acActive = 'T') AND (acSubject NOT IN ('001', '001 Alisa', '002', '076')) AND (acCode='{0}')
                        //ORDER BY acSubject", clientSearch, maxClients);

                        sqlString = string.Format(@"SELECT acName2 AS EntityName, acCode AS PIB, acRegNo AS MBR, UPPER(acSubject) AS EntityNameShort, UPPER(acAddress) AS Adress, acPost AS Post, acCountry AS Country FROM tHE_SetSubj
                        WHERE  (LEN(acCode) > 7) AND (acActive = 'T') AND (acSubject NOT IN ('001', '001 Alisa', '002', '076')) AND (acCode='{0}')
                        ORDER BY acSubject", clientSearch, maxClients);

                        break;
                    case 2:
                        //                        sqlString = string.Format(@"SELECT acName2 AS EntityName, acCode AS PIB, acRegNo AS MBR, UPPER(acSubject) AS EntityNameShort, UPPER(acAddress) AS Adress, acPost AS Post, acCountry AS Country FROM tHE_SetSubj
                        //WHERE  (LEN(acCode) = 9) AND (acActive = 'T') AND (acSubject NOT IN ('001', '001 Alisa', '002', '076')) AND (acName2 LIKE '%{0}%')
                        //ORDER BY acSubject", clientSearch, maxClients);
                        sqlString = string.Format(@"SELECT acName2 AS EntityName, acCode AS PIB, acRegNo AS MBR, UPPER(acSubject) AS EntityNameShort, UPPER(acAddress) AS Adress, acPost AS Post, acCountry AS Country FROM tHE_SetSubj
                        WHERE  (LEN(acCode) > 7) AND (acActive = 'T') AND (acSubject NOT IN ('001', '001 Alisa', '002', '076')) AND (acName2 LIKE '%{0}%')
                        ORDER BY acSubject", clientSearch, maxClients);
                        break;
                }
                //.

                _logger.Info(string.Format("SQL string: {0}", sqlString));

                // Pukni upit
                var reader = sqlHelper.ExecuteReader(sqlString);

                // Napravi Response
                var obj = new ClientData_Response();
                if (reader.Read())
                {
                    obj.ClientName = Convert.ToString(reader["EntityNameShort"]).Trim();
                    obj.PIB = Convert.ToString(reader["PIB"]).Trim();
                    obj.MBR = Convert.ToString(reader["MBR"]).Trim();
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
                    Status = "Greška",
                    Message = ex.Message
                };
                response = Request.CreateResponse(httpStatusCode, result);
            }
            finally
            {
                // Metrika end
                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
                _logger.Info("Metoda:: ClientDataSearch finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());
            }
            return response;
        }
        #endregion
    }
}
