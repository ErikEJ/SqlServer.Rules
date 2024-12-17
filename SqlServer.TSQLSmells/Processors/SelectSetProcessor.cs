using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class SelectSetProcessor
    {
        private readonly Smells _smells;

        public SelectSetProcessor(Smells smells)
        {
            _smells = smells;
        }

        private void ProcessVariableReference(VariableReference VarRef, string VarName)
        {
            var VarAssignment = new VarAssignment();
            VarAssignment.SrcName = VarRef.Name;
            VarAssignment.VarName = VarName;
            _smells.AssignmentList.Add(VarAssignment);
        }

        private void ProcessSelectSetFragment(TSqlFragment Expression, string VarName)
        {
            string ElemType = FragmentTypeParser.GetFragmentType(Expression);
            switch (ElemType)
            {
                case "BinaryExpression":
                    var BinaryExpression = (BinaryExpression)Expression;
                    ProcessSelectSetFragment(BinaryExpression.FirstExpression, VarName);
                    ProcessSelectSetFragment(BinaryExpression.SecondExpression, VarName);
                    break;
                case "VariableReference":
                    ProcessVariableReference((VariableReference)Expression, VarName);
                    break;
                case "FunctionCall":
                    var Func = (FunctionCall)Expression;
                    foreach (TSqlFragment Parameter in Func.Parameters)
                    {
                        ProcessSelectSetFragment(Parameter, VarName);
                    }

                    break;
                case "CastCall":
                    var Cast = (CastCall)Expression;
                    if (FragmentTypeParser.GetFragmentType(Cast.Parameter) == "VariableReference")
                    {
                        ProcessVariableReference((VariableReference)Cast.Parameter, VarName);
                    }

                    break;
                case "StringLiteral":
                    break;
            }
        }

        public void ProcessSelectSetVariable(SelectSetVariable SelectElement)
        {
            string VarName = SelectElement.Variable.Name;
            ScalarExpression Expression = SelectElement.Expression;
            ProcessSelectSetFragment(Expression, VarName);
        }
    }
}