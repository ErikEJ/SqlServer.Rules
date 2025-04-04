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

        return stringBuilder.ToString();
    }
}
