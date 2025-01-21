using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class FunctionStatementBodyProcessor
    {
        private readonly Smells smells;

        public FunctionStatementBodyProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessFunctionStatementBody(FunctionStatementBody function)
        {
            if (function.Name.SchemaIdentifier == null)
            {
                smells.SendFeedBack(24, function.Name);
            }

            smells.ProcessTsqlFragment(function.ReturnType);

            if (function.StatementList != null)
            {
                foreach (TSqlFragment statement in function.StatementList.Statements)
                {
                    smells.ProcessTsqlFragment(statement);
                }
            }
        }
    }
}