using System.Globalization;
using Spectre.Console;

namespace SqlAnalyzerCli.Services;

internal static class DisplayService
{
    public static string Link(string link)
    {
        return $"[link]{link}[/]";
    }

    public static void Title(string message)
    {
        AnsiConsole.Write(
            new FigletText(message)
                .Centered()
                .Color(Color.Aqua));
    }

    public static void MarkupLine(string message, Color color, Func<string, string>? format)
    {
        AnsiConsole.MarkupLine($"[{color}]{format?.Invoke(message) ?? message}[/]");
    }

    public static void MarkupLine(string message, Color color)
    {
        MarkupLine(message, color, null);
    }

    public static void MarkupLine(params Func<string>[] messages)
    {
        if (messages?.Length < 1)
        {
            AnsiConsole.WriteLine();
            return;
        }

        AnsiConsole.MarkupLine(string.Join(' ', messages!.Select(func => func())));
    }

    public static void Error(string message)
    {
        AnsiConsole.MarkupLineInterpolated(CultureInfo.InvariantCulture, $"[red]error: {message}[/]");
    }
}