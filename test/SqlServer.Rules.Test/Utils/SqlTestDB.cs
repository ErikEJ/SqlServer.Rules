using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace SqlServer.Rules.Tests.Utils;

/// <summary>
/// TestDB manages a database that is used during unit testing.  It provides
/// services such as connection strings and attach/detach of the DB from
/// the test database server
/// </summary>
public sealed class SqlTestDB : IDisposable
{
    public enum ReallyCleanUpDatabase
    {
        NotIfItCameFromABackupFile,
        YesReally,
    }

    private readonly InstanceInfo instance;
    private readonly string dbName;

    // Variables for tracking restored DB information
    private readonly List<string> cleanupScripts;
    private bool cleanupDatabase;

    public event EventHandler<EventArgs> Disposing;

    public static SqlTestDB CreateFromDacpac(InstanceInfo instance, string dacpacPath, DacDeployOptions deployOptions = null, bool dropDatabaseOnCleanup = false)
    {
        var dbName = Path.GetFileNameWithoutExtension(dacpacPath);
        var ds = new DacServices(instance.BuildConnectionString(dbName));
        using (var dp = DacPackage.Load(dacpacPath, DacSchemaModelStorageType.Memory))
        {
            ds.Deploy(dp, dbName, true, deployOptions);
        }

        var sqlDb = new SqlTestDB(instance, dbName, dropDatabaseOnCleanup);
        return sqlDb;
    }

    public static SqlTestDB CreateFromBacpac(InstanceInfo instance, string bacpacPath, DacImportOptions importOptions = null, bool dropDatabaseOnCleanup = false)
    {
        var dbName = Path.GetFileNameWithoutExtension(bacpacPath);
        var ds = new DacServices(instance.BuildConnectionString(dbName));
        using (var bp = BacPackage.Load(bacpacPath, DacSchemaModelStorageType.Memory))
        {
            importOptions = FillDefaultImportOptionsForTest(importOptions);
            ds.ImportBacpac(bp, dbName, importOptions);
        }

        var sqlDb = new SqlTestDB(instance, dbName, dropDatabaseOnCleanup);
        return sqlDb;
    }

    public static bool TryCreateFromDacpac(InstanceInfo instance, string dacpacPath, out SqlTestDB db, out string error, DacDeployOptions deployOptions = null, bool dropDatabaseOnCleanup = false)
    {
        error = null;
        var dbName = string.Empty;
        try
        {
            dbName = Path.GetFileNameWithoutExtension(dacpacPath);
            db = CreateFromDacpac(instance, dacpacPath, deployOptions, dropDatabaseOnCleanup);
            return true;
        }
        catch (Exception ex)
        {
            error = ExceptionText.GetText(ex);
            db = null;

            var dbCreated = SafeDatabaseExists(instance, dbName);
            if (dbCreated)
            {
                db = new SqlTestDB(instance, dbName, dropDatabaseOnCleanup);
            }

            return false;
        }
    }

    public static bool TryCreateFromBacpac(InstanceInfo instance, string bacpacPath, out SqlTestDB db, out string error, DacImportOptions importOptions = null, bool dropDatabaseOnCleanup = false)
    {
        error = null;
        var dbName = string.Empty;
        try
        {
            dbName = Path.GetFileNameWithoutExtension(bacpacPath);
            importOptions = FillDefaultImportOptionsForTest(importOptions);
            db = CreateFromBacpac(instance, bacpacPath, importOptions, dropDatabaseOnCleanup);
            return true;
        }
        catch (Exception ex)
        {
            error = ExceptionText.GetText(ex);
            db = null;

            var dbCreated = SafeDatabaseExists(instance, dbName);
            if (dbCreated)
            {
                db = new SqlTestDB(instance, dbName, dropDatabaseOnCleanup);
            }

            return false;
        }
    }

    private static DacImportOptions FillDefaultImportOptionsForTest(DacImportOptions importOptions)
    {
        var result = new DacImportOptions();

        if (importOptions != null)
        {
            result.CommandTimeout = importOptions.CommandTimeout;
            result.ImportContributorArguments = importOptions.ImportContributorArguments;
            result.ImportContributors = importOptions.ImportContributors;
        }

        return result;
    }

