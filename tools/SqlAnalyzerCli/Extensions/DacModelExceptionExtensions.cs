using System.Text;
using Microsoft.SqlServer.Dac.Model;

namespace SqlAnalyzerCli.Extensions;

internal static class DacModelExceptionExtensions
{
    public static string Format(this DacModelException exception, string fileName)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var stringBuilder = new StringBuilder();

        foreach (var modelError in exception.Messages)
        {
            stringBuilder.Append(fileName);
            stringBuilder.Append('(');
            stringBuilder.Append('1');
            stringBuilder.Append(',');
            stringBuilder.Append('1');
            stringBuilder.Append("):");
            stringBuilder.Append(' ');
            stringBuilder.Append("Error");
            stringBuilder.Append(' ');
            stringBuilder.Append(modelError.Prefix);
            stringBuilder.Append(modelError.Number);
            stringBuilder.Append(": ");
            stringBuilder.Append(modelError.Message);
        }

        return stringBuilder.ToString();
    }

}
