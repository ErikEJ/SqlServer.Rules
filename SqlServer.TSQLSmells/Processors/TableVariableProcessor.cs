using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class TableVariableProcessor
    {
        private readonly Smells _smells;

        public TableVariableProcessor(Smells smells)
        {
            _smells = smells;
        }

        public void ProcessTableVariableStatement(DeclareTableVariableStatement Fragment)
        {
            if (Fragment.Body.VariableName.Value.Length <= 2)
            {
                _smells.SendFeedBack(33, Fragment);
            }
        }

        public void ProcessTableValuedFunctionReturnType(TableValuedFunctionReturnType Fragment)
        {
            _smells.ProcessTsqlFragment(Fragment.DeclareTableVariableBody);
        }

        public void ProcessTableVariableBody(DeclareTableVariableBody Fragment)
        {
            if (Fragment.VariableName.Value.Length <= 2)
            {
                _smells.SendFeedBack(33, Fragment);
            }
        }

        public void ProcessExistsPredicate(ExistsPredicate ExistsPredicate)
        {
            _smells.ProcessTsqlFragment(ExistsPredicate.Subquery);
        }
    }
}