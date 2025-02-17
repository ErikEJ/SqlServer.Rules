using System;

namespace SqlServer.Rules.Tests.Utils;

internal sealed class ArgumentValidation
{
    public static void CheckForEmptyString(string arg, string argName)
    {
        if (string.IsNullOrEmpty(arg))
        {
            throw new ArgumentException(argName);
        }
    }
}