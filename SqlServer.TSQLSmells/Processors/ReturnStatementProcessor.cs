using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class ReturnStatementProcessor
    {
        private readonly Smells _smells;

        public ReturnStatementProcessor(Smells smells)
        {
            _smells = smells;
        }

        public void ProcessReturnStatement(ReturnStatement ReturnStatement)
        {
            if (ReturnStatement.Expression != null) _smells.ProcessTsqlFragment(ReturnStatement.Expression);
        }
    }
}