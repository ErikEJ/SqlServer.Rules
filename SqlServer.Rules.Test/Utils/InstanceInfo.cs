//------------------------------------------------------------------------------
// <copyright company="Microsoft">
//
//    The MIT License (MIT)
//
//    Copyright (c) 2015 Microsoft
//
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the "Software"), to deal
//    in the Software without restriction, including without limitation the rights
//    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//    copies of the Software, and to permit persons to whom the Software is
//    furnished to do so, subject to the following conditions:
//
//    The above copyright notice and this permission notice shall be included in all
//    copies or substantial portions of the Software.
//
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//    SOFTWARE.
// </copyright>
//------------------------------------------------------------------------------

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
        get { return ConnectTimeout.ToString(CultureInfo.InvariantCulture); }
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
        var scsb = new SqlConnectionStringBuilder {
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