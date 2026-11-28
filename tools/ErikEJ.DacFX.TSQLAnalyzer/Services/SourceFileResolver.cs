namespace ErikEJ.DacFX.TSQLAnalyzer.Services;

/// <summary>
/// Tells problem sources that are files on disk apart from database object names and in-memory scripts.
/// </summary>
internal sealed class SourceFileResolver
{
    private readonly Dictionary<string, string?> paths = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the full path of a problem source.
    /// </summary>
    /// <param name="sourceName">The source name reported by the analysis engine.</param>
    /// <returns>The full path, or <see langword="null" /> when the source is not a file on disk.</returns>
    public string? GetFilePath(string? sourceName)
    {
        // DacFx reports "-1" for objects added without a named source.
        if (string.IsNullOrEmpty(sourceName) || sourceName == "-1")
        {
            return null;
        }

        if (!paths.TryGetValue(sourceName, out var path))
        {
            path = File.Exists(sourceName) ? Path.GetFullPath(sourceName) : null;
            paths[sourceName] = path;
        }

        return path;
    }
}
