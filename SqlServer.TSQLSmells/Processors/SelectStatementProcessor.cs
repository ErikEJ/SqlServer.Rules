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

        public void Process(SelectStatement SelStatement, string ParentType, bool TestTop = false,
            WithCtesAndXmlNamespaces Cte = null)
        {
            if (Cte == null && SelStatement.WithCtesAndXmlNamespaces != null)
            {
                Cte = SelStatement.WithCtesAndXmlNamespaces;
                if (Cte != null)
                {
                    smells.InsertProcessor.ProcessWithCtesAndXmlNamespaces(Cte);
                }
            }

            smells.ProcessQueryExpression(SelStatement.QueryExpression, ParentType, false, Cte);
            ProcessOptimizerHints(SelStatement.OptimizerHints, SelStatement);
        }

        private void ProcessSelectElement(SelectElement SelectElement, string ParentType, WithCtesAndXmlNamespaces Cte)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var ElemType = FragmentTypeParser.GetFragmentType(SelectElement);
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            switch (ElemType)
            {
                case "SelectStarExpression":
                    smells.SendFeedBack(5, SelectElement);
                    break;
                case "SelectScalarExpression":

#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var ScalarExpression = (SelectScalarExpression)SelectElement;
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
                            smells.ProcessQueryExpression(SubQuery.QueryExpression, ParentType, false, Cte);
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
                    smells.SelectSetProcessor.ProcessSelectSetVariable((SelectSetVariable)SelectElement);
                    break;
            }
        }

        public void ProcessSelectElements(IList<SelectElement> selectElements, string parentType,
            WithCtesAndXmlNamespaces cte)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            foreach (var SelectElement in selectElements)
            {
                ProcessSelectElement(SelectElement, parentType, cte);
            }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
        }

        private void ProcessHint(OptimizerHint Hint, SelectStatement SelStatement)
        {
            switch (Hint.HintKind)
            {
                case OptimizerHintKind.OrderGroup:
                case OptimizerHintKind.MergeJoin:
                case OptimizerHintKind.HashJoin:
                case OptimizerHintKind.LoopJoin:
                case OptimizerHintKind.ConcatUnion:
                case OptimizerHintKind.HashUnion:
                case OptimizerHintKind.MergeUnion:
                case OptimizerHintKind.KeepUnion:
                    smells.SendFeedBack(4, SelStatement);
                    break;
            }
        }
    }
}