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
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            foreach (var Statement in bEStatement.StatementList.Statements)
            {
                smells.ProcessTsqlFragment(Statement);
            }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
        }
    }
}