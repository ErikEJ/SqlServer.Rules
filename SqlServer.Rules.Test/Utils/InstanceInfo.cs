using System;
using System.Globalization;
using Microsoft.Data.SqlClient;

namespace SqlServer.Rules.Tests.Utils;

public class InstanceInfo
{
    public InstanceInfo(string dataSource)
    {
        DataSource = dataSource;
    }

    // Persisted data properties
    public string DataSource { get; set; }

    public string RemoteSharePath { get; set; }

    public int ConnectTimeout { get; set; }

    public string ConnectTimeoutAsString
    {
        get
        {
            return ConnectTimeout.ToString(CultureInfo.InvariantCulture);
        }

        set
        {
            int temp;
            if (int.TryParse(value, out temp))
            {
                ConnectTimeout = temp;
            }
            else
            {
                ConnectTimeout = 15;
            }
        }
    }

    public string MachineName
    {
        get
        {
            var serverName = DataSource;
            var index = DataSource.IndexOf('\\', StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                serverName = DataSource.Substring(0, index);
            }

            if (StringComparer.OrdinalIgnoreCase.Compare("(local)", serverName) == 0
                || StringComparer.OrdinalIgnoreCase.Compare(".", serverName) == 0)
            {
                serverName = Environment.MachineName;
            }

            return serverName;
        }
    }

    public string InstanceName
    {
        get
        {
            string name = null;
            var index = DataSource.IndexOf('\\', StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                name = DataSource.Substring(index + 1);
            }

            return name;
        }
    }

    public string UserId { get; set; }
    public string Password { get; set; }

    /// <summary>
    /// Connection string to this instance with the master database as the default.
    /// Integrated security is used
    /// </summary>
    /// <returns></returns>
    public string BuildConnectionString()
    {
        return CreateBuilder().ConnectionString;
    }

    public SqlConnectionStringBuilder CreateBuilder()
    {
        return CreateBuilder(CommonConstants.MasterDatabaseName);
    }

    public string BuildConnectionString(string dbName)
    {
        return CreateBuilder(dbName).ConnectionString;
    }

    public SqlConnectionStringBuilder CreateBuilder(string dbName)
    {
        return CreateBuilder(UserId, Password, dbName);
    }

    /// <summary>
    /// Build a connection string for this instance using the specified
    /// username/password for security and specifying the dbName as the
    /// initial catalog
    /// </summary>
    public string BuildConnectionString(string userId, string password, string dbName)
    {
        var scsb = CreateBuilder(userId, password, dbName);
        return scsb.ConnectionString;
    }

    public SqlConnectionStringBuilder CreateBuilder(string userId, string password, string dbName)
    {
        var scsb = new SqlConnectionStringBuilder
        {
            DataSource = DataSource,
            InitialCatalog = dbName,
            Pooling = false,
            MultipleActiveResultSets = false,
        };
        if (ConnectTimeout != 15)
        {
            scsb.ConnectTimeout = ConnectTimeout;
        }

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
        {
            scsb.IntegratedSecurity = true;
        }
        else
        {
            scsb.IntegratedSecurity = false;
            scsb.UserID = userId;
            scsb.Password = password;
        }

        return scsb;
    }
}