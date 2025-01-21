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

        public void ProcessTableVariableStatement(DeclareTableVariableStatement fragment)
        {
            if (fragment.Body.VariableName.Value.Length <= 2)
            {
                smells.SendFeedBack(33, fragment);
            }
        }

        public void ProcessTableValuedFunctionReturnType(TableValuedFunctionReturnType fragment)
        {
            smells.ProcessTsqlFragment(fragment.DeclareTableVariableBody);
        }

        public void ProcessTableVariableBody(DeclareTableVariableBody fragment)
        {
            if (fragment.VariableName.Value.Length <= 2)
            {
                smells.SendFeedBack(33, fragment);
            }
        }

        public void ProcessExistsPredicate(ExistsPredicate existsPredicate)
        {
            smells.ProcessTsqlFragment(existsPredicate.Subquery);
        }
    }
}