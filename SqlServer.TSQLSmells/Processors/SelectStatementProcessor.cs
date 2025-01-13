using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class SelectStatementProcessor
    {
        private readonly Smells _smells;

        public SelectStatementProcessor(Smells smells)
        {
            _smells = smells;
        }

        private void ProcessOptimizerHints(IList<OptimizerHint> OptimizerHints, SelectStatement SelStatement)
        {
            /* OptimizerHints is not a decendant of TSQLFragment */
            foreach (var Hint in OptimizerHints)
            {
                ProcessHint(Hint, SelStatement);
            }
        }

        public void Process(SelectStatement SelStatement, string ParentType, bool TestTop = false,
            WithCtesAndXmlNamespaces Cte = null)
        {
            if (Cte == null && SelStatement.WithCtesAndXmlNamespaces != null)
            {
                Cte = SelStatement.WithCtesAndXmlNamespaces;
                if (Cte != null)
                {
                    _smells.InsertProcessor.ProcessWithCtesAndXmlNamespaces(Cte);
                }
            }

            _smells.ProcessQueryExpression(SelStatement.QueryExpression, ParentType, false, Cte);
            ProcessOptimizerHints(SelStatement.OptimizerHints, SelStatement);
        }

        private void ProcessSelectElement(SelectElement SelectElement, string ParentType, WithCtesAndXmlNamespaces Cte)
        {
            var ElemType = FragmentTypeParser.GetFragmentType(SelectElement);
            switch (ElemType)
            {
                case "SelectStarExpression":
                    _smells.SendFeedBack(5, SelectElement);
                    break;
                case "SelectScalarExpression":

                    var ScalarExpression = (SelectScalarExpression)SelectElement;
                    var ExpressionType = FragmentTypeParser.GetFragmentType(ScalarExpression.Expression);
                    switch (ExpressionType)
                    {
                        case "ScalarSubquery":
                            var SubQuery = (ScalarSubquery)ScalarExpression.Expression;
                            _smells.ProcessQueryExpression(SubQuery.QueryExpression, ParentType, false, Cte);
                            break;
                        case "ColumnReferenceExpression":
                            var Expression = (ColumnReferenceExpression)ScalarExpression.Expression;
                            break;
                        case "FunctionCall":
                            _smells.FunctionProcessor.ProcessFunctionCall((FunctionCall)ScalarExpression.Expression);
                            break;
                        case "IntegerLiteral":
                            break;
                        case "ConvertCall":
                            break;
                    }

                    break;
                case "SelectSetVariable":
                    _smells.SelectSetProcessor.ProcessSelectSetVariable((SelectSetVariable)SelectElement);
                    break;
            }
        }

        public void ProcessSelectElements(IList<SelectElement> SelectElements, string ParentType,
            WithCtesAndXmlNamespaces Cte)
        {
            foreach (var SelectElement in SelectElements)
            {
                ProcessSelectElement(SelectElement, ParentType, Cte);
            }
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
                    _smells.SendFeedBack(4, SelStatement);
                    break;
            }
        }
    }
}