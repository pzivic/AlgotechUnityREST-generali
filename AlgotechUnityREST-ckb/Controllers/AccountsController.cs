// Predrag Živić
// 08.03.2024

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Web.Http;

using Hyland.Unity;
using AlgotechUnityREST_ckb.Models;
using Newtonsoft.Json;
using System.Linq;
using System.Web.Http.Description;

namespace AlgotechUnityREST_ckb.Controllers
{
    /// <summary>
    /// Korisnički nalozi
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountsController : ApiController
    {
        #region Variable  
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        HttpResponseMessage response;
        #endregion

        #region AccountList
        /// <summary>
        /// Vraća listu korisničkih naloga
        /// </summary>
        [HttpGet, ActionName("AccountList")]
        public HttpResponseMessage AccountList(string userGroupName="")
        {
            Application application = null;
            Result result;
            var dateTimeStart = DateTime.Now;

            try
            {
                // Konektuj OnBase
                _logger.Debug("OnBase Connect()");
                var obj = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                application = obj.OnBaseConnect();
                if (application == null)
                {
                    _logger.Error("Greška kod uspostavljanja konekcije sa OnBase");
                    throw new ArgumentException("Greška kod uspostavljanja konekcije sa OnBase. Kontaktirajte OnBase administratora.");
                }
                //.

                // Logijemo od momenta uspostavljanja konekcije
                dateTimeStart = DateTime.Now;
                _logger.Info("Metoda:: GetUsers start " + dateTimeStart);

                // Daj nam korisnike
                UserList userList;
                UserAdministration userAdministration = application.Core.UserAdministration;
                UserGroupList userGroupList = userAdministration.GetUserGroups();
                if (userGroupName == string.Empty)
                {
                    userList = userAdministration.GetUsers();
                }
                else 
                {
                    UserGroup userGroup = userAdministration.GetUserGroups().Find(userGroupName);
                    userList = userAdministration.GetUsers(userGroup);
                }
                //.

                var list = new List<Accounts>();
                foreach (var item in userList)
                {
                    // Podaci Dokumenta
                    var objAccount = new Accounts()
                    {
                        UserID = item.ID,
                        UserName = item.Name,
                        DisplayName = item.DisplayName,
                        EmailAddress = item.EmailAddress,
                    };
                    //.
                    list.Add(objAccount);
                }
                //

                // Serijalizacija
                var json = JsonConvert.SerializeObject(list.OrderBy(n=>n.UserID), Formatting.None);
                //.

                // Dodaj root
                //json = "{\"documents\":" + json + "}";
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                //.
            }

            catch (Exception ex)
            {
                // Logger
                _logger.Error("Greška preuzimaja podataka");
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
            finally
            {
                if (application != null)
                {
                    _logger.Info("OnBase Disconnect()");
                    var obj = new Algotech.UTL.AlgotechUnityUDF.OnBaseConnection();
                    obj.OnBaseDisconnect(application);
                }

                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
                _logger.Info("Metoda:: GetUsers finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());
            }
            return response;
        }
        #endregion
    }
}