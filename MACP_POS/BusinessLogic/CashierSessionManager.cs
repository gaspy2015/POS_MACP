using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using MACP_POS.DataAccess;

namespace MACP_POS.BusinessLogic
{
    /// <summary>
    /// Business logic class for managing cashier sessions
    /// </summary>
    public class CashierSessionManager : IDisposable
    {
        private readonly DBHelper _dbHelper;
        private bool disposed = false;

        public CashierSessionManager()
        {
            _dbHelper = new DBHelper();
        }

        /// <summary>
        /// Result model for cashier session operations
        /// </summary>
        public class CashierSessionResult
        {
            public bool IsSuccess { get; set; }
            public string SessionID { get; set; }
            public string ErrorMessage { get; set; }
            public Exception Exception { get; set; }
        }

        /// <summary>
        /// Model for cashier session start request
        /// </summary>
        public class StartSessionRequest
        {
            public string TerminalID { get; set; }
            public string CashierID { get; set; }
            public decimal StartingAmount { get; set; }
            public string UserID { get; set; }
        }

        /// <summary>
        /// Starts a new cashier session
        /// </summary>
        /// <param name="request">Session start request details</param>
        /// <returns>Result containing session ID or error information</returns>
        public CashierSessionResult StartCashierSession(StartSessionRequest request)
        {
            var result = new CashierSessionResult();

            try
            {
                // Validate input parameters
                var validationResult = ValidateStartSessionRequest(request);
                if (!validationResult.IsSuccess)
                {
                    return validationResult;
                }

                // Prepare parameters for stored procedure
                var parameters = new SqlParameter[]
                {
                    _dbHelper.CreateParameter("@TerminalID", SqlDbType.NVarChar, 10, request.TerminalID),
                    _dbHelper.CreateParameter("@CashierID", SqlDbType.NVarChar, 10, request.CashierID),
                    _dbHelper.CreateParameter("@StartingAmount", SqlDbType.Decimal, request.StartingAmount),
                    _dbHelper.CreateParameter("@UserID", SqlDbType.NVarChar, 10, request.UserID),
                    _dbHelper.CreateParameter("@SessionID", SqlDbType.NVarChar, 20, null, ParameterDirection.Output)
                };

                // Execute stored procedure
                Dictionary<string, object> outputValues;
                var dataTable = _dbHelper.ExecuteStoredProcedureWithOutput("sp_StartCashierSession", parameters, out outputValues);

                // Check if the stored procedure returned a result set
                if (dataTable.Rows.Count > 0)
                {
                    var row = dataTable.Rows[0];
                    var procedureResult = row["Result"].ToString();

                    if (procedureResult == "SUCCESS")
                    {
                        result.IsSuccess = true;
                        result.SessionID = outputValues["@SessionID"].ToString();
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = "Stored procedure did not return SUCCESS status";
                    }
                }
                else
                {
                    // If no result set, check output parameter
                    if (outputValues.ContainsKey("@SessionID") && outputValues["@SessionID"] != DBNull.Value)
                    {
                        result.IsSuccess = true;
                        result.SessionID = outputValues["@SessionID"].ToString();
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = "Failed to start chasier session - no session ID returned";
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                result.IsSuccess = false;
                result.ErrorMessage = GetFriendlyErrorMessage(sqlEx);
                result.Exception = sqlEx;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "An unexpected error occured while starting the cashier session";
                result.Exception = ex;
            }

            return result;
        }

        /// <summary>
        /// Validates the start session request parameters
        /// </summary>
        /// <param name="request">Request to validate</param>
        /// <returns>Validation result</returns>
        private CashierSessionResult ValidateStartSessionRequest(StartSessionRequest request)
        {
            var result = new CashierSessionResult { IsSuccess = true };

            if (request == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Request cannot be null";
                return result;
            }

            if (string.IsNullOrWhiteSpace(request.TerminalID))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Terminal ID is required";
                return result;
            }

            if (string.IsNullOrWhiteSpace(request.CashierID))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Cashier ID is required";
                return result;
            }

            if (string.IsNullOrWhiteSpace(request.UserID))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "User ID is required";
                return result;
            }

