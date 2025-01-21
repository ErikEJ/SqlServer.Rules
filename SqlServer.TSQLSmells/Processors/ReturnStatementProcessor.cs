using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class ReturnStatementProcessor
    {
        private readonly Smells smells;

        public ReturnStatementProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessReturnStatement(ReturnStatement returnStatement)
        {
            if (returnStatement.Expression != null)
            {
                smells.ProcessTsqlFragment(returnStatement.Expression);
            }
        }
    }
}