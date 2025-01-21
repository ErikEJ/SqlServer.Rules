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
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            foreach (var Hint in optimizerHints)
            {
                ProcessHint(Hint, selStatement);
            }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
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
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var ElemType = FragmentTypeParser.GetFragmentType(selectElement);
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            switch (ElemType)
            {
                case "SelectStarExpression":
                    smells.SendFeedBack(5, selectElement);
                    break;
                case "SelectScalarExpression":

#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var ScalarExpression = (SelectScalarExpression)selectElement;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var ExpressionType = FragmentTypeParser.GetFragmentType(ScalarExpression.Expression);
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    switch (ExpressionType)
                    {
                        case "ScalarSubquery":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                            var SubQuery = (ScalarSubquery)ScalarExpression.Expression;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                            smells.ProcessQueryExpression(SubQuery.QueryExpression, parentType, false, cte);
                            break;
                        case "ColumnReferenceExpression":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                            var Expression = (ColumnReferenceExpression)ScalarExpression.Expression;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                            break;
                        case "FunctionCall":
                            smells.FunctionProcessor.ProcessFunctionCall((FunctionCall)ScalarExpression.Expression);
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
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            foreach (var SelectElement in selectElements)
            {
                ProcessSelectElement(SelectElement, parentType, cte);
            }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
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