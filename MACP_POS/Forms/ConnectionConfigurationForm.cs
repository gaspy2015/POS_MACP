using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MACP_POS.DataAccess;

namespace MACP_POS.Forms
{
    public partial class ConnectionConfigurationForm : Form
    {
        private ConnectionManager _connectionManager;
        private bool isLoading;
        public ConnectionConfigurationForm()
        {
            InitializeComponent();
            _connectionManager = ConnectionManager.Instance;
            isLoading = false;
        }

        private void ConnectionConfigurationForm_Load(object sender, EventArgs e)
        {
            LoadCurrentConnection();
            LoadSavedConnections();
            UpdateControlStates();
        }

        private void LoadSavedConnections()
        {
            isLoading = true;
            cmbSavedConnections.Items.Clear();
            cmbSavedConnections.Items.Add("-- New Connection --");
            foreach (var connection in _connectionManager.SavedConnections)
            {
                cmbSavedConnections.Items.Add(connection.Name);
            }

            // Check if there's a current connection and select it
            var currentConnection = _connectionManager.CurrentConnection;
            if (currentConnection != null && !string.IsNullOrEmpty(currentConnection.Name))
            {
                int index = cmbSavedConnections.FindString(currentConnection.Name);
                if (index >= 0)
                {
                    cmbSavedConnections.SelectedIndex = index;
                }
                else
                {
                    cmbSavedConnections.SelectedIndex = 0; // Default to "New Connection"
                }
            }
            else
            {
                cmbSavedConnections.SelectedIndex = 0; // Default to "New Connection"
            }

            isLoading = false;
        }

        private void LoadCurrentConnection()
        {
            var currentConnection = _connectionManager.CurrentConnection;
            if (currentConnection != null)
            {
                txtServerName.Text = currentConnection.ServerName ?? "";
                txtDatabaseName.Text = currentConnection.DatabaseName ?? "";
                txtUsername.Text = currentConnection.Username ?? "";
                txtPassword.Text = currentConnection.Password ?? "";
                chkIntegratedSecurity.Checked = currentConnection.UseIntegratedSecurity;
            }
            else
            {
                // Clear fields if no current connection
                ClearConnectionFields();
            }
        }

        private void UpdateControlStates()
        {
            bool useWindowsAuth = chkIntegratedSecurity.Checked;
            lblUsername.Enabled = !useWindowsAuth;
            txtUsername.Enabled = !useWindowsAuth;
            lblPassword.Enabled = !useWindowsAuth;
            txtPassword.Enabled = !useWindowsAuth;

            btnDelete.Enabled = cmbSavedConnections.SelectedIndex > 0;
        }

        private void ClearConnectionFields()
        {
            txtServerName.Text = "";
            txtDatabaseName.Text = "";
            txtUsername.Text = "";
            txtPassword.Text = "";
            chkIntegratedSecurity.Checked = false;
            lblStatus.Text = "Ready";
            lblStatus.ForeColor = Color.Blue;
        }

        private void cmbSavedConnections_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isLoading || cmbSavedConnections.SelectedIndex <= 0)
            {
                ClearConnectionFields();
                return;
            }

            string selectedName = cmbSavedConnections.SelectedItem.ToString();
            var connection = _connectionManager.SavedConnections.FirstOrDefault(c => c.Name == selectedName);

            if (connection != null)
            {
                txtServerName.Text = connection.ServerName ?? "";
                txtDatabaseName.Text = connection.DatabaseName ?? "";
                txtUsername.Text = connection.Username ?? "";
                txtPassword.Text = connection.Password ?? "";
                chkIntegratedSecurity.Checked = connection.UseIntegratedSecurity;
            }

