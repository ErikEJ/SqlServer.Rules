using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class InsertProcessor
    {
        private readonly Smells _smells;

        public InsertProcessor(Smells smells)
        {
            _smells = smells;
        }

        public void ProcessWithCtesAndXmlNamespaces(WithCtesAndXmlNamespaces Cte)
        {
            foreach (var Expression in Cte.CommonTableExpressions)
            {
                _smells.ProcessQueryExpression(Expression.QueryExpression, "RG", false, Cte);
            }
        }

        public void Process(InsertStatement Fragment)
        {
            if (Fragment.InsertSpecification.Columns.Count == 0)
            {
                _smells.SendFeedBack(12, Fragment);
            }

            switch (FragmentTypeParser.GetFragmentType(Fragment.InsertSpecification.InsertSource))
            {
                case "SelectInsertSource":
                    var InsSource = (SelectInsertSource) Fragment.InsertSpecification.InsertSource;
                    var Cte = Fragment.WithCtesAndXmlNamespaces;
                    _smells.ProcessQueryExpression(InsSource.Select, "RG", false, Cte);
                    if (Cte != null)
                    {
                        ProcessWithCtesAndXmlNamespaces(Cte);
                    }

                    break;
                case "ExecuteInsertSource":
                    var ExecSource = (ExecuteInsertSource) Fragment.InsertSpecification.InsertSource;

                    // ProcessExecuteSpecification(ExecSource.Execute);
                    var ExecutableEntity = ExecSource.Execute.ExecutableEntity;
                    _smells.ExecutableEntityProcessor.ProcessExecutableEntity(ExecutableEntity);
                    break;
            }
        }
    }
}