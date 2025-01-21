using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class SelectStatementProcessor
    {
        private readonly Smells smells;

        public SelectStatementProcessor(Smells smells)
        {
            this.smells = smells;
        }

        private void ProcessOptimizerHints(IList<OptimizerHint> optimizerHints, SelectStatement selStatement)
        {
            /* OptimizerHints is not a decendant of TSQLFragment */

            foreach (var hint in optimizerHints)
            {
                ProcessHint(hint, selStatement);
            }
        }

        public void Process(
            SelectStatement selStatement,
            string parentType,
            bool testTop = false,
            WithCtesAndXmlNamespaces cte = null)
        {
            if (cte == null && selStatement.WithCtesAndXmlNamespaces != null)
            {
                cte = selStatement.WithCtesAndXmlNamespaces;
                if (cte != null)
                {
                    smells.InsertProcessor.ProcessWithCtesAndXmlNamespaces(cte);
                }
            }

            smells.ProcessQueryExpression(selStatement.QueryExpression, parentType, false, cte);
            ProcessOptimizerHints(selStatement.OptimizerHints, selStatement);
        }

        private void ProcessSelectElement(SelectElement selectElement, string parentType, WithCtesAndXmlNamespaces cte)
        {
            var elemType = FragmentTypeParser.GetFragmentType(selectElement);

            switch (elemType)
            {
                case "SelectStarExpression":
                    smells.SendFeedBack(5, selectElement);
                    break;
                case "SelectScalarExpression":

                    var scalarExpression = (SelectScalarExpression)selectElement;

                    var expressionType = FragmentTypeParser.GetFragmentType(scalarExpression.Expression);

                    switch (expressionType)
                    {
                        case "ScalarSubquery":

                            var subQuery = (ScalarSubquery)scalarExpression.Expression;

                            smells.ProcessQueryExpression(subQuery.QueryExpression, parentType, false, cte);
                            break;
                        case "ColumnReferenceExpression":

                            var expression = (ColumnReferenceExpression)scalarExpression.Expression;

                            break;
                        case "FunctionCall":
                            smells.FunctionProcessor.ProcessFunctionCall((FunctionCall)scalarExpression.Expression);
                            break;
                        case "IntegerLiteral":
                            break;
                        case "ConvertCall":
                            break;
                    }

                    break;
                case "SelectSetVariable":
                    smells.SelectSetProcessor.ProcessSelectSetVariable((SelectSetVariable)selectElement);
                    break;
            }
        }

        public void ProcessSelectElements(
            IList<SelectElement> selectElements,
            string parentType,
            WithCtesAndXmlNamespaces cte)
        {
            foreach (var selectElement in selectElements)
            {
                ProcessSelectElement(selectElement, parentType, cte);
            }
        }

        private void ProcessHint(OptimizerHint hint, SelectStatement selStatement)
        {
            switch (hint.HintKind)
            {
                case OptimizerHintKind.OrderGroup:
                case OptimizerHintKind.MergeJoin:
                case OptimizerHintKind.HashJoin:
                case OptimizerHintKind.LoopJoin:
                case OptimizerHintKind.ConcatUnion:
                case OptimizerHintKind.HashUnion:
                case OptimizerHintKind.MergeUnion:
                case OptimizerHintKind.KeepUnion:
                    smells.SendFeedBack(4, selStatement);
                    break;
            }
        }
    }
}