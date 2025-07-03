using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace MACP_POS.DataAccess
{
    public class DBHelper : IDisposable
    {
        private readonly string connectionString;
        private SqlConnection connection;
        private SqlTransaction transaction;

        public DBHelper()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        // Open and return a SqlConnection
        private SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        // Test if connection is valid
        public bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // Get current connection string (useful for debugging)
        public string GetConnectionString()
        {
            return connectionString;
        }

        // Execute a query and return a DataTable
        public DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var conn = GetConnection())
                {
                    using (var command = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        conn.Open();
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing query: " + ex.Message, ex);
            }

            return dataTable;
        }

        // Execute a non-query command (INSERT, UPDATE, DELETE)
        public int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            int rowsAffected = 0;

            try
            {
                using (var conn = GetConnection())
                {
                    using (var command = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        conn.Open();
                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing non-query: " + ex.Message, ex);
            }

            return rowsAffected;
        }

        // Execute a scalar command (returns single value)
        public object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            object result = null;

            try
            {
                using (var conn = GetConnection())
                {
                    using (var command = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        conn.Open();
                        result = command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing scalar: " + ex.Message, ex);
            }

            return result;
        }

        // Execute a stored procedure and return DataTable
        public DataTable ExecuteStoredProcedure(string procedureName, SqlParameter[] parameters = null)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var conn = GetConnection())
                {
                    using (var command = new SqlCommand(procedureName, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        conn.Open();
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing stored procedure: " + ex.Message, ex);
            }

            return dataTable;
        }

        // Execute stored procedure for non-query operations (INSERT, UPDATE, DELETE)
        public int ExecuteStoredProcedureNonQuery(string procedureName, SqlParameter[] parameters = null)
        {
            int rowsAffected = 0;

            try
            {
                using (var conn = GetConnection())
                {
                    using (var command = new SqlCommand(procedureName, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        conn.Open();
                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing stored procedure: " + ex.Message, ex);
            }

            return rowsAffected;
        }

        // Execute stored procedure and return scalar value
        public object ExecuteStoredProcedureScalar(string procedureName, SqlParameter[] parameters = null)
        {
            object result = null;

            try
            {
                using (var conn = GetConnection())
                {
                    using (var command = new SqlCommand(procedureName, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        conn.Open();
                        result = command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing stored procedure scalar: " + ex.Message, ex);
            }

            return result;
        }

        // Execute stored procedure with output parameters
        public DataTable ExecuteStoredProcedureWithOutput(string procedureName, SqlParameter[] parameters, out Dictionary<string, object> outputValues)
        {
            DataTable dataTable = new DataTable();
            outputValues = new Dictionary<string, object>();

            try
            {
                using (var conn = GetConnection())
                {
                    using (var command = new SqlCommand(procedureName, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        conn.Open();
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }

                        // Get output parameter values
                        foreach (SqlParameter param in command.Parameters)
                        {
                            if (param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput)
                            {
                                outputValues[param.ParameterName] = param.Value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing stored procedure with output: " + ex.Message, ex);
            }

            return dataTable;
        }

        // Execute multiple stored procedures in a transaction
        public bool ExecuteStoredProceduresInTransaction(List<StoredProcedureCall> procedures)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (var proc in procedures)
                            {
                                using (var command = new SqlCommand(proc.ProcedureName, conn, transaction))
                                {
                                    command.CommandType = CommandType.StoredProcedure;

                                    if (proc.Parameters != null)
                                    {
                                        command.Parameters.AddRange(proc.Parameters);
                                    }

                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing stored procedures in transaction: " + ex.Message, ex);
            }
        }

        // Helper method to create SqlParameter
        public SqlParameter CreateParameter(string parameterName, SqlDbType dbType, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            return new SqlParameter
            {
                ParameterName = parameterName,
                SqlDbType = dbType,
                Value = value ?? DBNull.Value,
                Direction = direction
            };
        }

        // Helper method to create SqlParameter with size
        public SqlParameter CreateParameter(string parameterName, SqlDbType dbType, int size, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            return new SqlParameter
            {
                ParameterName = parameterName,
                SqlDbType = dbType,
                Size = size,
                Value = value ?? DBNull.Value,
                Direction = direction
            };
        }

        // Get database server information
        public string GetServerInfo()
        {
            try
            {
                string query = "SELECT @@SERVERNAME as ServerName, @@VERSION as Version, DB_NAME() as DatabaseName";
                var result = ExecuteQuery(query);

                if (result.Rows.Count > 0)
                {
                    var row = result.Rows[0];
                    return string.Format("Server: {0}, Database: {1}",
                        row["ServerName"], row["DatabaseName"]);
                }
            }
            catch (Exception ex)
            {
                return "Error getting server info: " + ex.Message;
            }

            return "Unknown";
        }

        // Dispose pattern
        public void Dispose()
        {
            if (transaction != null)
            {
                transaction.Dispose();
            }
            if (connection != null)
            {
                connection.Dispose();
            }
        }
    }

    // Helper class for transaction operations
    public class StoredProcedureCall
    {
        public string ProcedureName { get; set; }
        public SqlParameter[] Parameters { get; set; }
    }
}