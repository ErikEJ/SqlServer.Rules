using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class BeginEndBlockProcessor
    {
        private readonly Smells _smells;

        public BeginEndBlockProcessor(Smells smells)
        {
            _smells = smells;
        }

        public void ProcessBeginEndBlockStatement(BeginEndBlockStatement BEStatement)
        {
            foreach (TSqlStatement Statement in BEStatement.StatementList.Statements)
            {
                _smells.ProcessTsqlFragment(Statement);
            }
        }
    }
}