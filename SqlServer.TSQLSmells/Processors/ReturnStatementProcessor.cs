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

        public void ProcessReturnStatement(ReturnStatement ReturnStatement)
        {
            if (ReturnStatement.Expression != null)
            {
                smells.ProcessTsqlFragment(ReturnStatement.Expression);
            }
        }
    }
}