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
    public class DBHelper
    {
        private readonly string connectionString;

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
                using (var connection = GetConnection())
                {
                    connection.Open();
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
                using (var connection = GetConnection())
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        connection.Open();
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
                using (var connection = GetConnection())
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        connection.Open();
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
                using (var connection = GetConnection())
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        connection.Open();
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

        // Execute a stored procedure
        public DataTable ExecuteStoredProcedure(string procedureName, SqlParameter[] parameters = null)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new SqlCommand(procedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        connection.Open();
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
    }
}