            if (request.StartingAmount < 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Starting amount cannot be negative";
                return result;
            }

            // Validate string lengths to match database constraints
            if (request.TerminalID.Length > 10)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Terminal ID cannot exceed 10 charcters";
                return result;
            }

            if (request.CashierID.Length > 10)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Cashier ID cannot exceed 10 characters";
                return result;
            }

            if (request.UserID.Length > 10)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "User ID cannot exceed 10 characters";
                return result;
            }

            return result;
        }

        /// <summary>
        /// Converts SQL error messages to user-friendly messages
        /// </summary>
        /// <param name="sqlEx">SQL exception</param>
        /// <returns>User-friendly error message</returns>
        private string GetFriendlyErrorMessage(SqlException sqlEx)
        {
            // Handle specific error messages from the stored procedure
            if (sqlEx.Message.Contains("Terminal") && sqlEx.Message.Contains("not active"))
            {
                return "The selected teminal is not active or does not exist. Please select different terminal.";
            }

            if (sqlEx.Message.Contains("Cashier") && sqlEx.Message.Contains("already has an open session"))
            {
                return "This cashier already has an open session on another terminal. Please close the existing session first.";
            }

            // Handle database connection errors
            if (sqlEx.Number == 2) // Timeout
            {
                return "Database connection timeout. Please try again.";
            }

            if (sqlEx.Number == 18456) // Login failed
            {
                return "Database authentication failed. Please contact system administrator.";
            }

            // Default message for other SQL errors
            return string.Format("Database error occured: {0}", sqlEx.Message);
        }

        /// <summary>
        /// Tests the database connection
        /// </summary>
        /// <returns>True if connection is successful</returns>
        public bool TestDatabaseConnection()
        {
            try
            {
                return _dbHelper.TestConnection();
            }
            catch 
            {
                return false;
            }
        }

        /// <summary>
        /// Gets database server information
        /// </summary>
        /// <returns>Server information string</returns>
        public string GetDatabaseInfo()
        {
            try
            {
                return _dbHelper.GetServerInfo();
            }
            catch (Exception ex)
            {
                return "Error retrieving database info: " + ex.Message;
            }
        }

        /// <summary>
        /// Checks if a terminal is active and available
        /// </summary>
        /// <param name="terminalID">Terminal ID to check</param>
        /// <returns>True if terminal is active</returns>
        public bool IsTerminalActive(string terminalID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(terminalID))
                    return false;

                var parameters = new SqlParameter[]
                {
                    _dbHelper.CreateParameter("@TerminalID", SqlDbType.NVarChar, 10, terminalID)
                };

                var query = "SELECT COUNT(1) FROM POSTerminal WHERE TerminalID = @TerminalID and IsActive = 1";
                var result = _dbHelper.ExecuteScalar(query, parameters);

                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a cashier has an open session
        /// </summary>
        /// <param name="cashierID">Cashier ID to check</param>
        /// <returns>True if cashier has an open session</returns>
        public bool HasOpenSession(string cashierID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cashierID))
                    return false;

                var parameters = new SqlParameter[]
                {
                    _dbHelper.CreateParameter("@CashierID", SqlDbType.NVarChar, 10, cashierID)
                };

                var query = "SELECT COUNT(1) FROM CashierSession WHERE CashierID = @CashierID and Status = 'Open'";
                var result = _dbHelper.ExecuteScalar(query, parameters);

                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets active terminals list
        /// </summary>
        /// <returns>DataTable containing active terminals</returns>
        public DataTable GetActiveTerminals()
        {
            try
            {
                var query = "SELECT TerminalID, TerminalName, Location FROM POSTerminal WHERE IsActive = 1 ORDER BY TerminalName";
                return _dbHelper.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving active terminals: " + ex.Message, ex);
            }
        }

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (_dbHelper != null)
                    {
                        _dbHelper.Dispose();
                    }
                }
                disposed = true;
            }
        }

        ~CashierSessionManager()
        {
            Dispose(false);
        }

        #endregion
    }
}