    private static bool SafeDatabaseExists(InstanceInfo instance, string dbName)
    {
        try
        {
            using var masterDb = new SqlTestDB(instance, "master");
            using (var connection = masterDb.OpenSqlConnection())
            {
                using (var command = connection.CreateCommand())
                {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    command.CommandText = string.Format(CultureInfo.CurrentCulture, "select count(*) from sys.databases where [name]='{0}'", dbName);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    var result = command.ExecuteScalar();
                    int count;
                    return result != null && int.TryParse(result.ToString(), out count) && count > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return false;
        }
    }

    private SqlTestDB()
    {
        cleanupScripts = [];
    }

    public SqlTestDB(InstanceInfo instance, string dbName, bool dropDatabaseOnCleanup = false)
    {
        if (string.IsNullOrEmpty(dbName))
        {
            throw new ArgumentOutOfRangeException(nameof(dbName));
        }

        this.instance = instance ?? throw new ArgumentNullException(nameof(instance));
        this.dbName = dbName;

        cleanupDatabase = true;
    }

    /// <summary>
    /// Server name
    /// </summary>
    public string ServerName
    {
        get
        {
            return instance.DataSource;
        }
    }

    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName
    {
        get
        {
            return dbName;
        }
    }

    /// <summary>
    /// InstanceInfo
    /// </summary>
    public InstanceInfo Instance
    {
        get
        {
            return instance;
        }
    }

    public void Dispose()
    {
        Cleanup(ReallyCleanUpDatabase.NotIfItCameFromABackupFile);

        var h = Disposing;
        h?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Build a connection string that can be used to connect to the database
    /// </summary>
    /// <returns>
    /// A new connection string configured to use the current user's domain credentials to
    /// authenticate to the database
    /// </returns>
    public string BuildConnectionString()
    {
        return CreateBuilder().ConnectionString;
    }

    public SqlConnectionStringBuilder CreateBuilder()
    {
        return instance.CreateBuilder(dbName);
    }

    public string BuildConnectionString(string userName, string password)
    {
        return CreateBuilder(userName, password).ConnectionString;
    }

    public SqlConnectionStringBuilder CreateBuilder(string userName, string password)
    {
        return instance.CreateBuilder(userName, password, dbName);
    }

    /// <summary>
    /// Retrieve an open connection to the test database
    /// </summary>
    /// <returns>An open connection to the </returns>
    public DbConnection OpenConnection()
    {
        return OpenSqlConnection();
    }

    public SqlConnection OpenSqlConnection()
    {
        var conn = new SqlConnection(instance.BuildConnectionString(dbName));
        conn.Open();
        return conn;
    }

    public DbConnection OpenConnection(string userName, string password)
    {
        var conn = new SqlConnection(instance.BuildConnectionString(userName, password, dbName));
        conn.Open();
        return conn;
    }

    public void Execute(string script, int? timeout = null)
    {
        var batches = TestUtils.GetBatches(script);
        using (var connection = OpenSqlConnection())
        {
            foreach (var batch in batches)
            {
                Debug.WriteLine(batch);
                TestUtils.Execute(connection, batch, timeout);
            }
        }
    }

    public void SafeExecute(string script, int? timeout = null)
    {
        try
        {
            Execute(script, timeout);
        }
        catch (Exception ex)
        {
            var message = string.Format(
                CultureInfo.CurrentCulture,
                "Executing script on server '{0}' database '{1}' failed. Error: {2}.\r\n\r\nScript: {3}.)",
                Instance.DataSource,
                DatabaseName,
                ex.Message,
                script);
            Debug.WriteLine(message);
        }
    }

    public void ExtractDacpac(string filePath, IEnumerable<Tuple<string, string>> tables = null, DacExtractOptions extractOptions = null)
    {
        var ds = new DacServices(BuildConnectionString());
        ds.Extract(filePath, DatabaseName, DatabaseName, new Version(1, 0, 0), string.Empty, tables, extractOptions);
    }

    public bool TryExtractDacpac(string filePath, out string error, IEnumerable<Tuple<string, string>> tables = null, DacExtractOptions extractOptions = null)
    {
        error = null;
        try
        {
            ExtractDacpac(filePath, tables, extractOptions);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public void ExportBacpac(string filePath, IEnumerable<Tuple<string, string>> tables = null, DacExportOptions extractOptions = null)
    {
        var ds = new DacServices(BuildConnectionString());
        ds.ExportBacpac(filePath, DatabaseName, extractOptions, tables);
    }

    public bool TryExportBacpac(string filePath, out string error, IEnumerable<Tuple<string, string>> tables = null, DacExportOptions exportOptions = null)
    {
        error = null;
        try
        {
            ExportBacpac(filePath, tables, exportOptions);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Cleanup the DB if it was restored during the testing process.  A restoredDB will
    /// removed from the server and then .mdf/ldf files deleted from disk
    /// </summary>
    /// <param name="reallyCleanUpDatabase">ReallyCleanUpDatabase.NotIfItCameFromABackupFile: means to
    /// check whether the database came from a backup file or has previously been cleaned. If either
    /// of those two things is true, then the database is not cleaned up.
    ///
    /// ReallyCleanUpDatabase.YesReally: means to clean up the database regardless of its origin.
    /// </param>
    public void Cleanup(ReallyCleanUpDatabase reallyCleanUpDatabase = ReallyCleanUpDatabase.YesReally)
    {
        if (cleanupDatabase || reallyCleanUpDatabase == ReallyCleanUpDatabase.YesReally)
        {
            DoCleanup();
        }
    }

    private void DoCleanup()
    {
        if (cleanupScripts != null && cleanupScripts.Count > 0)
        {
            Log("Running cleanup scripts for DB {0}", dbName);
            using (var conn = new SqlConnection(instance.BuildConnectionString(dbName)))
            {
                conn.Open();
                foreach (var script in cleanupScripts)
                {
                    TestUtils.Execute(conn, script);
                }
            }
        }

        Log("Deleting DB {0}", dbName);
        try
        {
            TestUtils.DropDatabase(instance, dbName);
        }
        catch (Exception ex)
        {
            // We do not want a cleanup failure to block a test's execution result
            Log("Exception thrown during cleanup of DB " + dbName + " " + ex);
        }

        cleanupDatabase = false;
    }

    private static void Log(string format, params object[] args)
    {
        Trace.TraceInformation(
            "*** {0} TEST {1}",
            DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture),
            string.Format(CultureInfo.InvariantCulture, format, args));
    }

    internal void AddCleanupScript(string script)
    {
        cleanupScripts.Add(script);
    }
}