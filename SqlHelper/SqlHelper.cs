// ============================================================================
// Autor:           Predrag Živić
// Datum kreiranja: 23.04.2008
// Naziv:           Komponenta omogućava lakši rad sa ADO.NET metodama za pristup podacima.
//
// Opis:
// Unity Script koji se poziva iz OnBase Workflow-a preko System Task-a.
// Skripta poziva eksterni REST servis radi pokretanja procesa digitalnog
// potpisivanja dokumenata.
//
// Istorija izmena:
// 15.07.2011, C# 3.0 optimizacija
// 13.04.2012, .NET 4.0
// 12.11.2022, .NET 4.8
// 12.03.2024, fixirano je ovo sa ownder nad stored procedurama i sql izratima cmd parametar
// 14.05.2024 log4net
// 29.01.2026 Poboljšano učitavanje connection string-a i owner-a iz config-a za scenario windows servis i web aplikacija
// ============================================================================


using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System;

namespace DataSystem.DAL.SqlHelper
{
    public class SqlHelper : ISqlHelper
    {
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _connectionString = "";
        private readonly string _owner = "dbo";

        #region SqlHelper old
        ////Pokupi konekcioni string baze podataka
        //public SqlHelper(string confingString)
        //{
        //    if (confingString == "exe")
        //    {
        //        // Ako je exe moraš iz .exe.config
        //        // Hardcode
        //        // var config = ConfigurationManager.OpenExeConfiguration(@"D:\Users\pzivic\Source\Repos\AlgotechUnity\AlgotechADWinService\bin\Release\AlgotechADWinService.exe");

        //        var fileName = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
        //        var configMap = new ExeConfigurationFileMap
        //        {
        //            ExeConfigFilename = fileName
        //        };
        //        var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

        //        _connectionString = config.AppSettings.Settings["connection_string"].Value;

        //        _logger.Debug(string.Format("ConfigurationFile: {0}.", fileName));
        //        _logger.Debug(string.Format("ConnectionString: {0}.", _connectionString));
        //    }
        //    else
        //    {
        //        _connectionString = confingString == "" ? ConfigurationManager.ConnectionStrings["connection_string"].ConnectionString : ConfigurationManager.ConnectionStrings[confingString].ConnectionString;
        //    }
        //    _owner = ConfigurationManager.AppSettings["dbOwner"];
        //}
        #endregion

        // Default konstruktor: koristi standardni key
        # region SqlHelper
        public SqlHelper() : this("connection_string")
        {
        }

        // Konstruktor koji prima ime connection string-a iz <connectionStrings>
        public SqlHelper(string connectionStringName)
        {
            if (string.IsNullOrWhiteSpace(connectionStringName))
                connectionStringName = "connection_string";

            _connectionString = GetConnectionStringOrThrow(connectionStringName);
            _owner = (ConfigurationManager.AppSettings["dbOwner"] ?? "dbo").Trim();

            // Ovo je OK za debug: putanja config-a, ali ne connection string.
            _logger.Debug($"SqlHelper initialized. Config file: {AppDomain.CurrentDomain.SetupInformation.ConfigurationFile}. " +
                          $"ConnectionStringName: {connectionStringName}. Owner: {_owner}.");
        }

        private static string GetConnectionStringOrThrow(string name)
        {
            var cs = ConfigurationManager.ConnectionStrings[name]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(cs))
            {
                var configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                throw new InvalidOperationException(
                    $"Nedostaje ConnectionStrings['{name}'] u konfiguraciji ({configFile}).");
            }

            return cs;
        }
        #endregion

        #region ExecuteNonQuery
        //Izvršava upit vraća Integer
        public int ExecuteNonQuery(string query)
        {
            var cnn = new SqlConnection(_connectionString);
            SqlCommand cmd;

            if (query.StartsWith("INSERT") | query.StartsWith("insert") | query.StartsWith("UPDATE") | query.StartsWith("update") | query.StartsWith("DELETE") | query.StartsWith("delete"))
            {
                cmd = new SqlCommand(query, cnn);
                cmd.CommandType = CommandType.Text;
            }
            else
            {
                cmd = new SqlCommand(_owner + "." + query, cnn);
                cmd.CommandType = CommandType.StoredProcedure;
            }
            int retval;
            try
            {
                cnn.Open();
                retval = cmd.ExecuteNonQuery();
            }
            finally
            {
                if (cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                }
            }
            return retval;

        }
        #endregion

        #region ExecuteNonQuery
        public int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            var cnn = new SqlConnection(_connectionString);
            SqlCommand cmd;

