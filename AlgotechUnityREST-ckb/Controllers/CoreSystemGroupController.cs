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
using System.Linq;

namespace AlgotechUnityREST_ckb.Controllers
{
    /// <summary>
    /// Preuzimanje podataka iz Core sistema o grupama
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = false)]
    public class CoreSystemGroupController : ApiController
    {
        #region Variable 
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        HttpResponseMessage response;
        #endregion

        #region GroupInfoResult
        /// <summary>
        /// Privatna klasa za vraćanje podataka o grupi
        /// </summary>
        private class GroupInfoResult
        {
            /// <summary>
            /// Idektifikator grupe
            /// </summary>
            public string GroupID { get; set; }

            /// <summary>
            /// Naziv grupe
            /// </summary>
            public string GroupName { get; set; }
            /// <summary>
            /// Tip
            /// </summary>
            public string Type { get; set; }
            /// <summary>
            /// Matični broj/PIB
            /// </summary>
            public string MBR { get; set; }
            /// <summary>
            /// Tip K (što god to značilo)
            /// </summary>
            public string TypeK { get; set; }
            /// <summary>
            /// Naziv člana (islio sam je naziv grupe)
            /// </summary>
            public string MemberName { get; set; }
            /// <summary>
            /// Važi od
            /// </summary>
            public string ValidFrom { get; set; }
            /// <summary>
            /// Kriterijum
            /// </summary>
            public string CriteriaID { get; set; }
            /// <summary>
            /// Opis kriterijuma
            /// </summary>
            public string CriteriaID_Description { get; set; }
            /// <summary>
            /// Komentar
            /// </summary>
            public string Comment { get; set; }

            /// <summary>
            /// Segment grupe je upitan po poslednjoj informaciji postoji nosilac grupe koji će povući segment
            /// </summary>
            public string GroupSegment { get; set; }

            /// <summary>
            /// Rejting člana
            /// </summary>
            public string MemberRating { get; set; }
        }
        #endregion

        #region GroupInfo 
        /// <summary>
        /// Vraća listu aktivnih članova grupa (nosilac ili član) kojoj pripada identifikator klijenta.
        /// </summary>
        /// <param name="clientIdentifier">Identifikatgor klijenta npr. PIB</param>
        /// <param name="clientType">Tip klijenta</param>
        /// <returns></returns>
        /// <remarks>
        /// Primer vrednosti: 
        /// clientIdentifier = "02280612", clientType = 1
        /// </remarks>        

        [HttpGet, ActionName("GroupInfo")]

        // Ovo mi pravi problem jer onda traži isklučivo preko /
        //[HttpGet, Route("api/CoreSystemGroup/GroupInfo/{clientIdentifier}/{clientType}")]
        //[ActionName("GroupInfo")]
        public HttpResponseMessage GroupInfo(string clientIdentifier, short clientType)
        {
            Result result;
            string json;

            // Defalt error code
            var httpStatusCode = HttpStatusCode.Forbidden;
            // Definisanje početnog status koda za greške chat kaže InternalServerError
            //var httpStatusCode = HttpStatusCode.InternalServerError;

            try
            {
                //_logger.Info("Metoda GroupInfo");
                _logger.Info("Metoda GroupInfo je pozvana.");

                // Validacija ulaznih parametara
                if (string.IsNullOrEmpty(clientIdentifier) || clientIdentifier.Length > 50)
                {
                    _logger.Warn("Nevažeći clientIdentifier.");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Nevažeći clientIdentifier.");
                }
                if (clientType < 0)
                {
                    _logger.Warn("Nevažeći clientType.");
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Nevažeći clientType.");
                }
                //.

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
                        GroupName = Convert.ToString(reader["grupa_naziv"]).Trim(),

                        Type = Convert.ToString(reader["tip"]).Trim(),
                        MBR = Convert.ToString(reader["mbr"]).Trim(),
                        TypeK = Convert.ToString(reader["tipk"]).Trim(),
                        MemberName = Convert.ToString(reader["naziv"]).Trim(),
                        ValidFrom = Convert.ToString(reader["vazi_od"]).Trim(),
                        CriteriaID = Convert.ToString(reader["criteria_id"]).Trim(),
                        CriteriaID_Description = Convert.ToString(reader["criteria_id_opis"]).Trim(),
                        Comment = Convert.ToString(reader["komentar"]).Trim(),

                        //// Nove kolone
                        //GroupName = reader.HasColumn("grupa_naziv") ? Convert.ToString(reader["grupa_naziv"]).Trim() : null,
                        //GroupSegment = reader.HasColumn("segmentation_internal") ? Convert.ToString(reader["segmentation_internal"]).Trim() : null,
                        //MemberRating = reader.HasColumn("rejting_clan") ? Convert.ToString(reader["rejting_clan"]).Trim() : null

                        GroupSegment = Convert.ToString(reader["segmentation_internal"]).Trim(),
                        MemberRating = Convert.ToString(reader["rejting_clan"]).Trim()
                    };
                    list.Add(obj);
                }
                reader.Close();
                reader.Dispose();
                //.

                if (list.Count == 0)
                {
                    result = new Result
                    {
                        Status = string.Empty,
                        Message = "Nema podataka za prosleđene parametre."
                    };
                    response = Request.CreateResponse(HttpStatusCode.OK, result);
                }
                else 
                {  
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

            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error("Greška kod preuzimaja podataka o grupi iz Core sistema.");
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

        #region GroupInfoByID 
        /// <summary>
        /// Vraća listu aktivnih članova grupa (nosilac ili član) kojoj pripada identifikator klijenta sa pretragom po findValue (Name, MBR).
        /// </summary>
        /// <param name="groupID">ID grupe</param>
        /// <param name="findValue">String za pretragu naziv ili PIB</param>
        /// <param name="filterByType">Filtriranje po tipi podrazimevano je N onda se ne vrši pretagana po findValue</param>
        /// <returns></returns>
        /// <remarks>
        /// Primer vrednosti: 
        /// groupID = "46", findValue = "02007819 ili SYSGIHFY OMVOLVAEBUF IQZPWS FJXQ "
        /// </remarks>        
        [HttpGet, ActionName("GetGroupInfoByID")]
        //[HttpGet, Route("api/CoreSystemGroup/GroupInfoByID/{groupID}/{findValue?}/{filterByType?}")]
        //[ActionName("GetGroupInfoByID")]
        public HttpResponseMessage GroupInfoByID(int groupID, string findValue = null, string filterByType = null)
        {
            Result result;
            string json;

            if (findValue == "-")
                findValue=string.Empty;
            if (filterByType == "-")
                filterByType = string.Empty;

            // Defalt error code
            var httpStatusCode = HttpStatusCode.Forbidden;

            try
            {
                _logger.Info("Metoda GroupInfoByID");
                _logger.Info("Konektujem Oracle");
                var sqlHelper = new Algotech.DAL.OracleHelper.OracleHelper("connection_string_oracle");

                // Izvrši proceduru
                var oracleParameter = new OracleParameter[2];

                oracleParameter[0] = new OracleParameter("Result", OracleDbType.RefCursor, ParameterDirection.ReturnValue);
                oracleParameter[1] = new OracleParameter("nGroup_id", groupID);

                var plSqlProcName = "Dms.dms_tobiis_getdata.get_group_info_for_id";
                _logger.Info(string.Format("Oracle procedura plSqlProcName: {0} param value groupID: {1}.", plSqlProcName, groupID));
                var reader = sqlHelper.ExecuteReader(plSqlProcName, oracleParameter);

                var list = new List<GroupInfoResult>();
                while (reader.Read())
                {
                    var obj = new GroupInfoResult()
                    {
                        GroupID = Convert.ToString(reader["grupa_id"]).Trim(),
                        GroupName = Convert.ToString(reader["grupa_naziv"]).Trim(),

                        Type = Convert.ToString(reader["tip"]).Trim(),
                        MBR = Convert.ToString(reader["mbr"]).Trim(),
                        TypeK = Convert.ToString(reader["tipk"]).Trim(),
                        MemberName = Convert.ToString(reader["naziv"]).Trim(),
                        ValidFrom = Convert.ToString(reader["vazi_od"]).Trim(),
                        CriteriaID = Convert.ToString(reader["criteria_id"]).Trim(),
                        CriteriaID_Description = Convert.ToString(reader["criteria_id_opis"]).Trim(),
                        Comment = Convert.ToString(reader["komentar"]).Trim(),

                        //// Nove kolone
                        //GroupName = reader.HasColumn("grupa_naziv") ? Convert.ToString(reader["grupa_naziv"]).Trim() : null,
                        GroupSegment = Convert.ToString(reader["segmentation_internal"]).Trim(),
                        MemberRating = Convert.ToString(reader["rejting_clan"]).Trim() 
                    };
                    list.Add(obj);
                }
                reader.Close();
                reader.Dispose();
                //.

                // Dodaj where uslov
                if (!string.IsNullOrEmpty(findValue))
                {
                    list = list.Where(x => x.MemberName.Contains(findValue) || x.MBR.Contains(findValue)).ToList();
                    _logger.Info(string.Format("Pretraga po nazivu ili PIB: {0}", findValue));
                }
                if (filterByType != string.Empty)
                {
                    list = list.Where(x => x.Type.Equals(filterByType, StringComparison.OrdinalIgnoreCase)).ToList();
                    _logger.Info(string.Format("Filtriranje primarnih članova grupe: {0}", filterByType));
                }
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
                _logger.Error("Greška kod preuzimaja podataka o grupi iz Core sistema.");
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