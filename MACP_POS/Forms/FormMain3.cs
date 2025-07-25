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

        public FormMain3()
        {
            InitializeComponent();
            _itemService = new ItemService();
        }

        #region Basic Itel lookup from barcode scanner

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
                
            }
            catch (Exception ex)
            {
                
                throw;
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
                //--AddToCartDisplay(cartItem);

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
    }

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
}
