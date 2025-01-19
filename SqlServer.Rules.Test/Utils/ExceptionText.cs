using System;
using System.Text;

namespace SqlServer.Rules.Tests.Utils;

public static class ExceptionText
{
    public static string GetText(Exception ex, bool stackTrace = false)
    {
        var sb = new StringBuilder();
        var depth = 0;
        while (ex != null)
        {
            if (depth > 0)
            {
                sb.Append("Inner Exception: ");
            }

            sb.AppendLine(ex.Message);
            if (stackTrace)
            {
                sb.AppendLine(ex.StackTrace);
            }

            ex = ex.InnerException;
            ++depth;
        }

        return sb.ToString();
    }
}