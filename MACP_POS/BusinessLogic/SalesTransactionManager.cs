using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;
using MACP_POS.DataAccess;

namespace MACP_POS.BusinessLogic
{
    public class SalesTransactionManager : IDisposable
    {
        private readonly DBHelper _dbHelper;

        public SalesTransactionManager()
        {
            _dbHelper = new DBHelper();
        }

        public SaleTransactionResult ProcessSaleTransaction(SaleTransactionRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request", "Sale transaction request cannot be null");

            ValidateRequest(request);

            try
            {
                // Create XML for sales items
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

        /// <summary>
        /// Validates the sale transaction request
        /// </summary>
        /// <param name="request"></param>
        private void ValidateRequest(SaleTransactionRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(request.SessionID))
                errors.Add("Session ID id required");

            if (string.IsNullOrEmpty(request.UserID))
                errors.Add("User ID is required");

            if (request.SalesItems == null || request.SalesItems.Count == 0)
                errors.Add("At least one sales item is required");

            if (request.PaymentDetails == null || request.PaymentDetails.Count == 0)
                errors.Add("At least one payment method is required");

            // Validate sales items
            if (request.SalesItems != null)
            {
                for (int i = 0; i < request.SalesItems.Count; i++)
                {
                    var item = request.SalesItems[i];
                    if (string.IsNullOrEmpty(item.ProductSKU) && string.IsNullOrEmpty(item.Barcode))
                        errors.Add(string.Format("Sales item {0}: Quantity must be greater than 0", i + 1));

                    if (item.Quantity <= 0)
                        errors.Add(string.Format("Sales item {0}: Quantity must be greater than 0", i + 1));

                    if (item.UnitPrice < 0)
                        errors.Add(string.Format("Sales item {0}: Unit price cannot be negative", i + 1));
                }
            }

            // Validate payment details
            if (request.PaymentDetails != null)
            {
                for (int i = 0; i < request.PaymentDetails.Count; i++)
                {
                    var payment = request.PaymentDetails[i];
                    if (string.IsNullOrEmpty(payment.PaymentMethodTypeID))
                        errors.Add(string.Format("Payment {0}: Payment method type ID is required", i + 1));

                    if (payment.Amount <= 0)
                        errors.Add(string.Format("Payment {0} Amount must be greater than 0", i + 1));
                }
            }

            if (errors.Count > 0)
            {
                throw new ArgumentException("Validation errors: " + string.Join("; ", errors));
            }
        }

        /// <summary>
        /// Create XML string for sales items
        /// </summary>
        /// <param name="salesItems"></param>
        /// <returns></returns>
        private string CreateSalesItemsXml(List<SalesItem> salesItems)
        {
            var itemsElement = new XElement("Items");

            foreach (var item in salesItems)
            {
                var itemElement = new XElement(
                    "Item",
                    new XAttribute("ProductSKU", item.ProductSKU ?? string.Empty),
                    new XAttribute("Barcode", item.Barcode ?? string.Empty),
                    new XAttribute("Quantity", item.Quantity),
                    new XAttribute("UnitPrice", item.UnitPrice),
                    new XAttribute("DiscountReasonID", item.DiscountReasonID ?? string.Empty)
                    );

                itemsElement.Add(itemElement);
            }

            return itemsElement.ToString();
        }

        /// <summary>
        /// Create XML string for payment details
        /// </summary>
        /// <param name="paymentDetails"></param>
        /// <returns></returns>
        private string CreatePaymentDetailsXml(List<PaymentDetail> paymentDetails)
        {
            var paymentsElement = new XElement("Payments");

            foreach (var payment in paymentDetails)
            {
                var paymentElement = new XElement(
                    "Payment",
                    new XAttribute("PaymentMethodTypeID", payment.PaymentMethodTypeID),
                    new XAttribute("Aount", payment.Amount),
                    new XAttribute("ReferenceNumber", payment.ReferenceNumber ?? string.Empty),
                    new XAttribute("CardTypeID", payment.CardTypeID ?? string.Empty),
                    new XAttribute("BankID", payment.BankID ?? string.Empty)
                    );

                paymentsElement.Add(paymentElement);
            }

            return paymentsElement.ToString();
        }

        /// <summary>
        /// Tests the database connection
        /// </summary>
        /// <returns></returns>
        public bool TestConnection()
        {
            return _dbHelper.TestConnection();
        }

        /// <summary>
        /// Gets database server information
        /// </summary>
        /// <returns></returns>
        public string GetServerInfo()
        {
            return _dbHelper.GetServerInfo();
        }

        public void Dispose()
        {
            if (_dbHelper != null)
            {
                _dbHelper.Dispose();
            }
        }
    }

    public class SaleTransactionRequest
    {
        public string SessionID{ get; set; }
        public string PrivilegeCardNumber { get; set; }
        public List<SalesItem> SalesItems {get; set; }
        public List<PaymentDetail> PaymentDetails { get; set; }
        public string UserID { get; set; }

        public SaleTransactionRequest()
        {
            SalesItems = new List<SalesItem>();
            PaymentDetails = new List<PaymentDetail>();
        }
    }

    /// <summary>
    /// Represents a sales item in the transaction
    /// </summary>
    public class SalesItem
    {
        public string ProductSKU { get; set; }
        public string Barcode { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string DiscountReasonID { get; set; }

        public SalesItem()
        {
            Quantity = 1;
            UnitPrice = 0;
        }
    }

    /// <summary>
    /// Represents a payment detail in the transaction
    /// </summary>
    public class PaymentDetail
    {
        public string PaymentMethodTypeID { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceNumber { get; set; }
        public string CardTypeID { get; set; }
        public string BankID { get; set; }

        public PaymentDetail()
        {
            Amount = 0;
        }
    }

    /// <summary>
    /// Result object for sale transaction processing
    /// </summary>
    public class SaleTransactionResult
    {
        public bool IsSuccess { get; set; }
        public string TransactionID { get; set; }
        public string ReceiptNumber { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }

        public SaleTransactionResult()
        {
            IsSuccess = false;
            TransactionID = string.Empty;
            ReceiptNumber = string.Empty;
            Message = string.Empty;
        }
    }
}
