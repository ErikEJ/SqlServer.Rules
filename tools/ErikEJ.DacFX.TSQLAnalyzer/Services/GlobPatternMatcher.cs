using Microsoft.Extensions.FileSystemGlobbing;

namespace ErikEJ.DacFX.TSQLAnalyzer.Services
{
    internal sealed class GlobPatternMatcher
    {
        private readonly Matcher matcher = new Matcher();

        public GlobPatternMatcher(Matcher matcher)
        {
            this.matcher = matcher;
        }

        public IEnumerable<string> GetResultsInFullPath(string path)
        {
            return matcher.GetResultsInFullPath(path);
        }
    }
}
