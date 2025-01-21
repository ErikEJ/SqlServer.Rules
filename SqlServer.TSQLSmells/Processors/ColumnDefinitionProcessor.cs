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

        public void ProcessColumnDefinition(ColumnDefinition columnDef)
        {
            smells.ProcessTsqlFragment(columnDef.DataType);
            foreach (var constraint in columnDef.Constraints)
            {
                smells.ProcessTsqlFragment(constraint);
            }
        }
    }
}