using System;

namespace TSQLSmellsSSDTTest;

public class TestProblem
{
    public int StartColumn { get; set; }
    public int StartLine { get; set; }
    public string RuleId { get; set; }

    public TestProblem(int startLine, int startColumn, string ruleId)
    {
        StartColumn = startColumn;
        StartLine = startLine;
        RuleId = ruleId;
    }

    public override bool Equals(Object obj)
    {
        TestProblem prb = obj as TestProblem;
        if (prb.RuleId.Equals(RuleId, StringComparison.OrdinalIgnoreCase) &&
            prb.StartColumn == StartColumn &&
            prb.StartLine == StartLine)
        {
            return true;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return string.Format("{0}:{1}:{2}", RuleId, StartColumn, StartLine).GetHashCode();
    }
}
