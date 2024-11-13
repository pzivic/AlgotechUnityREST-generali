// Predrag Živić
// 2024-11-13

using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

/// <summary>
/// Pomoćna klasa za priširenje DataReader za Oracle
/// </summary>
public static class DataReaderExtensions
{
    /// <summary>
    /// Provera da li imamo kolonu u readru
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public static bool HasColumn(this OracleDataReader reader, string columnName)
    {
        var schemaTable = reader.GetSchemaTable();
        foreach (DataRow row in schemaTable.Rows)
        {
            if (row["ColumnName"].ToString().Equals(columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}