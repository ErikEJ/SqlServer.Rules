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

        public void ProcessFunctionStatementBody(FunctionStatementBody Function)
        {
            if (Function.Name.SchemaIdentifier == null)
            {
                smells.SendFeedBack(24, Function.Name);
            }

            smells.ProcessTsqlFragment(Function.ReturnType);

            if (Function.StatementList != null)
            {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                foreach (TSqlFragment Statement in Function.StatementList.Statements)
                {
                    smells.ProcessTsqlFragment(Statement);
                }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            }
        }
    }
}