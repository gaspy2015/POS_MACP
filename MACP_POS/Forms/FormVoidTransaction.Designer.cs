namespace MACP_POS.Forms
{
    partial class FormVoidTransaction
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxTransaction = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtTransactionID = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.pnlTransactionDetails = new System.Windows.Forms.Panel();
            this.groupBoxDetails = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTransactionDate = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblTotalAmount = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblCashier = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblItemCount = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblPrivilegeCard = new System.Windows.Forms.Label();
            this.groupBoxVoid = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.cmbVoidReason = new System.Windows.Forms.ComboBox();
            this.lblApprovalCode = new System.Windows.Forms.Label();
            this.txtApprovalCode = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnVoid = new System.Windows.Forms.Button();
            this.groupBoxTransaction.SuspendLayout();
            this.pnlTransactionDetails.SuspendLayout();
            this.groupBoxDetails.SuspendLayout();
            this.groupBoxVoid.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxTransaction
            // 
            this.groupBoxTransaction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTransaction.Controls.Add(this.btnSearch);
            this.groupBoxTransaction.Controls.Add(this.txtTransactionID);
            this.groupBoxTransaction.Controls.Add(this.label1);
            this.groupBoxTransaction.Location = new System.Drawing.Point(12, 12);
            this.groupBoxTransaction.Name = "groupBoxTransaction";
            this.groupBoxTransaction.Size = new System.Drawing.Size(460, 60);
            this.groupBoxTransaction.TabIndex = 0;
            this.groupBoxTransaction.TabStop = false;
            this.groupBoxTransaction.Text = "Transaction Search";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Transaction ID:";
            // 
            // txtTransactionID
            // 
            this.txtTransactionID.Location = new System.Drawing.Point(120, 25);
            this.txtTransactionID.MaxLength = 20;
            this.txtTransactionID.Name = "txtTransactionID";
            this.txtTransactionID.Size = new System.Drawing.Size(220, 20);
            this.txtTransactionID.TabIndex = 1;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(350, 23);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            // 
            // pnlTransactionDetails
            // 
            this.pnlTransactionDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTransactionDetails.Controls.Add(this.groupBoxDetails);
            this.pnlTransactionDetails.Location = new System.Drawing.Point(12, 78);
            this.pnlTransactionDetails.Name = "pnlTransactionDetails";
            this.pnlTransactionDetails.Size = new System.Drawing.Size(460, 160);
            this.pnlTransactionDetails.TabIndex = 1;
            this.pnlTransactionDetails.Visible = false;
            // 
            // groupBoxDetails
            // 
            this.groupBoxDetails.Controls.Add(this.lblPrivilegeCard);
            this.groupBoxDetails.Controls.Add(this.label7);
            this.groupBoxDetails.Controls.Add(this.lblItemCount);
            this.groupBoxDetails.Controls.Add(this.label6);
            this.groupBoxDetails.Controls.Add(this.lblCashier);
            this.groupBoxDetails.Controls.Add(this.label5);
            this.groupBoxDetails.Controls.Add(this.lblStatus);
            this.groupBoxDetails.Controls.Add(this.label4);
            this.groupBoxDetails.Controls.Add(this.lblTotalAmount);
            this.groupBoxDetails.Controls.Add(this.label3);
            this.groupBoxDetails.Controls.Add(this.lblTransactionDate);
            this.groupBoxDetails.Controls.Add(this.label2);
            this.groupBoxDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxDetails.Location = new System.Drawing.Point(0, 0);
            this.groupBoxDetails.Name = "groupBoxDetails";
            this.groupBoxDetails.Size = new System.Drawing.Size(460, 160);
            this.groupBoxDetails.TabIndex = 0;
            this.groupBoxDetails.TabStop = false;
            this.groupBoxDetails.Text = "Transaction Details";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Date:";
            // 
            // lblTransactionDate
            // 
            this.lblTransactionDate.AutoSize = true;
            this.lblTransactionDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTransactionDate.Location = new System.Drawing.Point(100, 20);
            this.lblTransactionDate.Name = "lblTransactionDate";
            this.lblTransactionDate.Size = new System.Drawing.Size(41, 13);
            this.lblTransactionDate.TabIndex = 1;
            this.lblTransactionDate.Text = "label3";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Total Amount:";
            // 
            // lblTotalAmount
            // 
            this.lblTotalAmount.AutoSize = true;
            this.lblTotalAmount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalAmount.Location = new System.Drawing.Point(100, 50);
            this.lblTotalAmount.Name = "lblTotalAmount";
            this.lblTotalAmount.Size = new System.Drawing.Size(41, 13);
            this.lblTotalAmount.TabIndex = 3;
            this.lblTotalAmount.Text = "label4";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 80);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Status:";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.Location = new System.Drawing.Point(100, 80);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(41, 13);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "label5";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(250, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Cashier:";
            // 
            // lblCashier
            // 
            this.lblCashier.AutoSize = true;
            this.lblCashier.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCashier.Location = new System.Drawing.Point(350, 20);
            this.lblCashier.Name = "lblCashier";
            this.lblCashier.Size = new System.Drawing.Size(41, 13);
            this.lblCashier.TabIndex = 7;
            this.lblCashier.Text = "label6";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(250, 50);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(61, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Item Count:";
            // 
            // lblItemCount
            // 
            this.lblItemCount.AutoSize = true;
            this.lblItemCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblItemCount.Location = new System.Drawing.Point(350, 50);
            this.lblItemCount.Name = "lblItemCount";
            this.lblItemCount.Size = new System.Drawing.Size(41, 13);
            this.lblItemCount.TabIndex = 9;
            this.lblItemCount.Text = "label7";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(250, 80);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(75, 13);
            this.label7.TabIndex = 10;
            this.label7.Text = "Privilege Card:";
            // 
            // lblPrivilegeCard
            // 
            this.lblPrivilegeCard.AutoSize = true;
            this.lblPrivilegeCard.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrivilegeCard.Location = new System.Drawing.Point(350, 80);
            this.lblPrivilegeCard.Name = "lblPrivilegeCard";
            this.lblPrivilegeCard.Size = new System.Drawing.Size(41, 13);
            this.lblPrivilegeCard.TabIndex = 11;
            this.lblPrivilegeCard.Text = "label8";
            // 
            // groupBoxVoid
            // 
            this.groupBoxVoid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxVoid.Controls.Add(this.txtApprovalCode);
            this.groupBoxVoid.Controls.Add(this.lblApprovalCode);
            this.groupBoxVoid.Controls.Add(this.cmbVoidReason);
            this.groupBoxVoid.Controls.Add(this.label8);
            this.groupBoxVoid.Location = new System.Drawing.Point(12, 244);
            this.groupBoxVoid.Name = "groupBoxVoid";
            this.groupBoxVoid.Size = new System.Drawing.Size(460, 90);
            this.groupBoxVoid.TabIndex = 2;
            this.groupBoxVoid.TabStop = false;
            this.groupBoxVoid.Text = "Void Information";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(15, 28);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(71, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "Void Reason:";
            // 
            // cmbVoidReason
            // 
            this.cmbVoidReason.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVoidReason.FormattingEnabled = true;
            this.cmbVoidReason.Location = new System.Drawing.Point(120, 25);
            this.cmbVoidReason.Name = "cmbVoidReason";
            this.cmbVoidReason.Size = new System.Drawing.Size(320, 21);
            this.cmbVoidReason.TabIndex = 1;
            // 
            // lblApprovalCode
            // 
            this.lblApprovalCode.AutoSize = true;
            this.lblApprovalCode.Location = new System.Drawing.Point(15, 58);
            this.lblApprovalCode.Name = "lblApprovalCode";
            this.lblApprovalCode.Size = new System.Drawing.Size(80, 13);
            this.lblApprovalCode.TabIndex = 2;
            this.lblApprovalCode.Text = "Approval Code:";
            this.lblApprovalCode.Visible = false;
            // 
            // txtApprovalCode
            // 
            this.txtApprovalCode.Location = new System.Drawing.Point(120, 55);
            this.txtApprovalCode.MaxLength = 20;
            this.txtApprovalCode.Name = "txtApprovalCode";
            this.txtApprovalCode.Size = new System.Drawing.Size(220, 20);
            this.txtApprovalCode.TabIndex = 3;
            this.txtApprovalCode.Visible = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(316, 350);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnVoid
            // 
            this.btnVoid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVoid.BackColor = System.Drawing.Color.Red;
            this.btnVoid.Enabled = false;
            this.btnVoid.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVoid.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVoid.ForeColor = System.Drawing.Color.White;
            this.btnVoid.Location = new System.Drawing.Point(397, 350);
            this.btnVoid.Name = "btnVoid";
            this.btnVoid.Size = new System.Drawing.Size(75, 30);
            this.btnVoid.TabIndex = 4;
            this.btnVoid.Text = "VOID";
            this.btnVoid.UseVisualStyleBackColor = false;
            // 
            // FormVoidTransaction
            // 
            this.AcceptButton = this.btnVoid;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(484, 391);
            this.Controls.Add(this.btnVoid);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.groupBoxVoid);
            this.Controls.Add(this.pnlTransactionDetails);
            this.Controls.Add(this.groupBoxTransaction);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormVoidTransaction";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Void Transaction";
            this.groupBoxTransaction.ResumeLayout(false);
            this.groupBoxTransaction.PerformLayout();
            this.pnlTransactionDetails.ResumeLayout(false);
            this.groupBoxDetails.ResumeLayout(false);
            this.groupBoxDetails.PerformLayout();
            this.groupBoxVoid.ResumeLayout(false);
            this.groupBoxVoid.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxTransaction;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.TextBox txtTransactionID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pnlTransactionDetails;
        private System.Windows.Forms.GroupBox groupBoxDetails;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblTotalAmount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblTransactionDate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblItemCount;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblCashier;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblPrivilegeCard;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBoxVoid;
        private System.Windows.Forms.TextBox txtApprovalCode;
        private System.Windows.Forms.Label lblApprovalCode;
        private System.Windows.Forms.ComboBox cmbVoidReason;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnVoid;

        
    }
}