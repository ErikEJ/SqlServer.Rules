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
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace SqlServer.Rules.Tests.Utils
{
    /// <summary>
    /// TestDB manages a database that is used during unit testing.  It provides
    /// services such as connection strings and attach/detach of the DB from
    /// the test database server
    /// </summary>
    public class SqlTestDB : IDisposable
    {
        public enum ReallyCleanUpDatabase
        {
            NotIfItCameFromABackupFile, YesReally,
        }

        private readonly InstanceInfo _instance;
        private readonly string _dbName;

        // Variables for tracking restored DB information
        private bool _cleanupDatabase;
        private readonly List<string> _cleanupScripts;
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
                var masterDb = new SqlTestDB(instance, "master");
                using (var connection = masterDb.OpenSqlConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = string.Format(CultureInfo.CurrentCulture, "select count(*) from sys.databases where [name]='{0}'", dbName);
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
            _cleanupScripts = [];
        }

        /// <summary>
        /// Represents a test Database that was created for tests.  The DB has already been attached/created,
        /// and will not be removed unless dropDatabaseOnCleanup is true.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="dropDatabaseOnCleanup">If true the db instance will be dropped when the Cleanup method is called</param>
        /// <param name="dbName"></param>
        public SqlTestDB(InstanceInfo instance, string dbName, bool dropDatabaseOnCleanup = false)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentOutOfRangeException(nameof(dbName));
            }

            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _dbName = dbName;

            _cleanupDatabase = true;
        }

        /// <summary>
        /// Server name
        /// </summary>
        public string ServerName
        {
            get
            {
                return _instance.DataSource;
            }
        }

        /// <summary>
        /// Database name
        /// </summary>
        public string DatabaseName
        {
            get
            {
                return _dbName;
            }
        }

        /// <summary>
        /// InstanceInfo
        /// </summary>
        public InstanceInfo Instance
        {
            get
            {
                return _instance;
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
            return _instance.CreateBuilder(_dbName);
        }

        public string BuildConnectionString(string userName, string password)
        {
            return CreateBuilder(userName, password).ConnectionString;
        }

        public SqlConnectionStringBuilder CreateBuilder(string userName, string password)
        {
            return _instance.CreateBuilder(userName, password, _dbName);
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
            var conn = new SqlConnection(_instance.BuildConnectionString(_dbName));
            conn.Open();
            return conn;
        }

        public DbConnection OpenConnection(string userName, string password)
        {
            var conn = new SqlConnection(_instance.BuildConnectionString(userName, password, _dbName));
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
                var message = string.Format(CultureInfo.CurrentCulture, "Executing script on server '{0}' database '{1}' failed. Error: {2}.\r\n\r\nScript: {3}.)",
                    Instance.DataSource, DatabaseName, ex.Message, script);
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
            if (_cleanupDatabase || reallyCleanUpDatabase == ReallyCleanUpDatabase.YesReally)
            {
                DoCleanup();
            }
        }

        private void DoCleanup()
        {
            if (_cleanupScripts != null && _cleanupScripts.Count > 0)
            {
                Log("Running cleanup scripts for DB {0}", _dbName);
                using (var conn = new SqlConnection(_instance.BuildConnectionString(_dbName)))
                {
                    conn.Open();
                    foreach (var script in _cleanupScripts)
                    {
                        TestUtils.Execute(conn, script);
                    }
                }
            }

            Log("Deleting DB {0}", _dbName);
            try
            {
                TestUtils.DropDatabase(_instance, _dbName);
            }
            catch (Exception ex)
            {
                // We do not want a cleanup failure to block a test's execution result
                Log("Exception thrown during cleanup of DB " + _dbName + " " + ex);
            }

            _cleanupDatabase = false;
        }

        private static void Log(string format, params object[] args)
        {
            Trace.TraceInformation("*** {0} TEST {1}",
                DateTime.Now.ToString("HH:mm:ss.fff"), string.Format(format, args));
        }

        internal void AddCleanupScript(string script)
        {
            _cleanupScripts.Add(script);
        }
    }
}