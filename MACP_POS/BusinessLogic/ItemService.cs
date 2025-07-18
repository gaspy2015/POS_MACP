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
    #region Data Models

    /// <summary>
    ///  Represents an item with its pricing and promotion information
    /// </summary>
    public class ItemWithPromotions
    {
        // Item Information
        public string Barcode { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public string ItemDescription { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal CostPrice { get; set; }
        public string Department { get; set; }
        public string DepartmentCode { get; set; }
        public string SubCategory { get; set; }
        public string SubcategoryCode { get; set; }
        public bool ItemActive { get; set; }
        public bool IsDiscountable { get; set; }

        // Current Price Information
        public decimal CurrentPrice { get; set; }
        public string PriceDescription { get; set; }
        public string PriceType { get; set; }

        // Promotion Information
        public int? PromotionID { get; set; }
        public string PromotionName { get; set; }
        public string PromotionType { get; set; }
        public string PromotionDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinimumPurchaseAmount { get; set; }
        public int? BuyQuantity { get; set; }
        public int? GetQuantity { get; set; }
        public int? MaxUsagePerCustomer { get; set; }
        public int? MaxUsageTotal { get; set; }
        public int CurrentUsageCount { get; set; }

        // Calculated prices and discounts
        public decimal FinalPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal? SpecialPrice { get; set; }

        // Promotion Flags
        public bool HasPromotion { get; set; }
        public bool IsQualifyingItem { get; set; }
        public bool IsRewardItem { get; set; }

        // Savings Calculation
        public decimal SavingsAmount { get; set; }
        
        // Tax information
        public string SalesTax { get; set; }
        public decimal SalesTaxPercent { get; set; }

        // Additional Item Details
        public int UOMFactor { get; set; }
        public string BrandID { get; set; }
        public string BrandDescription { get; set; }

        // Metadata
        public DateTime QueryTimestamp { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Represents the result of an item lookup operation
    /// </summary>
    public class ItemLookupResult
    {
        public bool Success { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public ItemWithPromotions Item { get; set; }
        public DateTime QueryTime { get; set; }

        public ItemLookupResult()
        {
            QueryTime = DateTime.Now;
        }
    }

    #endregion

    #region Service Class

    /// <summary>
    /// Service class for handling item lookups with promotions for POS operations
    /// </summary>
    public class ItemService : IDisposable
    {
        private readonly DBHelper _dbHelper;
        private bool _disposed = false;

        public ItemService()
        {
            _dbHelper = new DBHelper();
        }

        /// <summary>
        /// Gets an item with its current pricing and applicable promotions
        /// </summary>
        /// <param name="barcode">The barcode of the item to lookup</param>
        /// <returns>ItemLookupResult containing the item information or error details</returns>
        public ItemLookupResult GetItemWithPromotions(string barcode)
        {
            var result = new ItemLookupResult();

            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(barcode))
                {
                    result.Success = false;
                    result.ErrorCode = "INVALID_BARCODE";
                    result.ErrorMessage = "Barcode cannot be null or empty";
                    return result;
                }

                // Trim the barcode
                barcode = barcode.Trim();

                // Create parameter for stored procedure
                var parameters = new SqlParameter[]
                {
                    _dbHelper.CreateParameter("@Barcode", SqlDbType.NVarChar, 18, barcode)
                };

                // Execute stored procedure
                var dataTable = _dbHelper.ExecuteStoredProcedure("sp_GetItemWithPromotions_Safe", parameters);

                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    var row = dataTable.Rows[0];

                    // Check if this is an error result
                    if (row.Table.Columns.Contains("ErrorCode") && row["ErrorCode"] != DBNull.Value)
                    {
                        result.Success = false;
                        result.ErrorCode = row["ErrorCode"].ToString();
                        result.ErrorMessage = row["ErrorMessage"].ToString();
                        return result;
                    }

                    // Check for general error status
                    if (row.Table.Columns.Contains("Status") && row["Status"].ToString() == "ERROR")
                    {
                        result.Success = false;
                        result.ErrorCode = "DATABASE_ERROR";
                        result.ErrorMessage = row.Table.Columns.Contains("ErrorMessage") ?
                            row["ErrorMessage"].ToString() : "An error occurred while retrieving item information";
                        return result;
                    }

                    // Map the result to our model
                    result.Item = MapDataRowToItem(row);
                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                    result.ErrorCode = "NO_DATA";
                    result.ErrorMessage = "No data returned from stored procedure";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorCode = "EXCEPTION";
                result.ErrorMessage = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Maps a DataRow to an ItemWithPromotions object
        /// </summary>
        /// <param name="row">The DataRow containing item data</param>
        /// <returns>ItemWithPromotions object</returns>
        private ItemWithPromotions MapDataRowToItem(DataRow row)
        {
            var item = new ItemWithPromotions
            {
                // Item Information
                Barcode = GetSafeString(row, "barcode"),
                SKU = GetSafeString(row, "SKU"),
                ProductName = GetSafeString(row, "ProductName"),
                ItemDescription = GetSafeString(row, "ItemDescription"),
                RetailPrice = GetSafeDecimal(row, "RetailPrice"),
                CostPrice = GetSafeDecimal(row, "CostPrice"),
                Department = GetSafeString(row, "Department"),
                DepartmentCode = GetSafeString(row, "DepartmentCode"),
                SubCategory = GetSafeString(row, "SubCategory"),
                SubcategoryCode = GetSafeString(row, "SubcategoryCode"),
                ItemActive = GetSafeBool(row, "ItemActive"),
                IsDiscountable = GetSafeBool(row, "IsDiscountable"),

                // Current Price Information
                CurrentPrice = GetSafeDecimal(row, "CurrentPrice"),
                PriceDescription = GetSafeString(row, "PriceDescription"),
                PriceType = GetSafeString(row, "PriceType"),

                // Promotion Information
                PromotionID = GetSafeNullableInt(row, "PromotionID"),
                PromotionName = GetSafeString(row, "PromotionName"),
                PromotionType = GetSafeString(row, "PromotionType"),
                PromotionDescription = GetSafeString(row, "PromotionDescription"),
                StartDate = GetSafeNullableDateTime(row, "StartDate"),
                EndDate = GetSafeNullableDateTime(row, "EndDate"),
                MinimumPurchaseAmount = GetSafeNullableDecimal(row, "MinimumPurchaseAmount"),
                BuyQuantity = GetSafeNullableInt(row, "BuyQuantity"),
                GetQuantity = GetSafeNullableInt(row, "GetQuantity"),
                MaxUsagePerCustomer = GetSafeNullableInt(row, "MaxUsagePerCustomer"),
                MaxUsageTotal = GetSafeNullableInt(row, "MaxUsageTotal"),
                CurrentUsageCount = GetSafeInt(row, "CurrentUsageCount"),

                // Calculated Prices and Discounts
                FinalPrice = GetSafeDecimal(row, "FinalPrice"),
                DiscountPercent = GetSafeDecimal(row, "DiscountPercent"),
                DiscountAmount = GetSafeDecimal(row, "DiscountAmount"),
                SpecialPrice = GetSafeNullableDecimal(row, "SpecialPrice"),

                // Promotion Flags
                HasPromotion = GetSafeBool(row, "HasPromotion"),
                IsQualifyingItem = GetSafeBool(row, "IsQualifyingItem"),
                IsRewardItem = GetSafeBool(row, "IsRewardItem"),

                // Savings Calculation
                SavingsAmount = GetSafeDecimal(row, "SavingsAmount"),

                // Tax Information
                SalesTax = GetSafeString(row, "SalesTax"),
                SalesTaxPercent = GetSafeDecimal(row, "SalesTaxPercent"),

                // Additional Item Details
                UOMFactor = GetSafeInt(row, "UOMFactor"),
                BrandID = GetSafeString(row, "BrandID"),
                BrandDescription = GetSafeString(row, "BrandDescription"),

                // Metadata
                QueryTimestamp = GetSafeDateTime(row, "QueryTimestamp"),
                Status = GetSafeString(row, "Status")
            };

            return item;
        }

        #region Safe Data Extraction Methods

        private string GetSafeString(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? row[columnName].ToString()
                : string.Empty;
        }

        private decimal GetSafeDecimal(DataRow row, string columnName)
        {
            if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
            {
                decimal value;
                if (decimal.TryParse(row[columnName].ToString(), out value))
                    return value;
            }
            return 0m;
        }

        private decimal? GetSafeNullableDecimal(DataRow row, string columnName)
        {
            if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
            {
                decimal value;
                if (decimal.TryParse(row[columnName].ToString(), out value))
                    return value;
            }
            return null;
        }

        private int GetSafeInt(DataRow row, string columnName)
        {
            if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
            {
                int value;
                if (int.TryParse(row[columnName].ToString(), out value))
                    return value;
            }
            return 0;
        }

        private int? GetSafeNullableInt(DataRow row, string columnName)
        {
            if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
            {
                int value;
                if (int.TryParse(row[columnName].ToString(), out value))
                    return value;
            }
            return null;
        }

        private bool GetSafeBool(DataRow row, string columnName)
        {
            if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
            {
                bool value;
                if (bool.TryParse(row[columnName].ToString(), out value))
                    return value;

                // Handle integer representations of boolean
                int intValue;
                if (int.TryParse(row[columnName].ToString(), out intValue))
                    return intValue != 0;
            }
            return false;
        }

        private DateTime GetSafeDateTime(DataRow row, string columnName)
        {
            if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
            {
                DateTime value;
                if (DateTime.TryParse(row[columnName].ToString(), out value))
                    return value;
            }
            return DateTime.MinValue;
        }

        private DateTime? GetSafeNullableDateTime(DataRow row, string columnName)
        {
            if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
            {
                DateTime value;
                if (DateTime.TryParse(row[columnName].ToString(), out value))
                    return value;
            }
            return null;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Tests the database connection
        /// </summary>
        /// <returns>True if connection is successful</returns>
        public bool TestConnection()
        {
            return _dbHelper.TestConnection();
        }

        /// <summary>
        /// Gets server information for debugging
        /// </summary>
        /// <returns>Server information string</returns>
        public string GetServerInfo()
        {
            return _dbHelper.GetServerInfo();
        }

        /// <summary>
        /// Calculates the final price after tax
        /// </summary>
        /// <param name="item">The item to calculate tax for</param>
        /// <returns>Final price including tax</returns>
        public decimal CalculatePriceWithTax(ItemWithPromotions item)
        {
            if (item == null) return 0m;

            var taxAmount = item.FinalPrice * (item.SalesTaxPercent / 100m);
            return item.FinalPrice + taxAmount;
        }

        /// <summary>
        /// Formats currency for display
        /// </summary>
        /// <param name="amount">Amount to format</param>
        /// <returns>Formatted currency string</returns>
        public string FormatCurrency(decimal amount)
        {
            return amount.ToString("C2");
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_dbHelper != null) _dbHelper.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion
    }

     #endregion
}