            if (query.StartsWith("INSERT") | query.StartsWith("insert") | query.StartsWith("UPDATE") | query.StartsWith("update") | query.StartsWith("DELETE") | query.StartsWith("delete"))
            {
                cmd = new SqlCommand(query, cnn);
                cmd.CommandType = CommandType.Text;
            }
            else
            {
                cmd = new SqlCommand(_owner + "." + query, cnn);
                cmd.CommandType = CommandType.StoredProcedure;
            }
            int i;
            for (i = 0; i <= parameters.Length - 1; i++)
            {
                cmd.Parameters.Add(parameters[i]);
            }
            cnn.Open();
            var retval = cmd.ExecuteNonQuery();
            cnn.Close();
            return retval;
        }
        #endregion

        #region ExecuteScalar
        //Izvršava upit vraća jednu vrednost
        public object ExecuteScalar(string query)
        {
            var cnn = new SqlConnection(_connectionString);
            SqlCommand cmd;
            if (query.StartsWith("SELECT") | query.StartsWith("select"))
            {
                cmd = new SqlCommand(query, cnn);
                cmd.CommandType = CommandType.Text;
            }
            else
            {
                cmd = new SqlCommand(_owner + "." + query, cnn);
                cmd.CommandType = CommandType.StoredProcedure;
            }
            cnn.Open();

            //Peđa 21.12.2009, ne znam da li je ovo greška ExecuteScalar a u stvari originalni kod okida ExecuteNonQuery
            //_Peđa 12.03.2024 izleda da je otrkrivena misterija kada nema parametara onda je ExecuteNonQuery
            //object retval = cmd.ExecuteNonQuery();
            var retval = cmd.ExecuteScalar();
            cnn.Close();
            return retval;
        }
        #endregion

        #region ExecuteScalar
        public object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            var cnn = new SqlConnection(_connectionString);
            SqlCommand cmd;
            if (query.StartsWith("SELECT") | query.StartsWith("select"))
            {
                cmd = new SqlCommand(query, cnn);
                cmd.CommandType = CommandType.Text;
            }
            else
            {
                cmd = new SqlCommand(_owner + "." + query, cnn);
                cmd.CommandType = CommandType.StoredProcedure;
            }
            int i;
            for (i = 0; i <= parameters.Length - 1; i++)
            {
                cmd.Parameters.Add(parameters[i]);
            }
            cnn.Open();
            var retval = cmd.ExecuteScalar();
            cnn.Close();
            return retval;
        }
        #endregion

        #region ExecuteReader
        //Izvrašava upit vraća sqldatareader objekat
        public SqlDataReader ExecuteReader(string query)
        {
            var cnn = new SqlConnection(_connectionString);
            SqlCommand cmd;

            if (query.StartsWith("SELECT") | query.StartsWith("select"))
            {
                cmd = new SqlCommand(query, cnn);
                cmd.CommandType = CommandType.Text;
            }
            else
            {
                cmd = new SqlCommand(_owner + "." + query, cnn);
                cmd.CommandType = CommandType.StoredProcedure;
            }
            cnn.Open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }
        #endregion

        #region ExecuteReader
        public SqlDataReader ExecuteReader(string query, params SqlParameter[] parameters)
        {
            var cnn = new SqlConnection(_connectionString);
            SqlCommand cmd;

            if (query.StartsWith("SELECT") | query.StartsWith("select"))
            {
                cmd = new SqlCommand(query, cnn);
                cmd.CommandType = CommandType.Text;
            }
            else
            {
                cmd = new SqlCommand(_owner + "." + query, cnn);
                cmd.CommandType = CommandType.StoredProcedure;
            }
            int i;
            for (i = 0; i <= parameters.Length - 1; i++)
            {
                cmd.Parameters.Add(parameters[i]);
            }
            cnn.Open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }
        #endregion

        #region ExecuteDataSet
        //Izvrašava upit vraća dataset
        public DataSet ExecuteDataSet(string query)
        {
            var cnn = new SqlConnection(_connectionString);
            var cmd = new SqlCommand(_owner + "." + query, cnn);
            if (query.StartsWith("SELECT") | query.StartsWith("select"))
            {
                cmd.CommandType = CommandType.Text;
            }
            else
            {
                cmd.CommandType = CommandType.StoredProcedure;
            }
            var da = new SqlDataAdapter { SelectCommand = cmd };
            var ds = new DataSet();
            da.Fill(ds);
            return ds;
        }
        #endregion

        #region ExecuteDataSet
        public DataSet ExecuteDataSet(string query, params SqlParameter[] parameters)
        {
            try
            {
                var cnn = new SqlConnection(_connectionString);
                SqlCommand cmd;

                if (query.StartsWith("SELECT") | query.StartsWith("select"))
                {
                    cmd = new SqlCommand(query, cnn);
                    cmd.CommandType = CommandType.Text;
                }
                else
                {
                    cmd = new SqlCommand(_owner + "." + query, cnn);
                    cmd.CommandType = CommandType.StoredProcedure;
                }
                int i;
                for (i = 0; i <= parameters.Length - 1; i++)
                {
                    cmd.Parameters.Add(parameters[i]);
                }
                var da = new SqlDataAdapter { SelectCommand = cmd };
                var ds = new DataSet();
                da.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion
    }
}
