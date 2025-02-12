using Microsoft.Extensions.FileSystemGlobbing;

namespace SqlAnalyzerCli.Services
{
    public class GlobPatternMatcher
    {
        private readonly Matcher matcher = new Matcher();

        public IEnumerable<string> GetResultsInFullPath(string path)
        {
            return matcher.GetResultsInFullPath(path);
        }
    }
}
