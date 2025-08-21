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

namespace MACP_POS.Forms
{
    public partial class FormVoidTransaction : Form
    {
        #region Private Fields

        private VoidTransactionManager _voidManager;
        private string currentUserId;
        private string currentEmployeeId;
        private bool requiresApproval = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets whether the void operation was successful
        /// </summary>
        public bool VoidSuccessful { get; set; }

        /// <summary>
        /// Gets the voided transaction ID
        /// </summary>
        public string VoidedTransactionID { get; set; }

        #endregion

        public FormVoidTransaction(string userId, string employeeId)
        {
            InitializeComponent();
            currentUserId = userId;
            currentEmployeeId = employeeId;
            _voidManager = new VoidTransactionManager();
        }

        #region Event Handlers

        private void FormVoidTransaction_Load(object sender, EventArgs e)
        {
            // Cnter the form on parent
            this.CenterToParent();
        }

        private void LoadVoidReasons()
        {
            try
            {
                var voidReasons = _voidManager.GetVoidReasons();

                cmbVoidReason.DisplayMember = "ReasonDescription";
                cmbVoidReason.ValueMember = "ReasonID";
                cmbVoidReason.DataSource = voidReasons;
                cmbVoidReason.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ShowError("Error loading void reasons: " + ex.Message);
            }
        }

        private void txtTransactionID_TextChanged(object sender, EventArgs e)
        {
            // Clear transaction details when ID changes
            ClearTransactionDetails();
            btnVoid.Enabled = false;
        }

        private void txtTransactionID_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only aphanumeric characters and control keys
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }

