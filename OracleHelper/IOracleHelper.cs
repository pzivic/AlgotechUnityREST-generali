using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace Algotech.DAL.OracleHelper
{
    internal interface IOracleHelper
    {
        int ExecuteNonQuery(string query);
        int ExecuteNonQuery(string query, params OracleParameter[] parameters);
        object ExecuteScalar(string query);
        object ExecuteScalar(string query, params OracleParameter[] parameters);
        OracleDataReader ExecuteReader(string query);
        OracleDataReader ExecuteReader(string query, params OracleParameter[] parameters);
        DataSet ExecuteDataSet(string query);
        DataSet ExecuteDataSet(string query, params OracleParameter[] parameters);
    }
}
