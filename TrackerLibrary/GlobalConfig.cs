using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using TrackerLibrary.DataAccess;

namespace TrackerLibrary
{
    public static class GlobalConfig
    {
        public static IDataConnection Connection { get; private set; }

        public static void InitializeConnections (DatabaseType dbType)
        {
            if (dbType == DatabaseType.Sql)
            {
                // TODO - setup SQL connector properly
                Connection = new SqlConnector();
            }
            else if (dbType == DatabaseType.TextFile)
            {
                // TODO - setup text connector properly
                Connection = new TextConnector();
                
            }
        }

        public static string GetConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }
    }
}
