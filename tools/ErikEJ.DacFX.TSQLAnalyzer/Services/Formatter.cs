using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace ErikEJ.DacFX.TSQLAnalyzer.Services;

internal sealed class Formatter
{
    public static (bool Completed, string? FormattedText) Format(string text, string scriptPath)
    {
        if (string.IsNullOrWhiteSpace(text) || !TryParse(text, out TSqlFragment? fragment))
        {
            return (false, null);
        }

        var options = FormatterConfig.GetOptions(scriptPath);
        Sql170ScriptGenerator generator = new(options);

        generator.GenerateScript(fragment, out var formattedSql);

        if (text.Trim() != formattedSql.Trim())
        {
            return (true, formattedSql.Trim());
        }

        return (false, null);
    }

    private static bool TryParse(string text, out TSqlFragment? fragment)
    {
        try
        {
            // Use newest parser and all engine types to support all SQL Server versions.
            TSql170Parser parser = new(true, SqlEngineType.All);

            using (var reader = new StringReader(text))
            {
                fragment = parser.Parse(reader, out IList<ParseError> errors);
                return !errors.Any();
            }
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            // Handle parsing errors
            fragment = null;
            return false;
        }
    }
}
