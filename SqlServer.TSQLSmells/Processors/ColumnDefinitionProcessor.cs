using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class ColumnDefinitionProcessor
    {
        private readonly Smells _smells;

        public ColumnDefinitionProcessor(Smells smells)
        {
            _smells = smells;
        }

        public void ProcessColumnDefinition(ColumnDefinition ColumnDef)
        {
            _smells.ProcessTsqlFragment(ColumnDef.DataType);
            foreach (var Constraint in ColumnDef.Constraints)
            {

                _smells.ProcessTsqlFragment(Constraint);
            }
        }
    }
}