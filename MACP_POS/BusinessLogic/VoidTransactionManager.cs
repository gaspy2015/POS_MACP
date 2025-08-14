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
    class VoidTransactionManager : IDisposable
    {
        private readonly DBHelper _dbHelper;

        public VoidTransactionManager()
        {
            _dbHelper = new DBHelper();
        }

        /// <summary>
        /// Voids a transaction with the specified parameters
        /// </summary>
        /// <param name="request">Void transaction request object</param>
        /// <returns>VoidTransactionResponse containing the result</returns>
        public VoidTransactionResponse VoidTransaction(VoidTransactionRequest request)
        {
            var response = new VoidTransactionResponse();

            try
            {
                // Validate input
                var validationResult = ValidateRequest(request);
                if (!validationResult.IsValid)
                {
                    response.IsSucess = false;
                    response.ErrorMessage = validationResult.ErrorMessage;
                    response.ResultType = VoidResultType.ValidationError;
                    return response;
                }

                // Create parameters for stored procedure
                var parameters = CreateParameters(request);

                // Execute  stored procedure
                var result = _dbHelper.ExecuteStoredProcedure("sp_VoidTransaction", parameters);

                // Process the result
                response = ProcessStoredProcedureResult(result);
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "An unexpected error occured: " + ex.Message;
                response.ResultType = VoidResultType.SystemError;
                response.Exception = ex;
            }

            return response;
        }

        /// <summary>
        /// Checks if a transaction can be voided
        /// </summary>
        /// <param name="transactionId">Transaction ID to check</param>
        /// <returns>CanVoidResult indicating if transaction can be voided</returns>
        public CanVoidResult CanTransactionBeVoided(string transactionId)
        {
            var result = new CanVoidResult();

            try
            {
                if (string.IsNullOrWhiteSpace(transactionId))
                {
                    result.CanVoid = false;
                    result.Reason = "Transaction ID cannot be empty";
                    return result;
                }

                var parameters = new[]
                {
                    _dbHelper.CreateParameter("@TransactionID", SqlDbType.NVarChar, 20, transactionId)             
                };

                var query = @"
                    SELECT 
                        Status,
                        CASE 
                            WHEN Status = 'Completed' THEN 1
                            WHEN Status = 'Voided' THEN 0
                            ELSE 0
                        END AS CanVoid,
                        CASE 
                            WHEN Status = 'Voided' THEN 'Transaction is already voided'
                            WHEN Status != 'Completed' THEN 'Transaction must be completed to void'
                            ELSE 'Transaction can be voided'
                        END AS Reason
                    FROM SalesHeader 
                    WHERE TransactionID = @TransactionID";

                var data = _dbHelper.ExecuteQuery(query, parameters);

                if (data.Rows.Count == 0)
                {
                    result.CanVoid = false;
                    result.Reason = "Transaction not found";
                }
                else
                {
                    var row = data.Rows[0];
                    result.CanVoid = Convert.ToBoolean(row["CanVoid"]);
                    result.Reason = row["Reason"].ToString();
                    result.CurrentStatus = row["Status"].ToString();
                }
            }
            catch (Exception ex)
            {
                result.CanVoid = false;
                result.Reason = "Error checking transaction status: " + ex.Message;
                result.Exception = ex;
            }

            return result;
        }

        /// <summary>
        /// Gets available void reasons from the database
        /// </summary>
        /// <returns>List of void reasons</returns>
        public List<VoidReason> GetVoidReasons()
        {
            var voidReasons = new List<VoidReason>();

            try
            {
                var query = @"
                    SELECT 
                        ReasonID,
                        ReasonDescription,
                        ISNULL(RequiresApproval, 0) AS RequiresApproval,
                        IsActive
                    FROM ReasonCode 
                    WHERE ReasonType = 'Void' AND IsActive = 1
                    ORDER BY ReasonDescription";

                var data = _dbHelper.ExecuteQuery(query);

                foreach (DataRow row in data.Rows)
                {
                    voidReasons.Add(new VoidReason
                        {
                            ReasonID = row["ReasonID"].ToString(),
                            ReasonDescription = row["ReasonDescription"].ToString(),
                            RequiredApproval = Convert.ToBoolean(row["RequiresApproval"]),
                            IsActive = Convert.ToBoolean(row["IsActive"])
                        });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving void reasons: " + ex.Message, ex);
            }

            return voidReasons;
        }

        /// <summary>
        /// Gets transaction details for display purposes
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns>TransactionSummary object</returns>
        public TransactionSummary GetTrasnactionSummary(string transactionId)
        {
            var summary = new TransactionSummary();

            try
            {
                if (string.IsNullOrWhiteSpace(transactionId))
                {
                    throw new ArgumentException("Transaction ID cannot be empty");
                }

                var parameters = new[] { 
                    _dbHelper.CreateParameter("@TrasnactionID", SqlDbType.NVarChar, 20, transactionId)
                };

                var query = @"
                    SELECT 
                        sh.TransactionID,
                        sh.TransactionDate,
                        sh.TotalAmount,
                        sh.Status,
                        sh.CashierID,
                        sh.PrivilegeCardNumber,
                        COUNT(sd.Barcode) AS ItemCount
                    FROM SalesHeader sh
                    LEFT JOIN SalesDetail sd ON sh.TransactionID = sd.TransactionID
                    WHERE sh.TransactionID = @TransactionID
                    GROUP BY sh.TransactionID, sh.TransactionDate, sh.TotalAmount, 
                             sh.Status, sh.CashierID, sh.PrivilegeCardNumber";

                var data = _dbHelper.ExecuteQuery(query, parameters);

                if (data.Rows.Count > 0)
                {
                    var row = data.Rows[0];
                    summary.TransactionID = row["TransactionID"].ToString();
                    summary.TransactionDate = Convert.ToDateTime(row["TransactionDate"]);
                    summary.TotalAmount = Convert.ToDecimal(row["TotalAmount"]);
                    summary.Status = row["Status"].ToString();
                    summary.CashierID = row["CashierID"].ToString();
                    summary.PrivilegeCardNumber = row["PrivilegeCardNumber"].ToString();
                    summary.ItemCount = Convert.ToInt32(row["ItemCount"]);
                    summary.Found = true;
                }
                else
                {
                    summary.Found = false;
                }
            }
            catch (Exception ex)
            {
                summary.Found = false;
                summary.ErrorMessage = ex.Message;
            }

            return summary;
        }

        #region Private Methods

        private ValidationResult ValidateRequest(VoidTransactionRequest request)
        {
            var result = new ValidationResult { IsValid = true };

            if (request == null)
            {
                result.IsValid = false;
                result.ErrorMessage = "Request cannot be null";
                return result;
            }

            if (string.IsNullOrWhiteSpace(request.TransactionID))
            {
                result.IsValid = false;
                result.ErrorMessage = "Transaction ID is required";
                return result;
            }

            if (string.IsNullOrWhiteSpace(request.VoidReasonID))
            {
                result.IsValid = false;
                result.ErrorMessage = "Void reason is required";
                return result;
            }

            if (string.IsNullOrWhiteSpace(request.VoidedBy))
            {
                result.IsValid = false;
                result.ErrorMessage = "Voided by field is required";
                return result;
            }

            if (string.IsNullOrWhiteSpace(request.UserID))
            {
                result.IsValid = false;
                result.ErrorMessage = "User ID is required";
                return result;
            }

            return result;
        }

        private SqlParameter[] CreateParameters(VoidTransactionRequest request)
        {
            var parameters = new List<SqlParameter> {
                _dbHelper.CreateParameter("@TransactionID", SqlDbType.NVarChar, 20, request.TransactionID),
                _dbHelper.CreateParameter("@VoidedReasonID", SqlDbType.NVarChar, 10, request.VoidReasonID),
                _dbHelper.CreateParameter("@VoidedBy", SqlDbType.NVarChar, 10, request.VoidedBy),
                _dbHelper.CreateParameter("@UserID", SqlDbType.NVarChar, 10, request.UserID)
            };

            // Add approval code if provided
            if (!string.IsNullOrWhiteSpace(request.ApprovalCode))
            {
                parameters.Add(_dbHelper.CreateParameter("@ApprovalCode", SqlDbType.NVarChar, 20, request.ApprovalCode));
            }
            else
            {
                parameters.Add(_dbHelper.CreateParameter("@ApprovalCode", SqlDbType.NVarChar, 20, DBNull.Value));
            }

            return parameters.ToArray();
        }

        private VoidTransactionResponse ProcessStoredProcedureResult(DataTable result)
        {
            var response = new VoidTransactionResponse();

            if (result.Rows.Count == 0)
            {
                response.IsSucess = false;
                response.ErrorMessage = "No response from stored procedure";
                response.ResultType = VoidResultType.SystemError;
                return response;
            }

            var row = result.Rows[0];
            var resultType = row["Result"].ToString();

            switch (resultType.ToUpper())
            {
                case "SUCCESS":
                    response.IsSucess = true;
                    response.ResultType = VoidResultType.Success;
                    response.Message = row["Message"].ToString();
                    response.TransactionID = row["TransactionID"].ToString();
                    response.DetailRowsVoided = Convert.ToInt32(row["DetailRowsVoided"]);
                    response.PaymentRowsVoided = Convert.ToInt32(row["PaymentRowsVoided"]);
                    response.VoidedTimestamp = Convert.ToDateTime(row["VoidedTimeStamp"]);
                    break;

                case "WARNING":
                    response.IsSucess = true;
                    response.ResultType = VoidResultType.Warning;
                    response.Message = row["Message"].ToString();
                    response.ErrorMessage = row["Message"].ToString();
                    break;
                
                case "ERROR":
                    response.IsSucess = false;
                    response.ResultType = VoidResultType.BusinessError;
                    response.ErrorMessage = row["Message"].ToString();
                    response.TransactionID = row.Table.Columns.Contains("TransactionID")
                                             ? (row["TransactionID"] != null ? row["TransactionID"].ToString() : null)
                                             : null;
                    break;

                default:
                    response.IsSucess = false;
                    response.ResultType = VoidResultType.SystemError;
                    response.ErrorMessage = "Unknown response from stored procedure";
                    break;
            }

            return response;
        }

        #endregion

        #region IDisposable Implementation

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    #region Request/Response Classes

    /// <summary>
    ///  Request object for void transaction operation
    /// </summary>
    public class VoidTransactionRequest
    {
        public string TransactionID { get; set; }
        public string VoidReasonID { get; set; }
        public string VoidedBy { get; set; }
        public string ApprovalCode { get; set; }
        public string UserID { get; set; }
    }

    /// <summary>
    /// Response object for void transaction operation
    /// </summary>
    public class VoidTransactionResponse
    {
        public bool IsSucess { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public VoidResultType ResultType { get; set; }
        public string TransactionID { get; set; }
        public int DetailRowsVoided { get; set; }
        public int PaymentRowsVoided { get; set; }
        public DateTime? VoidedTimestamp { get; set; }
        public Exception Exception { get; set; }
    }

    /// <summary>
    /// Result object for checking if transaction can be voided
    /// </summary>
    public class CanVoidResult
    {
        public bool CanVoid { get; set; }
        public string Reason { get; set; }
        public string CurrentStatus { get; set; }
        public Exception Exception { get; set; }
    }

    /// <summary>
    /// Void reason information
    /// </summary>
    public class VoidReason
    {
        public string ReasonID { get; set; }
        public string ReasonDescription { get; set; }
        public bool RequiredApproval { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Transaction summary information
    /// </summary>
    public class TransactionSummary
    {
        public bool Found { get; set; }
        public string TransactionID { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string CashierID { get; set; }
        public string PrivilegeCardNumber { get; set; }
        public int ItemCount { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Validation result for input validation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Enumaration for different result types
    /// </summary>
    public enum VoidResultType
    {
        Success,
        Warning,
        ValidationError,
        BusinessError,
        SystemError
    }

    #endregion 
}
