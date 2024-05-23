//Autor... Predrag Živić
//Datum... 14.10.2022
//Opis... Komponenta omogućava lakši rad sa ADO.NET metodama za pristup podacima.

/*
Oracle.ManagedDataAccess by Oracle

ODP.NET, Managed Driver is a 100% native code .NET Framework driver for Oracle Database. 
No additional Oracle Client software is required to be installed to connect to Oracle Database.
.NET Framework 48
*/

using System;
using System.Data;
using System.Configuration;

using Oracle.ManagedDataAccess.Client;

namespace Algotech.DAL.OracleHelper
{
    public class OracleHelper
    {
        #region Varijable
        private readonly string _connectionString = "";
        private readonly int _command_timeout = 120;

        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Konstruktor
        //Pokupi konekcioni string baze podataka
        public OracleHelper(string confingString)
        {
            _connectionString = confingString == "" ? 
                ConfigurationManager.ConnectionStrings["connection_string"].ConnectionString : 
                ConfigurationManager.ConnectionStrings[confingString].ConnectionString;
        }
        #endregion

        #region ExecuteNonQuery
        //Izvršava upit vraća Integer
        public int ExecuteNonQuery(string query)
        {
            var cnn = new OracleConnection(_connectionString);
            var cmd = new OracleCommand(query, cnn)
            {
                CommandType = CommandType.Text,
                CommandTimeout = _command_timeout
            };

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
        public int ExecuteNonQuery(string query, params OracleParameter[] parameters)
        {
            var cnn = new OracleConnection(_connectionString);
            var cmd = new OracleCommand(query, cnn)
            {
                CommandType = CommandType.Text,
                CommandTimeout = _command_timeout
            };

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
            var cnn = new OracleConnection(_connectionString);
            var cmd = new OracleCommand(query, cnn)
            {
                CommandType = CommandType.Text,
                CommandTimeout = _command_timeout
            };
            cnn.Open();
            var retval = cmd.ExecuteScalar();
            cnn.Close();

            return retval;
        }
        #endregion

        #region ExecuteScalar
        public object ExecuteScalar(string query, params OracleParameter[] parameters)
        {
            var cnn = new OracleConnection(_connectionString);
            var cmd = new OracleCommand(query, cnn)
            {
                CommandType = CommandType.Text,
                CommandTimeout = _command_timeout
            };

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
        /// <summary>
        /// Izvršava upit za čitanje
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public OracleDataReader ExecuteReader(string query)
        {
            var dateTimeStart = DateTime.Now;
            _logger.Info("Funkcija:: ExecuteReader Start: " + dateTimeStart);

            _logger.Info(query);

            var cnn = new OracleConnection(_connectionString);
            var cmd = new OracleCommand(query, cnn)
            {
                CommandType = CommandType.Text,
                CommandTimeout = _command_timeout
            };

            _logger.Info("Otvaram Oracle konekciju");
            cnn.Open();

            //
            var dateTimeEnd = DateTime.Now;
            var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
            _logger.Info("Izvršavam uput i zatvaram konekciju ");
            _logger.Info("Funkcija:: ExecuteReader End: " + dateTimeEnd + " sec " + diffInSeconds.ToString());
            //

            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            

            //// This code closes its connection immediately (and performs whatever other work is required to dispose it), without specifying the command behavio
            //using (OracleDataReader reader = cmd.ExecuteReader())
            //return reader;
        }
        #endregion

        #region ExecuteReader
        /// <summary>
        /// Izvršava upit za šitanje sa oracle parametrima
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public OracleDataReader ExecuteReader(string query, params OracleParameter[] parameters)
        {
            var cnn = new OracleConnection(_connectionString);
            OracleCommand cmd;

            //var cmd = new OracleCommand(query, cnn)
            //{
            //    CommandType = CommandType.Text,
            //    CommandTimeout = _command_timeout
            //};

            if (query.StartsWith("SELECT") | query.StartsWith("select"))
            {
                OracleCommand oracleCommand = new OracleCommand(query, cnn);
                cmd = oracleCommand;
                cmd.CommandType = CommandType.Text;
            }
            else
            {
                OracleCommand oracleCommand = new OracleCommand(query, cnn);
                cmd = oracleCommand;
                cmd.CommandType = CommandType.StoredProcedure;
            }

            cmd.BindByName = true;

            int i;
            for (i = 0; i <= parameters.Length-1; i++)
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
            var cnn = new OracleConnection(_connectionString);
            var cmd = new OracleCommand(query, cnn)
            {
                CommandType = CommandType.Text,
                CommandTimeout = _command_timeout
            };

            var da = new OracleDataAdapter { SelectCommand = cmd };
            var ds = new DataSet();

            da.Fill(ds);

            return ds;
        }
        #endregion

        #region ExecuteDataSet
        public DataSet ExecuteDataSet(string query, params OracleParameter[] parameters)
        {
            var cnn = new OracleConnection(_connectionString);
            var cmd = new OracleCommand(query, cnn)
            {
                CommandType = CommandType.Text,
                CommandTimeout = _command_timeout
            };
            int i;
            for (i = 0; i <= parameters.Length - 1; i++)
            {
                cmd.Parameters.Add(parameters[i]);
            }
            var da = new OracleDataAdapter { SelectCommand = cmd };
            var ds = new DataSet();

            da.Fill(ds);

            return ds;
        }
        #endregion
    }
}
