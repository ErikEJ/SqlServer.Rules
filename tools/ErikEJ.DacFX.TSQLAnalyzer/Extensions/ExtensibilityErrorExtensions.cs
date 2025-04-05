using System.Text;
using Microsoft.SqlServer.Dac.Extensibility;

namespace ErikEJ.DacFX.TSQLAnalyzer.Extensions;

/// <summary>
/// A wrapper for <see cref="ExtensibilityError" /> that provides MSBuild compatible output and source document information.
/// </summary>
public static class ExtensibilityErrorExtensions
{
    public static string GetOutputMessage(this ExtensibilityError extensibilityError)
    {
        ArgumentNullException.ThrowIfNull(extensibilityError);

        var stringBuilder = new StringBuilder();
        stringBuilder.Append(extensibilityError.ErrorCode);
        stringBuilder.Append(": ");
        stringBuilder.Append(extensibilityError.Message);
        stringBuilder.Append(". ");
        stringBuilder.Append(extensibilityError.Exception?.ToString() ?? string.Empty);

        if (extensibilityError.ErrorCode == 72043)
        {
            stringBuilder.AppendLine();
            stringBuilder.Append("The provided model cannot be analyzed, if it is a live database, extract the schema and create a buildable .dacpac.");
        }

        return stringBuilder.ToString();
    }
}
