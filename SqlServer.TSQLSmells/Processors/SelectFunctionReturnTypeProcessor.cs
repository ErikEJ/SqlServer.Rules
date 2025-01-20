using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class SelectFunctionReturnTypeProcessor
    {
        private readonly Smells smells;

        public SelectFunctionReturnTypeProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessSelectFunctionReturnType(SelectFunctionReturnType ReturnType)
        {
            smells.ProcessTsqlFragment(ReturnType.SelectStatement);
        }
    }
}