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

        public void ProcessWithCtesAndXmlNamespaces(WithCtesAndXmlNamespaces Cte)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            foreach (var Expression in Cte.CommonTableExpressions)
            {
                smells.ProcessQueryExpression(Expression.QueryExpression, "RG", false, Cte);
            }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
        }

        public void Process(InsertStatement Fragment)
        {
            if (Fragment.InsertSpecification.Columns.Count == 0)
            {
                smells.SendFeedBack(12, Fragment);
            }

            switch (FragmentTypeParser.GetFragmentType(Fragment.InsertSpecification.InsertSource))
            {
                case "SelectInsertSource":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var InsSource = (SelectInsertSource)Fragment.InsertSpecification.InsertSource;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var Cte = Fragment.WithCtesAndXmlNamespaces;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    smells.ProcessQueryExpression(InsSource.Select, "RG", false, Cte);
                    if (Cte != null)
                    {
                        ProcessWithCtesAndXmlNamespaces(Cte);
                    }

                    break;
                case "ExecuteInsertSource":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var ExecSource = (ExecuteInsertSource)Fragment.InsertSpecification.InsertSource;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter

                    // ProcessExecuteSpecification(ExecSource.Execute);
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var ExecutableEntity = ExecSource.Execute.ExecutableEntity;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    smells.ExecutableEntityProcessor.ProcessExecutableEntity(ExecutableEntity);
                    break;
            }
        }
    }
}