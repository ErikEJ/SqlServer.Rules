using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class ColumnDefinitionProcessor
    {
        private readonly Smells smells;

        public ColumnDefinitionProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessColumnDefinition(ColumnDefinition ColumnDef)
        {
            smells.ProcessTsqlFragment(ColumnDef.DataType);
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            foreach (var Constraint in ColumnDef.Constraints)
            {
                smells.ProcessTsqlFragment(Constraint);
            }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
        }
    }
}