            UpdateControlStates();
        }

        private void chkIntegratedSecurity_CheckedChanged(object sender, EventArgs e)
        {
            UpdateControlStates();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            var connectionInfo = GetConnectionInfoFromForm();

            lblStatus.Text = "Testing connection . . .";
            lblStatus.ForeColor = Color.Blue;
            Application.DoEvents();

            try
            {
                if (_connectionManager.TestConnection(connectionInfo))
                {
                    lblStatus.Text = "Connection successful!";
                    lblStatus.ForeColor = Color.Green;
                }
                else
                {
                    lblStatus.Text = "Connection failed!";
                    lblStatus.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Connection error: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtServerName.Text))
            {
                MessageBox.Show("Please enter a server name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtServerName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDatabaseName.Text))
            {
                MessageBox.Show("Please enter a database name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDatabaseName.Focus();
                return false;
            }

            if (!chkIntegratedSecurity.Checked)
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    MessageBox.Show("Please enter a username or use Windows Authentication.",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUsername.Focus();
                    return false;
                }
            }

            return true;
        }

        private ConnectionInfo GetConnectionInfoFromForm()
        {
            return new ConnectionInfo
            {
                ServerName = txtServerName.Text.Trim(),
                DatabaseName = txtDatabaseName.Text.Trim(),
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Text,
                UseIntegratedSecurity = chkIntegratedSecurity.Checked,
                ConnectionTimeOut = 30
            };
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            var connectionInfo = GetConnectionInfoFromForm();

            // If it's a new connection, prompt for name
            if (cmbSavedConnections.SelectedIndex <= 0)
            {
                string name = PromptForConnectionName();
                if (string.IsNullOrEmpty(name))
                    return;

                connectionInfo.Name = name;
            }
            else
            {
                connectionInfo.Name = cmbSavedConnections.SelectedItem.ToString();
            }

            lblStatus.Text = "Saving and applying connection . . .";
            lblStatus.ForeColor = Color.Blue;
            Application.DoEvents();

            try
            {
                // Save the connection
                _connectionManager.SaveConnection(connectionInfo);

                // Set as current connection
                if (_connectionManager.SetCurrentConnection(connectionInfo))
                {
                    lblStatus.Text = "Connection saved and applied successfully!";
                    lblStatus.ForeColor = Color.Green;

                    // Refresh the combo box
                    LoadSavedConnections();

                    // Select the saved connection
                    int index = cmbSavedConnections.FindString(connectionInfo.Name);
                    if (index >= 0)
                        cmbSavedConnections.SelectedIndex = index;

                    MessageBox.Show("Connection has been saved and applied successfully!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    lblStatus.Text = "Connection saved but failed to apply!";
                    lblStatus.ForeColor = Color.Orange;
                    MessageBox.Show("Connection was saved but could not be applied. Please test the connection.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show("Error saving connection: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string PromptForConnectionName()
        {
            using (var form = new Form())
            {
                form.Text = "Save Connection";
                form.Size = new Size(350, 150);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var label = new Label
                {
                    Text = "Connection Name:",
                    Location = new Point(20, 20),
                    Size = new Size(100, 20)
                };
                form.Controls.Add(label);

                var textBox = new TextBox
                {
                    Location = new Point(20, 45),
                    Size = new Size(280, 21)
                };
                form.Controls.Add(textBox);

                var btnOk = new Button
                {
                    Text = "OK",
                    Location = new Point(145, 80),
                    Size = new Size(75, 25),
                    DialogResult = DialogResult.OK
                };
                form.Controls.Add(btnOk);

                var btnCancel = new Button
                {
                    Text = "Cancel",
                    Location = new Point(225, 80),
                    Size = new Size(75, 25),
                    DialogResult = DialogResult.Cancel
                };
                form.Controls.Add(btnCancel);

                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                textBox.Focus();

                if (form.ShowDialog() == DialogResult.OK)
                {
                    string name = textBox.Text.Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        MessageBox.Show("Please enter a connection name.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return null;
                    }

                    // Check if name already exists
                    if (_connectionManager.SavedConnections.Any(c =>
                        c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        var result = MessageBox.Show(
                            "A connection with this name already exists. Do you want to overwrite it?",
                            "Connection Exists",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (result != DialogResult.Yes)
                            return null;
                    }

                    return name;
                }
            }

            return null;
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            string name = PromptForConnectionName();
            if (string.IsNullOrEmpty(name))
                return;

            var connectionInfo = GetConnectionInfoFromForm();
            connectionInfo.Name = name;

            try
            {
                _connectionManager.SaveConnection(connectionInfo);

                lblStatus.Text = "Connection saved as '" + name + "'";
                lblStatus.ForeColor = Color.Green;

                // Refresh the combo box
                LoadSavedConnections();

                // Select the new connection
                int index = cmbSavedConnections.FindString(name);
                if (index >= 0)
                    cmbSavedConnections.SelectedIndex = index;

                MessageBox.Show("Connection saved successfully as '" + name + "'!", 
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show("Error saving connection: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (cmbSavedConnections.SelectedIndex <= 0)
                return;

            string connectionName = cmbSavedConnections.SelectedItem.ToString();

            var result = MessageBox.Show(
                "Are you sure you want to delete the connection '" + connectionName + "'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _connectionManager.DeleteConnection(connectionName);
                    LoadSavedConnections();
                    ClearConnectionFields();

                    lblStatus.Text = "Connection '" + connectionName + "' deleted successfully.";
                    lblStatus.ForeColor = Color.Green;
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "Error deleting connection: " + ex.Message;
                    lblStatus.ForeColor = Color.Red;
                    MessageBox.Show("Error deleting connection: " + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ConnectionConfigurationForm_Load(this, e);
        }
    }
}
