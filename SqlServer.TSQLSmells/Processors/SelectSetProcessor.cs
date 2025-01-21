using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class SelectSetProcessor
    {
        private readonly Smells smells;

        public SelectSetProcessor(Smells smells)
        {
            this.smells = smells;
        }

        private void ProcessVariableReference(VariableReference varRef, string varName)
        {
            var varAssignment = new VarAssignment
            {
                SrcName = varRef.Name,
                VarName = varName,
            };
            smells.AssignmentList.Add(varAssignment);
        }

        private void ProcessSelectSetFragment(TSqlFragment expression, string varName)
        {
            var elemType = FragmentTypeParser.GetFragmentType(expression);
            switch (elemType)
            {
                case "BinaryExpression":
                    var binaryExpression = (BinaryExpression)expression;
                    ProcessSelectSetFragment(binaryExpression.FirstExpression, varName);
                    ProcessSelectSetFragment(binaryExpression.SecondExpression, varName);
                    break;
                case "VariableReference":
                    ProcessVariableReference((VariableReference)expression, varName);
                    break;
                case "FunctionCall":
                    var func = (FunctionCall)expression;
                    foreach (TSqlFragment parameter in func.Parameters)
                    {
                        ProcessSelectSetFragment(parameter, varName);
                    }

                    break;
                case "CastCall":
                    var cast = (CastCall)expression;
                    if (FragmentTypeParser.GetFragmentType(cast.Parameter) == "VariableReference")
                    {
                        ProcessVariableReference((VariableReference)cast.Parameter, varName);
                    }

                    break;
                case "StringLiteral":
                    break;
            }
        }

        public void ProcessSelectSetVariable(SelectSetVariable selectElement)
        {
            var varName = selectElement.Variable.Name;
            var expression = selectElement.Expression;
            ProcessSelectSetFragment(expression, varName);
        }
    }
}