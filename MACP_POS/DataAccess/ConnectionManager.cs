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
    }
}
