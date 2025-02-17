using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class FunctionProcessor
    {
        private readonly Smells smells;

        public FunctionProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessFunctionCall(FunctionCall functionCall)
        {
            if (functionCall.OverClause != null)
            {
                if (functionCall.OverClause.WindowFrameClause != null)
                {
                    if (functionCall.OverClause.WindowFrameClause.WindowFrameType == WindowFrameType.Range)
                    {
                        smells.SendFeedBack(25, functionCall.OverClause.WindowFrameClause);
                    }
                }
                else
                {
                    if (functionCall.OverClause.OrderByClause != null)
                    {
                        switch (functionCall.FunctionName.Value.ToUpperInvariant())
                        {
                            case "ROW_NUMBER":
                            case "RANK":
                            case "DENSE_RANK":
                            case "NTILE":
                            case "LAG":
                            case "LEAD":
                                break;
                            default:
                                smells.SendFeedBack(26, functionCall.OverClause);
                                break;
                        }
                    }
                }
            }
        }
    }
}