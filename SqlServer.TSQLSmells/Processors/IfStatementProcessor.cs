using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class IfStatementProcessor
    {
        private readonly Smells smells;

        public IfStatementProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessIfStatement(IfStatement ifStatement)
        {
            smells.ProcessTsqlFragment(ifStatement.Predicate);
            smells.ProcessTsqlFragment(ifStatement.ThenStatement);
            if (ifStatement.ElseStatement != null)
            {
                smells.ProcessTsqlFragment(ifStatement.ElseStatement);
            }
        }
    }
}