using System.Linq;

namespace SqlServer.Rules.DocsGenerator;

public static class DocsExtensions
{
    public static string ToSentence(this string input)
    {
        var parts = System.Text.RegularExpressions.Regex.Split(input, @"([A-Z]?[a-z]+)").Where(str => !string.IsNullOrEmpty(str));
        return string.Join(' ', parts);
    }

    public static string ToId(this string input)
    {
        return new string(input.Split('.').Last());
    }
}
