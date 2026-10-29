// Predrag Živić
// 10.04.2023
// Application pool ya OnBase konekcije

using System;
using System.Collections.Generic;
using System.IO;

using DataSystem.DAL.SqlHelper;

namespace Algotech.UTL.AlgotechUnityUDF
{
    public class AppPool
    {
        #region Enumerator
        public enum Status : short
        {
            InActive = 0,
            Active = 1
        }
        #endregion

        #region Variable  
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _appServerURL = System.Configuration.ConfigurationManager.AppSettings["AppServerURL"];
        private readonly string _dataSource = System.Configuration.ConfigurationManager.AppSettings["DataSource"];
        #endregion

        #region Svojstva
        public short AppPoolID { get; set; }
        public string AppPoolIdentity { get; set; }
        public string AppPoolPassword { get; set; }
        public string AppPoolPasswordSalt { get; set; }
        #endregion

        #region GetAppPools
        /// <summary>
        /// /// Lista vrednosi za app.pools preko baze podataka
        /// </summary>
        /// <param name="appPoolStatus"> Status: InActive, Active</param>
        /// <returns></returns>
        public List<AppPool> GetAppPools(short appPoolStatus)
        {
            var sqlHelper = new SqlHelper("connection_string_mssql");
            var sqlSQL = String.Format("SELECT AppPoolID, AppPoolIdentity, AppPoolPassword, AppPoolPasswordSalt FROM AppPool WHERE AppPoolStatus={0} ORDER BY AppPoolID;", appPoolStatus);
            var reader = sqlHelper.ExecuteReader(sqlSQL);
            var list = new List<AppPool>();

            while (reader.Read())
            {
                var obj = new AppPool
                {
                    AppPoolID = Convert.ToInt16(reader["AppPoolID"]),
                    AppPoolIdentity = Convert.ToString(reader["AppPoolIdentity"]),
                    AppPoolPassword = Convert.ToString(reader["AppPoolPassword"]),
                    AppPoolPasswordSalt = Convert.ToString(reader["AppPoolPasswordSalt"])
                };

                list.Add(obj);
            }
            reader.Close();

            return list;
        }
        #endregion

        #region GetAppPoolsByText
        /// <summary>
        /// Lista vrednosi za app.pools preko txt fajla podataka
        /// </summary>
        /// <param name="appPoolStatus"></param>
        /// <returns></returns>
        public List<AppPool> GetAppPoolsByText(short appPoolStatus)
        {
            // Putanja do fajla koji želite da otvorite
            string filePath = @"D:\Users\pzivic\Source\Repos\AlgotechUnityREST-generali\AlgotechUnityREST-generali\app_pool.txt";

            var list = new List<AppPool>();

            // Provera da li fajl postoji
            if (File.Exists(filePath))
            {
                // Otvaranje fajla i čitanje redova
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Dodavanje reda u listu
                        //lines.Add(line);

                        var obj = new AppPool
                        {
                            //AppPoolID = Convert.ToInt16(reader["AppPoolID"]),
                            AppPoolIdentity = line,
                            //AppPoolPassword = Convert.ToString(reader["AppPoolPassword"]),
                            //AppPoolPasswordSalt = Convert.ToString(reader["AppPoolPasswordSalt"])
                        };

                        list.Add(obj);

                    }
                }

                return list;

            }
            else
            {
                Console.WriteLine("Fajl ne postoji na datoj putanji.");
            }

            return list;
        }
        #endregion

        //#region SetAppPools
        ///// <summary>
        ///// /// Lista vrednosi za app.pools preko baze podataka
        ///// </summary>
        ///// <param name="appPoolID"> Identifikator app.poola</param>
        ///// <returns></returns>
        //public bool SetAppPools(int appPoolID)
        //{
        //    var sqlHelper = new SqlHelper("connection_string_mssql");
        //    var sqlSQL = string.Format("UPDATE AppPool SET ConnectionDate={0} WHERE AppPoolID={1};", DateTime.Now, appPoolID);
        //    sqlHelper.ExecuteScalar(sqlSQL);
        //    return true;
        //}
        //#endregion
    }
}
