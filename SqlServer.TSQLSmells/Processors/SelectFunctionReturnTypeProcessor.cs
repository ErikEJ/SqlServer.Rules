using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class SelectFunctionReturnTypeProcessor
    {
        private readonly Smells _smells;

        public SelectFunctionReturnTypeProcessor(Smells smells)
        {
            _smells = smells;
        }

        public void ProcessSelectFunctionReturnType(SelectFunctionReturnType ReturnType)
        {
            _smells.ProcessTsqlFragment(ReturnType.SelectStatement);
        }
    }
}