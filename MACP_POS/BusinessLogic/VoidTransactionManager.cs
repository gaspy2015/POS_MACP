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
