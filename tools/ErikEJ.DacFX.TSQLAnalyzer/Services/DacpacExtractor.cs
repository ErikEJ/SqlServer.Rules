using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace ErikEJ.DacFX.TSQLAnalyzer.Services
{
    internal sealed class DacpacExtractor
    {
        private readonly SqlConnectionStringBuilder connectionStringBuilder;

        public DacpacExtractor(SqlConnectionStringBuilder connectionStringBuilder)
        {
            ArgumentNullException.ThrowIfNull(connectionStringBuilder);
            this.connectionStringBuilder = connectionStringBuilder;
        }

        public FileInfo ExtractDacpac()
        {
            var extractedPackagePath = Path.Join(Path.GetTempPath(),  CleanDacpacName(connectionStringBuilder.InitialCatalog) + ".dacpac");

            var services = new DacServices(connectionStringBuilder.ConnectionString);

            var extractOptions = new DacExtractOptions
            {
                CommandTimeout = 300,
                VerifyExtraction = true,
                IgnorePermissions = true,
                IgnoreUserLoginMappings = true,
                IgnoreExtendedProperties = true,
                Storage = DacSchemaModelStorageType.Memory,
            };

            services.Extract(extractedPackagePath, connectionStringBuilder.InitialCatalog, "TSQLAnalyzer", new Version(1, 0), extractOptions: extractOptions);

            return new FileInfo(extractedPackagePath);
        }

        private static string CleanDacpacName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }

            return fileName;
        }
    }
}
