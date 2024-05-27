// Predrag Živić
// Test kontroler

using System;
using System.Web.Http;
using System.Net.Http;
using System.Net;

using AlgotechUnityREST_ckb.Models;

using System.Net.Http.Headers;
using System.Collections.Specialized;

using System.Threading.Tasks;

namespace AlgotechUnityREST_ckb.Controllers
{
    /// <summary>
    /// Testi kontroler
    /// </summary>
    public class TestDocumentsController : ApiController
    {
        #region Variable  
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static HttpResponseMessage response;
        #endregion

        #region Result
        private class Result
        {
            public string Status { get; set; }
            public string Message { get; set; }
        }
        #endregion

        #region Post
        /// <summary>
        /// Preuyimanje dokumenata
        /// </summary>
        /// <param name="documentTest"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Post")]
        public async Task<HttpResponseMessage> Post(TestDocuments documentTest)
        {
            Result result;

            var sessionID = Guid.NewGuid().ToString();
            var dateTimeStart = DateTime.Now;
            var message = string.Empty;
            var status = string.Empty;

            try
            {
                // Asinhrono
                await Task.Run(() =>
                {
                    _logger.Info("Metoda:: Test Asinhrona metoda");

                    // Validacija
                    if (documentTest.DocumentName == string.Empty) { throw new ArgumentException("Niste definisali naziv dokumenta!"); }

                    // Ide neka logika
                    status = "1001";
                    //.

                    // OK
                    message = string.Format("Dokument je uspešno sačuvan {0}.", status);
                    result = new Result
                    {
                        Status = status,
                        Message = message
                    };
                    response = Request.CreateResponse(HttpStatusCode.OK, result);
                    //.

                    // Kolačić
                    var nalueCollection = new NameValueCollection
                    {
                        ["SessionID"] = sessionID,
                    };
                    var cookie = new CookieHeaderValue("Session", nalueCollection)
                    {
                        Expires = DateTimeOffset.Now.AddDays(1),
                        Domain = Request.RequestUri.Host,
                        Path = "/"
                    };
                    response.Headers.AddCookies(new CookieHeaderValue[] { cookie });
                    //.

                });
            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error("Greška kod Post Test metode");
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                result = new Result
                {
                    Status = string.Empty,
                    Message = ex.Message
                };

                response = Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            finally
            {
                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
                _logger.Debug(string.Format("Metoda:: Test Asinhrona metoda: session [{0}] finally {1} in sec {2}.", sessionID, dateTimeEnd, diffInSeconds));

            }
            return response;
        }
        #endregion
    }
}
