// Predrag Živić
// 12.11.2022

using System.Data;
using System.Data.SqlClient;

namespace DataSystem.DAL.SqlHelper
{
    internal interface ISqlHelper
    {
        int ExecuteNonQuery(string query, params SqlParameter[] parameters);
        object ExecuteScalar(string query);
        object ExecuteScalar(string query, params SqlParameter[] parameters);
        SqlDataReader ExecuteReader(string query);
        SqlDataReader ExecuteReader(string query, params SqlParameter[] parameters);
        DataSet ExecuteDataSet(string query);
        DataSet ExecuteDataSet(string query, params SqlParameter[] parameters);
    }
}
