

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using DataSystem.DAL.SqlHelper;

using AlgotechUnityREST_ckb.Models;

namespace AlgotechUnityREST_ckb.Security
{
    /// <summary>
    /// Autehtifikacija i autorizacija za OAuth 2.0 client credentials flow.
    /// </summary>
    public class OAuth
    {
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
            var sqlHelper = new SqlHelper("connection_string_mssql");
            var sqlSQL = string.Format(@"SELECT OAuthClientID, ClientID, ClientSecretHash, IsActive, Description, AllowedIP FROM dbo.OAuthClient WHERE ClientID='{0}' AND IsActive = 1;", clientID);
            var reader = sqlHelper.ExecuteReader(sqlSQL);

            var obj = new OAuthClient();

            if (reader.Read())
            {
                obj.OAuthClientID = Convert.ToInt32(reader["OAuthClientID"]);
                obj.ClientID = Convert.ToString(reader["ClientID"]);
                obj.ClientSecretHash = Convert.ToString(reader["ClientSecretHash"]);
                obj.IsActive = Convert.ToBoolean(reader["IsActive"]);
                obj.Description = reader["Description"] == DBNull.Value ? null : Convert.ToString(reader["Description"]);
                obj.AllowedIP = reader["AllowedIP"] == DBNull.Value ? null : Convert.ToString(reader["AllowedIP"]);
            }
            reader.Close();
            return obj;

        }
        #endregion
    }
}