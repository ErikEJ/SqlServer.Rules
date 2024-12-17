using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class FunctionStatementBodyProcessor
    {
        private readonly Smells _smells;

        public FunctionStatementBodyProcessor(Smells smells)
        {
            _smells = smells;
        }

        public void ProcessFunctionStatementBody(FunctionStatementBody Function)
        {
            if (Function.Name.SchemaIdentifier == null) _smells.SendFeedBack(24, Function.Name);

            _smells.ProcessTsqlFragment(Function.ReturnType);

            if (Function.StatementList != null)
            {
                foreach (TSqlFragment Statement in Function.StatementList.Statements)
                {
                    _smells.ProcessTsqlFragment(Statement);
                }
            }
        }
    }
}