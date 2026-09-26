// ============================================================================
// Autor:           Predrag Živić
// Datum kreiranja: 23.09.2026
//
// Opis:
// Autehtifikacija i autorizacija za OAuth 2.0 client credentials flow.
//
// Istorija izmena:
// 25.06.2026 - Predrag Živić - SQL Helper implementacija za preuzimanje OAuthClient-a iz baze podataka.
// ============================================================================

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using DataSystem.DAL.SqlHelper;
using AlgotechUnityREST_ckb.Models;

namespace AlgotechUnityREST.Security
{
    /// <summary>
    /// Autehtifikacija i autorizacija za OAuth 2.0 client credentials flow.
    /// </summary>
    public class OAuth
    {
        #region Variable  
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region OAuthClient
        /// <summary>
        /// Preuzimanje OAuthClient iz baze podataka na osnovu ClientID-a.
        /// </summary>
        /// <param name="clientID"></param>
        /// <returns></returns>
        public static OAuthClient GetOAuthClient_(string clientID)
        {
            string ConnectionString = ConfigurationManager.ConnectionStrings["OAuthConnection"].ConnectionString;

            const string Sql = @"
        SELECT
            OAuthClientID,
            ClientID,
            ClientSecretHash,
            IsActive,
            Description,
            AllowedIP
        FROM dbo.OAuthClient
        WHERE ClientID = @ClientID
          AND IsActive = 1";

            using (var Connection = new SqlConnection(ConnectionString))
            using (var Command = new SqlCommand(Sql, Connection))
            {
                Command.Parameters.Add("@ClientID", SqlDbType.NVarChar, 100).Value = clientID;
                Connection.Open();

                using (var Reader = Command.ExecuteReader())
                {
                    if (!Reader.Read())
                        return null;

                    return new OAuthClient
                    {
                        OAuthClientID = Convert.ToInt32(Reader["OAuthClientID"]),
                        ClientID = Convert.ToString(Reader["ClientID"]),
                        ClientSecretHash = Convert.ToString(Reader["ClientSecretHash"]),
                        IsActive = Convert.ToBoolean(Reader["IsActive"]),
                        Description = Reader["Description"] == DBNull.Value ? null : Convert.ToString(Reader["Description"]),
                        AllowedIP = Reader["AllowedIP"] == DBNull.Value ? null : Convert.ToString(Reader["AllowedIP"])
                    };
                }
            }
        }
        #endregion

        #region GetOAuthClient

        /// <summary>
        /// Preuzimanje OAuthClient iz baze podataka na osnovu ClientID-a.
        /// </summary>
        /// <param name="clientID"></param>
        /// <returns></returns>
        public static OAuthClient GetOAuthClient(string clientID)
        {
            var dateTimeStart = DateTime.Now;
            SqlDataReader reader = null;

            try
            {
                _logger.Info("Metoda:: GetOAuthClient start " + dateTimeStart);
                _logger.Info("Preuzimanje OAuthClient iz baze podataka na osnovu ClientID-a.");

                var sqlHelper = new SqlHelper("connection_string_mssql");
                // Možda pomeriti u stored procedure
                var sqlSQL = string.Format(@"SELECT OAuthClientID, ClientID, ClientSecretHash, IsActive, Description, AllowedIP FROM dbo.OAuthClient WHERE ClientID = '{0}' AND IsActive = 1;", clientID);

                reader = sqlHelper.ExecuteReader(sqlSQL);

                var obj = new OAuthClient();
                if (reader.Read())
                {
                    obj.OAuthClientID = Convert.ToInt32(reader["OAuthClientID"]);
                    obj.ClientID = Convert.ToString(reader["ClientID"]);
                    obj.ClientSecretHash = Convert.ToString(reader["ClientSecretHash"]);
                    obj.IsActive = Convert.ToBoolean(reader["IsActive"]);
                    obj.Description = reader["Description"] == DBNull.Value ? null : Convert.ToString(reader["Description"]);
                    obj.AllowedIP = reader["AllowedIP"] == DBNull.Value ? null : Convert.ToString(reader["AllowedIP"]);

                    _logger.Info(string.Format("OAuthClient pronađen. ClientID: {0}, OAuthClientID: {1}.", obj.ClientID, obj.OAuthClientID));
                }
                else
                {
                    _logger.Warn(string.Format("OAuthClient nije pronađen ili nije aktivan. ClientID: {0}.", clientID));
                }

                return obj;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Greška kod preuzimanja OAuthClient-a. ClientID: {0}.", clientID));
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);

                throw;
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }

                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;

                _logger.Info("Metoda:: GetOAuthClient finally " + dateTimeEnd + " sec " + diffInSeconds);
            }
        }
        #endregion
    }
}