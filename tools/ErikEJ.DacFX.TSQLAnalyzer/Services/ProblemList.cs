namespace ErikEJ.DacFX.TSQLAnalyzer.Services;

public class ProblemList
{
#pragma warning disable CA2227 // Collection properties should be read only
    public IList<PlainProblem> Problems { get; set; } = new List<PlainProblem>();
#pragma warning restore CA2227 // Collection properties should be read only
}
