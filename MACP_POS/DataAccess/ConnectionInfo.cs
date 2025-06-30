using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MACP_POS.DataAccess
{
    public class ConnectionInfo
    {
        public string Name { get; set; }
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseIntegratedSecurity { get; set; }
        public int ConnectionTimeOut { get; set; }

        public ConnectionInfo()
        {
            ConnectionTimeOut = 30; // Default timeout
        }

        public string ToConnectionString()
        {
            if (UseIntegratedSecurity)
            {
                return string.Format(
                    "Server={0};Database={1};Integrated Security=true;Connection Timeout={2};",
                    ServerName, DatabaseName, ConnectionTimeOut);
            }
            else
            {
                return string.Format(
                    "Server={0};Database={1};User Id={2};Password={3};Connection Timeout={4};",
                    ServerName, DatabaseName, Username, Password, ConnectionTimeOut);
            }
        }

        public static ConnectionInfo FromConnectionString(string connectionString, string name = "")
        {
            var info = new ConnectionInfo { Name = name };

            try
            {
                var parts = connectionString.Split(';');
                foreach (var part in parts)
                {
                    var keyValue = part.Split('=');
                    if (keyValue.Length == 2)
                    {
                        var key = keyValue[0].Trim().ToLower();
                        var value = keyValue[1].Trim();

                        switch (key)
                        {
                            case "server":
                            case "data source":
                                info.ServerName = value;
                                break;
                            case "database":
                            case "initial catalog":
                                info.DatabaseName = value;
                                break;
                            case "user id":
                            case "uid":
                                info.Username = value;
                                break;
                            case "password":
                            case "pwd":
                                info.Password = value;
                                break;
                            case "integrated security":
                                info.UseIntegratedSecurity = value.ToLower() == "true" || value.ToLower() == "sspi";
                                break;
                            case "connection timeout":
                                int timeout;
                                if (int.TryParse(value, out timeout))
                                    info.ConnectionTimeOut = timeout;
                                break;
                        }
                    }
                }
            }
            catch
            {
                // Return empty info if parsing fails
                return new ConnectionInfo { Name = name };
            }

            return info;
        }
    }
}
