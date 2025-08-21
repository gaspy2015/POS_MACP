using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MACP_POS.BusinessLogic;

namespace MACP_POS
{
    public partial class FormMain3 : Form
    {
        private ItemService _itemService;
        private CashierSessionManager _sessionMnager;

        public FormMain3()
        {
            InitializeComponent();
            _itemService = new ItemService();
            _sessionMnager = new CashierSessionManager();
        }

        #region Basic Item lookup from barcode scanner

        /// <summary>
        /// Handle barcode scanner input
        /// </summary>
        /// <param name="barcode">Scanned barcode</param>
        private void ProcessScannedBarcode(string barcode)
        {
            try
            {
                // Show oading indicator
                this.Cursor = Cursors.WaitCursor;
                lblStatus.Text = "Looking up item...";

                // Lookup the item
                var result = _itemService.GetItemWithPromotions(barcode);

                if (result.Success)
                {
                    // Add item to cart
                    AddItemToCart(result.Item);

                    // Update display
                    
                    // Show any promotions
                    if (result.Item.HasPromotion)
                    {
                        ShowPromotionMessage(result.Item);
                    }

                    lblStatus.Text = "Item added successfully";
                }
                else
                {
                    // Handle errors
                    HandleItemLookupError(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error processing barcode: {0}", ex.Message), "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        #endregion

        #region Adding Items to cart

        /// <summary>
        /// Example: Add item to the shopping cart
        /// </summary>
        /// <param name="item">Item to add to cart</param>
        private void AddItemToCart(ItemWithPromotions item)
        {
            try
            {
                // Create a new cart item
                var cartItem = new CartItem
                {
                    Barcode = item.Barcode,
                    SKU = item.SKU,
                    ProductName = item.ProductName,
                    Description = item.ItemDescription,
                    RetailPrice = item.RetailPrice,
                    CurrentPrice = item.CurrentPrice,
                    FinalPrice = item.FinalPrice,
                    Quantity = 1,
                    HasPromotion = item.HasPromotion,
                    PromotionName = item.PromotionName,
                    SavingsAmount = item.SavingsAmount,
                    TaxPercent = item.SalesTaxPercent,
                    UOMFactor = item.UOMFactor,
                    ItemObject = item // store the full item for reference
                };

                // Add to cart  display (Datagridview or similar) - call created method
                AddToCartDisplay(cartItem);

                // Update Totals - call created method for this
                // --UpdateCartTotals(); 

                // Focus back to barcode input
                //txtBarcode.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error adding item to cart: {0}", ex.Message), "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Error Handling

        /// <summary>
        /// Example: Handle item lookup errors
        /// </summary>
        /// <param name="result">Failed lookup result</param>
        private void HandleItemLookupError(ItemLookupResult result)
        {
            string message;
            MessageBoxIcon icon;

            switch (result.ErrorCode)
            {
                case "ITEM_NOT_FOUND":
                    message = "Item not found or inactive, Please check the barcode.";
                    icon = MessageBoxIcon.Warning;
                    break;

                case "INVALID_BARCODE":
                    message = "Invalid barcode format. Please try again.";
                    icon = MessageBoxIcon.Error;
                    break;

                case "DATABASE_ERROR":
                    message = string.Format("Database error: {0}", result.ErrorMessage);
                    icon = MessageBoxIcon.Error;
                    break;

                case "EXCEPTION":
                    message = string.Format("System error: {0}", result.ErrorMessage);
                    icon = MessageBoxIcon.Error;
                    break;

                default:
                    message = string.Format("Unknown error: {0}", result.ErrorMessage);
                    icon = MessageBoxIcon.Error;
                    break;
            }

            MessageBox.Show(message, "Item Lookup Error", MessageBoxButtons.OK, icon);
            lblStatus.Text = "Error: " + result.ErrorMessage; 
        }

        #endregion

        #region Promotion Messages

        /// <summary>
        /// Example: Show promotion messages to user
        /// </summary>
        /// <param name="item">Item with promotion</param>
        private void ShowPromotionMessage(ItemWithPromotions item)
        {
            if (!item.HasPromotion) return;
            
           var message = "PROMOTION APPLIED!\n\n";
                message += String.Format("Promotion: {0}\n", item.PromotionName);
                message += String.Format("Description: {0}\n", item.PromotionDescription);
                message += String.Format("You Save: {0}\n", _itemService.FormatCurrency(item.SavingsAmount));
            
            if (item.DiscountPercent > 0)
            {
                message += String.Format("Discount: {0}% OFF\n", item.DiscountPercent);
            }
            
            if (item.SpecialPrice.HasValue)
            {
                message += String.Format("Special Price: {0}\n", _itemService.FormatCurrency(item.SpecialPrice.Value));
            }
            
            // Show promotion message (you might want to use a custom form instead)
            MessageBox.Show(message, "Promotion Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        /// <summary>
        /// Example: Cart item class
        /// </summary>
        public class CartItem
        {
            public string Barcode { get; set; }
            public string SKU { get; set; }
            public string ProductName { get; set; }
            public string Description { get; set; }
            public decimal RetailPrice { get; set; }
            public decimal CurrentPrice { get; set; }
            public decimal FinalPrice { get; set; }
            public int Quantity { get; set; }
            public bool HasPromotion { get; set; }
            public string PromotionName { get; set; }
            public decimal SavingsAmount { get; set; }
            public decimal TaxPercent { get; set; }
            public int UOMFactor { get; set; }
            public ItemWithPromotions ItemObject { get; set; }

            public decimal LineTotal
            {
                get { return FinalPrice * Quantity; }
            }

            public decimal LineTax
            {
                get { return LineTotal * (TaxPercent / 100); }
            }

            public decimal LineTotalWithTax
            {
                get { return LineTotal + LineTax; }
            }
        }

        /// <summary>
        /// Add item to cart display
        /// </summary>
        /// <param name="cartItem">Cart item to add</param>
        private void AddToCartDisplay(CartItem cartItem)
        {
            // Having a DataGridView named dgvCart
            var rowIndex = dgvCart.Rows.Add();
            var row = dgvCart.Rows[rowIndex];

            row.Cells["Barcode"].Value = cartItem.Barcode;
            row.Cells["ProductName"].Value = cartItem.ProductName;
            row.Cells["Price"].Value = cartItem.FinalPrice;
            row.Cells["Quantity"].Value = cartItem.Quantity;
            row.Cells["Total"].Value = cartItem.LineTotal;
            row.Cells["HasPromotion"].Value = cartItem.HasPromotion;
            row.Tag = cartItem; // Store the full cart item object
        }

        /// <summary>
        /// Update cart totals
        /// </summary>
        private void UpdateCartTotals()
        {
            decimal subTotal = 0;
            decimal totalTax = 0;
            decimal totalSavings = 0;

            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                var cartItem = row.Tag as CartItem;
                if (cartItem != null)
                {
                    subTotal += cartItem.LineTotal;
                    totalTax += cartItem.LineTax;
                    totalSavings += cartItem.SavingsAmount * cartItem.Quantity;
                }
            }

            var grandTotal = subTotal + totalTax;

            // Update total labels
            //lblSubtotal.Text = _itemService.FormatCurrency(subtotal);
            //lblTax.Text = _itemService.FormatCurrency(totalTax);
            //lblSavings.Text = _itemService.FormatCurrency(totalSavings);
            //lblGrandTotal.Text = _itemService.FormatCurrency(grandTotal);
        }

        /// <summary>
        /// Form load event
        /// </summary>
        private void FormMain3_Load(object sender, EventArgs e)
        {
            try
            {
                // Test database connection
                if (!_itemService.TestConnection())
                {
                    MessageBox.Show("Cannot connect to database. Please check your connection.", "Database Connection Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Initialize the form
                InitializeCartGrid();
                txtBarcode.Focus();
                lblStatus.Text = "Ready";
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error initializing POS: {0}", ex.Message), "Initialization Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormMain3_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Dispose of resources
                if (_itemService != null) _itemService.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error disposing ItemService: {0}", ex.Message));
            }
        }

        #region Helper Methods

        /// <summary>
        /// Initialize the cart data grid
        /// </summary>
        private void InitializeCartGrid()
        {
            dgvCart.Columns.Clear();
            dgvCart.Columns.Add("Barcode", "Barcode");
            dgvCart.Columns.Add("ProductName", "Product Name");
            dgvCart.Columns.Add("Price", "Price");
            dgvCart.Columns.Add("Quantity", "Qty");
            dgvCart.Columns.Add("Total", "Total");
            dgvCart.Columns.Add("HasPromotion", "Promo");

            // Set columns widths
            dgvCart.Columns["Barcode"].Width = 100;
            dgvCart.Columns["ProductName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvCart.Columns["Price"].Width = 80;
            dgvCart.Columns["Quantity"].Width = 50;
            dgvCart.Columns["Total"].Width = 80;
            dgvCart.Columns["HasPromotion"].Width = 50;

            // set column types
            dgvCart.Columns["Price"].DefaultCellStyle.Format = "C2";
            dgvCart.Columns["Total"].DefaultCellStyle.Format = "C2";
            dgvCart.Columns["HasPromotion"].DefaultCellStyle.Format = "Yes;No";

            // Center align some columns
            dgvCart.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCart.Columns["HasPromotion"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Right align currency columns
            dgvCart.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvCart.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        /// <summary>
        /// Clear the current transaction
        /// </summary>
        private void ClearTransaction()
        {
            dgvCart.Rows.Clear();
            UpdateCartTotals();
            txtBarcode.Clear();
            txtBarcode.Focus();
            lblStatus.Text = "Ready";

            // Clear some fields
        }

        #endregion

        #region Starting Cashier Session

        /// <summary>
        /// Validate inputs before starting session
        /// </summary>
        private bool ValidateSessionInputs()
        {
            // Check terminal selection
            if (string.IsNullOrWhiteSpace(lblTerminalId.Text) || lblTerminalId.Text == "POS")
            {
                MessageBox.Show("Please select a terminal.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Check cashier ID
            if (string.IsNullOrWhiteSpace(lblCashierId.Text) || lblCashierId.Text == "OPERATOR ID")
            {
                MessageBox.Show("Please enter cashier ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Check starting amount
            decimal startingAmount;
            if (!decimal.TryParse(txtBarcode.Text, out startingAmount))
            {
                MessageBox.Show("Please enter a valid starting amount.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBarcode.Focus();
                return false;
            }

            if (startingAmount < 0)
            {
                MessageBox.Show("Starting amount cannot be negative.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBarcode.Focus();
                return false;
            }

            return true;
        }

        private void EnableSessionControls(bool sessionActive)
        {
            // Enable/disable controls based on session status
            btnStartSession.Enabled = !sessionActive;
            btnEndSession.Enabled = sessionActive;
            // ... other controls
        }

        #endregion

        private void txtBarcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                var barcode = txtBarcode.Text.Trim();

                if (!string.IsNullOrEmpty(barcode))
                {
                    ProcessScannedBarcode(barcode);
                    txtBarcode.Clear();
                }
            }
        }

        private void btnStartSession_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate form inputs first
                if (!ValidateSessionInputs())
                    return;

                // Show loading cursor
                this.Cursor = Cursors.WaitCursor;

                // Create request object
                var request = new CashierSessionManager.StartSessionRequest
                {
                    TerminalID = lblTerminalId.Text.Trim(),
                    CashierID = lblCashierId.Text.Trim(),
                    StartingAmount = decimal.Parse(txtBarcode.Text),
                    UserID = lblCashierId.Text.Trim()
                };

                // Call business logic
                var result = _sessionMnager.StartCashierSession(request);

                if (result.IsSuccess)
                {
                    // Success - update UI
                    MessageBox.Show(string.Format("Session started succesfully!\nSession ID: {0}", result.SessionID), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Store session ID for later use
                    Properties.Settings.Default.CurrentSessionID = result.SessionID;
                    Properties.Settings.Default.Save();

                    // Enable/disable relevant controls
                    EnableSessionControls(true);
                    txtBarcode.Clear();
                }
                else
                {
                    // Show error message
                    MessageBox.Show(result.ErrorMessage, "Error Starting Session", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            finally
            {
                // Reset cursor
                this.Cursor = Cursors.Default;
            }
        }

        private void LogError(string operation, string message, Exception ex)
        {
            //
        }

        private void button9_Click(object sender, EventArgs e)
        {

        }
    }

        
}
