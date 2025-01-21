using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class BeginEndBlockProcessor
    {
        private readonly Smells smells;

        public BeginEndBlockProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessBeginEndBlockStatement(BeginEndBlockStatement bEStatement)
        {
            foreach (var statement in bEStatement.StatementList.Statements)
            {
                smells.ProcessTsqlFragment(statement);
            }
        }
    }
}