            // Search on Enter key
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnSearch_Click(sender, e);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadTransactionDetails();
        }

        private void cmbVoidReason_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbVoidReason.SelectedItem != null)
            {
                var selectedReason = (VoidReason)cmbVoidReason.SelectedItem;
                requiresApproval = selectedReason.RequiredApproval;

                // Show/hide approval code field
                lblApprovalCode.Visible = requiresApproval;
                txtApprovalCode.Visible = requiresApproval;

                if (requiresApproval)
                {
                    lblApprovalCode.Text = "Approval Code: *";
                    txtApprovalCode.Focus();
                }
                else
                {
                    txtApprovalCode.Text = "";
                }

                ValidateForm();
            }
        }

        private void btnVoid_Click(object sender, EventArgs e)
        {
            ProcessVoidTransaction();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FormVoidTransaction_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_voidManager != null)
            {
                _voidManager.Dispose();
            }
        }

        #endregion

        #region Private Methods

        private void LoadTransactionDetails()
        {
            if (string.IsNullOrWhiteSpace(txtTransactionID.Text))
            {
                ShowWarning("Please enter a transaction ID.");
                return;
            }

            try
            {
                // Show loading indicator
                this.Cursor = Cursors.WaitCursor;
                btnSearch.Enabled = false;

                // Check if transaction can be voided
                var canVoidResult = _voidManager.CanTransactionBeVoided(txtTransactionID.Text.Trim());

                if (!canVoidResult.CanVoid)
                {
                    ShowWarning(canVoidResult.Reason);
                    ClearTransactionDetails();
                    return;
                }

                // Get transaction summary
                var summary = _voidManager.GetTrasnactionSummary(txtTransactionID.Text.Trim());

                if (!summary.Found)
                {
                    ShowWarning("Transaction not found or cannot be voided.");
                    ClearTransactionDetails();
                    return;
                }

                // Display transaction details
                DisplayTransactionSummary(summary);

                // Enable void button if form is valid
                ValidateForm();
            }
            catch (Exception ex)
            {
                ShowError("Error loading transaction details: " + ex.Message);
                ClearTransactionDetails();
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnSearch.Enabled = true;
            }
        }

        private void DisplayTransactionSummary(TransactionSummary summary)
        {
            lblTransactionDate.Text = summary.TransactionDate.ToString("MM/dd/yyyy HH:mm:ss");
            lblTotalAmount.Text = summary.TotalAmount.ToString("C2");
            lblStatus.Text = summary.Status;
            lblCashier.Text = summary.CashierID;
            lblItemCount.Text = summary.ItemCount.ToString();
            lblPrivilegeCard.Text = string.IsNullOrEmpty(summary.PrivilegeCardNumber) ?
                "None" : summary.PrivilegeCardNumber;

            // Show transaction details panel
            pnlTransactionDetails.Visible = true;
        }

        private void ClearTransactionDetails()
        {
            pnlTransactionDetails.Visible = false;
            lblTransactionDate.Text = "";
            lblTotalAmount.Text = "";
            lblStatus.Text = "";
            lblCashier.Text = "";
            lblItemCount.Text = "";
            lblPrivilegeCard.Text = "";
        }

        private void ProcessVoidTransaction()
        {
            if (!ValidateVoidRequest())
                return;

            try
            {
                // Show confirmation dialog
                var confirmResult = MessageBox.Show(
                        string.Format(
                            "Are you sure you want to void this transaction?\n\nTransaction ID: {0}\nAmount: {1}\nReason: {2}",
                            txtTransactionID.Text,
                            lblTotalAmount.Text,
                            cmbVoidReason.Text
                        ),
                        "Confirm Void Transaction",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                if (confirmResult != DialogResult.Yes)
                    return;

                // Show processing indicator
                this.Cursor = Cursors.WaitCursor;
                btnVoid.Enabled = false;

                // Create void request
                var request = new VoidTransactionRequest { 
                    TransactionID = txtTransactionID.Text.Trim(),
                    VoidReasonID = ((VoidReason)cmbVoidReason.SelectedItem).ReasonID,
                    VoidedBy = currentEmployeeId,
                    ApprovalCode = requiresApproval ? txtApprovalCode.Text.Trim() : null,
                    UserID = currentUserId
                };

                // Execute void operation
                var response = _voidManager.VoidTransaction(request);

                if (response.IsSucess)
                {
                    VoidSuccessful = true;
                    VoidedTransactionID = response.TransactionID;

                    ShowSuccess(string.Format(
                        "Transaction voided successfully!\n\n" +
                        "Transaction ID: {0}\n" +
                        "Voided at: {1:MM/dd/yyyy HH:mm:ss}\n" +
                        "Details voided: {2}\n" +
                        "Payments voided: {3}",
                        response.TransactionID,
                        response.VoidedTimestamp,
                        response.DetailRowsVoided,
                        response.PaymentRowsVoided
                    ));

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    ShowError(string.Format("Failed to void transaction: \n{0}", response.ErrorMessage));
                }

            }
            catch (Exception ex)
            {
                ShowError(string.Format("Error processing void transaction: {0}", ex.Message));
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnVoid.Enabled = true;
            }
        }

        private bool ValidateVoidRequest()
        {
            if (string.IsNullOrWhiteSpace(txtTransactionID.Text))
            {
                ShowWarning("Transaction ID is required,");
                txtTransactionID.Focus();
                return false;
            }

            if (cmbVoidReason.SelectedItem == null)
            {
                ShowWarning("Please select a void reason.");
                cmbVoidReason.Focus();
                return false;
            }

            if (requiresApproval && string.IsNullOrWhiteSpace(txtApprovalCode.Text))
            {
                ShowWarning("Approval code is required for this void reason.");
                txtApprovalCode.Focus();
                return false;
            }

            if (!pnlTransactionDetails.Visible)
            {
                ShowWarning("Please search for the transaction first.");
                btnSearch.Focus();
                return false;
            }

            return true;
        }

        private void ValidateForm()
        {
            btnVoid.Enabled = !string.IsNullOrWhiteSpace(txtTransactionID.Text) &&
                cmbVoidReason.SelectedItem != null &&
                pnlTransactionDetails.Visible &&
                (!requiresApproval || !string.IsNullOrWhiteSpace(txtApprovalCode.Text));
        }

        private void ResetForm()
        {
            txtTransactionID.Text = "";
            cmbVoidReason.SelectedIndex = -1;
            txtApprovalCode.Text = "";

            ClearTransactionDetails();

            lblApprovalCode.Visible = false;
            txtApprovalCode.Visible = false;

            btnVoid.Enabled = false;
            VoidSuccessful = false;
            VoidedTransactionID = "";
        }

        #endregion

        #region Helper Method

        private void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowWarning(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        #endregion

    }
}
