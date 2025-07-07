using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Configuration;
using System.Data.SqlClient;

namespace MACP_POS.DataAccess
{
    public sealed class ConnectionManager
    {
        private static ConnectionManager instance = null;
        private static readonly object padlock = new object();
        private ConnectionInfo currentConnection;
        private List<ConnectionInfo> saveConnections;

        private ConnectionManager()
        {
            saveConnections = new List<ConnectionInfo>();
            LoadConnections();
        }

        public static ConnectionManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new ConnectionManager();
                    return instance;
                }
            }
        }

        public ConnectionInfo CurrentConnection
        {
            get { return currentConnection; }
        }

        public List<ConnectionInfo> SavedConnections
        {
            get { return new List<ConnectionInfo>(saveConnections); }
        }

        public string GetConnectionString()
        {
            if (currentConnection != null)
                return currentConnection.ToConnectionString();

            // Fallback to app.config
            return ConfigurationManager.ConnectionStrings["DefaultConnection"] != null
                ? ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString
                : "";
        }

        public bool TestConnection(ConnectionInfo connectionInfo)
        {
            try
            {
                using (var connection = new SqlConnection(connectionInfo.ToConnectionString()))
                {
                    connection.Open();
                    return true;
                }
            }
            catch 
            {
                return false;
            }
        }

        public bool SetCurrentConnection(ConnectionInfo connectionInfo)
        {
            if (TestConnection(connectionInfo))
            {
                currentConnection = connectionInfo;
                UpdateAppConfig(connectionInfo);
                SaveConnectionsToFile(); // Add this line to save current connection info
                return true;
            }
            return false;
        }

        public void SaveConnection(ConnectionInfo connectionInfo)
        {
            // Remove existing connection with same name
            saveConnections.RemoveAll(c => c.Name.Equals(connectionInfo.Name, StringComparison.OrdinalIgnoreCase));

            // Add new connection
            saveConnections.Add(connectionInfo);

            // Save to file
            SaveConnectionsToFile();
        }

        public void DeleteConnection(string name)
        {
            saveConnections.RemoveAll(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            SaveConnectionsToFile();
        }

        public void LoadConnections()
        {
            try
            {
                // Load saved connections from file first
                LoadConnectionsFromFile();

                // Load current connection from app.config only if no current connection is set
                if (currentConnection == null)
                {
                    var defaultConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"];
                    string connectionString = (defaultConnectionString != null) ? defaultConnectionString.ConnectionString : null;
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        currentConnection = ConnectionInfo.FromConnectionString(connectionString, "Default");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine("Error loading connections: " + ex.Message);
            }
        }

        private void LoadConnectionsFromFile()
        {
            try
            {
                string filePath = GetConnectionsFilePath();
                if (File.Exists(filePath))
                {
                    var xml = new XmlDocument();
                    xml.Load(filePath);

                    // Load current connection name
                    string currentConnectionName = "";
                    var currentNode = xml.SelectSingleNode("//CurrentConnection");
                    if (currentNode != null)
                    {
                        currentConnectionName = currentNode.InnerText;
                    }

                    var connectionNodes = xml.SelectNodes("//Connection");
                    foreach (XmlNode node in connectionNodes)
                    {
                        var connectionInfo = new ConnectionInfo
                        {
                            Name = node.Attributes["name"] != null ? node.Attributes["name"].Value : "",
                            ServerName = DecryptString(node.SelectSingleNode("ServerName") != null ? node.SelectSingleNode("ServerName").InnerText : ""),
                            DatabaseName = node.SelectSingleNode("DatabaseName") != null ? node.SelectSingleNode("DatabaseName").InnerText : "",
                            Username = DecryptString(node.SelectSingleNode("Username") != null ? node.SelectSingleNode("Username").InnerText : ""),
                            Password = DecryptString(node.SelectSingleNode("Password") != null ? node.SelectSingleNode("Password").InnerText : ""),
                            UseIntegratedSecurity = bool.Parse(node.SelectSingleNode("UseIntegratedSecurity") != null ? node.SelectSingleNode("UseIntegratedSecurity").InnerText : "false"),
                            ConnectionTimeOut = int.Parse(node.SelectSingleNode("ConnectionTimeout") != null ? node.SelectSingleNode("ConnectionTimeout").InnerText : "30")
                        };

                        saveConnections.Add(connectionInfo);

                        // Set as current connection if it matches the saved current connection name
                        if (!string.IsNullOrEmpty(currentConnectionName) && connectionInfo.Name == currentConnectionName)
                        {
                            currentConnection = connectionInfo;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading connections from file: " + ex.Message);
            }
        }

        private void SaveConnectionsToFile()
        {
            try
            {
                string filePath = GetConnectionsFilePath();
                var xml = new XmlDocument();
                var root = xml.CreateElement("Connections");
                xml.AppendChild(root);

                // Add current connection info
                if (currentConnection != null)
                {
                    var currentNode = xml.CreateElement("CurrentConnection");
                    currentNode.InnerText = currentConnection.Name ?? "";
                    root.AppendChild(currentNode);
                }

                foreach (var conn in saveConnections)
                {
                    var connectionNode = xml.CreateElement("Connection");
                    connectionNode.SetAttribute("name", conn.Name);

                    var serverNode = xml.CreateElement("ServerName");
                    serverNode.InnerText = EncryptString(conn.ServerName);
                    connectionNode.AppendChild(serverNode);

                    var databaseNode = xml.CreateElement("DatabaseName");
                    databaseNode.InnerText = conn.DatabaseName;
                    connectionNode.AppendChild(databaseNode);

                    var usernameNode = xml.CreateElement("Username");
                    usernameNode.InnerText = EncryptString(conn.Username);
                    connectionNode.AppendChild(usernameNode);

                    var passwordNode = xml.CreateElement("Password");
                    passwordNode.InnerText = EncryptString(conn.Password);
                    connectionNode.AppendChild(passwordNode);

                    var integratedNode = xml.CreateElement("UseIntegratedSecurity");
                    integratedNode.InnerText = conn.UseIntegratedSecurity.ToString();
                    connectionNode.AppendChild(integratedNode);

                    var timeoutNode = xml.CreateElement("ConnectionTimeout");
                    timeoutNode.InnerText = conn.ConnectionTimeOut.ToString();
                    connectionNode.AppendChild(timeoutNode);

                    root.AppendChild(connectionNode);
                }

                xml.Save(filePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error saving connections to file: " + ex.Message);
            }
        }

        private string GetConnectionsFilePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "MACP_POS");

            if (!Directory.Exists(appFolder))
                Directory.CreateDirectory(appFolder);

            return Path.Combine(appFolder, "connections.xml");
        }

        private void UpdateAppConfig(ConnectionInfo connectionInfo)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var connectionString = connectionInfo.ToConnectionString();

                if (config.ConnectionStrings.ConnectionStrings["DefaultConnection"] != null)
                {
                    config.ConnectionStrings.ConnectionStrings["DefaultConnection"].ConnectionString = connectionString;
                }
                else
                {
                    config.ConnectionStrings.ConnectionStrings.Add(
                        new ConnectionStringSettings("DefaultConnection", connectionString, "System.Data.SqlClient"));
                }

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("connectionStrings");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating app.config: " + ex.Message);
            }
        }

        // Simple encryption for stored passwords (not for production use)
        private string EncryptString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(text);
                byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encrypted);
            }
            catch
            {
                return text; // Return original if encryption fails
            }
        }

        private string DecryptString(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return "";

            try
            {
                byte[] data = Convert.FromBase64String(encryptedText);
                byte[] decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch
            {
                return encryptedText; // Return original if decryption fails
            }
        }
    }
}
