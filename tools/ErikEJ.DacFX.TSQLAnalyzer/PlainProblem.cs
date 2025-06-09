namespace ErikEJ.DacFX.TSQLAnalyzer;

public class PlainProblem
{
    public string Rule { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? SourceFile { get; set; } = null!;

    public int Line { get; set; }

    public int Column { get; set; }

    public string Severity { get; set; } = null!;
}
