using System.Collections.Concurrent;
using Microsoft.Extensions.FileSystemGlobbing;

namespace ErikEJ.DacFX.TSQLAnalyzer.Services
{
    internal sealed class SqlFileCollector
    {
        private readonly ConcurrentDictionary<string, string> files = new();

        private readonly GlobPatternMatcher globPatternMatcher;

        public SqlFileCollector()
        {
            var matcher = new Matcher();
            matcher.AddInclude("**/*.sql");
            globPatternMatcher = new GlobPatternMatcher(matcher);
        }

        public Dictionary<string, string> ProcessList(IList<string> filePaths)
        {
            Parallel.ForEach(filePaths, (filePath) =>
            {
                if (!File.Exists(filePath))
                {
                    if (Directory.Exists(filePath))
                    {
                        ProcessDirectory(filePath);
                    }
                    else
                    {
                        ProcessWildCard(filePath);
                    }
                }
                else
                {
                    ProcessIfSqlFile(filePath);
                }
            });

            return files.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private void ProcessFile(string filePath)
        {
            var fileStream = GetFileContents(filePath);
            AddToProcessing(filePath, fileStream);
        }

        private void AddToProcessing(string filePath, string fileContents)
        {
            files.TryAdd(filePath, fileContents);
        }

        private void ProcessDirectory(string path)
        {
            Parallel.ForEach(globPatternMatcher.GetResultsInFullPath(path), (file) =>
            {
                ProcessIfSqlFile(file);
            });
        }

        private void ProcessIfSqlFile(string fileName)
        {
            if (Path.GetExtension(fileName).Equals(".sql", StringComparison.OrdinalIgnoreCase))
            {
                ProcessFile(fileName);
            }
        }

        private void ProcessWildCard(string filePath)
        {
            var containsWildCard = filePath.Contains('*', StringComparison.OrdinalIgnoreCase)
                || filePath.Contains('?', StringComparison.OrdinalIgnoreCase);
            if (!containsWildCard)
            {
                ////DisplayService.Error($"{filePath} is not a valid file path.");
                return;
            }

            var dirPath = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(dirPath))
            {
                dirPath = Directory.GetCurrentDirectory();
            }

            if (!Directory.Exists(dirPath))
            {
                ////DisplayService.Error($"Directory does not exist: {dirPath}");
                return;
            }

            var searchPattern = Path.GetFileName(filePath);
            var files = Directory.EnumerateFiles(dirPath, searchPattern, SearchOption.TopDirectoryOnly);
            Parallel.ForEach(files, (file) =>
            {
                ProcessIfSqlFile(file);
            });
        }

        private string GetFileContents(string filePath)
        {
            return File.ReadAllText(filePath);
        }
    }
}
