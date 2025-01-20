using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class TableVariableProcessor
    {
        private readonly Smells smells;

        public TableVariableProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessTableVariableStatement(DeclareTableVariableStatement Fragment)
        {
            if (Fragment.Body.VariableName.Value.Length <= 2)
            {
                smells.SendFeedBack(33, Fragment);
            }
        }

        public void ProcessTableValuedFunctionReturnType(TableValuedFunctionReturnType Fragment)
        {
            smells.ProcessTsqlFragment(Fragment.DeclareTableVariableBody);
        }

        public void ProcessTableVariableBody(DeclareTableVariableBody Fragment)
        {
            if (Fragment.VariableName.Value.Length <= 2)
            {
                smells.SendFeedBack(33, Fragment);
            }
        }

        public void ProcessExistsPredicate(ExistsPredicate ExistsPredicate)
        {
            smells.ProcessTsqlFragment(ExistsPredicate.Subquery);
        }
    }
}