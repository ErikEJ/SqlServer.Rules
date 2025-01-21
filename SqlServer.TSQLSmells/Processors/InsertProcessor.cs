using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class InsertProcessor
    {
        private readonly Smells smells;

        public InsertProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessWithCtesAndXmlNamespaces(WithCtesAndXmlNamespaces cte)
        {
            foreach (var expression in cte.CommonTableExpressions)
            {
                smells.ProcessQueryExpression(expression.QueryExpression, "RG", false, cte);
            }
        }

        public void Process(InsertStatement fragment)
        {
            if (fragment.InsertSpecification.Columns.Count == 0)
            {
                smells.SendFeedBack(12, fragment);
            }

            switch (FragmentTypeParser.GetFragmentType(fragment.InsertSpecification.InsertSource))
            {
                case "SelectInsertSource":
                    var insSource = (SelectInsertSource)fragment.InsertSpecification.InsertSource;
                    var cte = fragment.WithCtesAndXmlNamespaces;
                    smells.ProcessQueryExpression(insSource.Select, "RG", false, cte);
                    if (cte != null)
                    {
                        ProcessWithCtesAndXmlNamespaces(cte);
                    }

                    break;
                case "ExecuteInsertSource":
                    var execSource = (ExecuteInsertSource)fragment.InsertSpecification.InsertSource;

                    // ProcessExecuteSpecification(ExecSource.Execute);
                    var executableEntity = execSource.Execute.ExecutableEntity;
                    smells.ExecutableEntityProcessor.ProcessExecutableEntity(executableEntity);
                    break;
            }
        }
    }
}