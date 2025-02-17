using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class WhileProcessor
    {
        private readonly Smells smells;

        public WhileProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessWhileStatement(WhileStatement whileStatement)
        {
            smells.ProcessTsqlFragment(whileStatement.Predicate);
            smells.ProcessTsqlFragment(whileStatement.Statement);
        }
    }
}