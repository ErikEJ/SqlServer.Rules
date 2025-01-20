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

        private void ProcessVariableReference(VariableReference VarRef, string VarName)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var VarAssignment = new VarAssignment
            {
                SrcName = VarRef.Name,
                VarName = VarName,
            };
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            smells.AssignmentList.Add(VarAssignment);
        }

        private void ProcessSelectSetFragment(TSqlFragment Expression, string VarName)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var ElemType = FragmentTypeParser.GetFragmentType(Expression);
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            switch (ElemType)
            {
                case "BinaryExpression":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var BinaryExpression = (BinaryExpression)Expression;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    ProcessSelectSetFragment(BinaryExpression.FirstExpression, VarName);
                    ProcessSelectSetFragment(BinaryExpression.SecondExpression, VarName);
                    break;
                case "VariableReference":
                    ProcessVariableReference((VariableReference)Expression, VarName);
                    break;
                case "FunctionCall":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var Func = (FunctionCall)Expression;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    foreach (TSqlFragment Parameter in Func.Parameters)
                    {
                        ProcessSelectSetFragment(Parameter, VarName);
                    }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter

                    break;
                case "CastCall":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var Cast = (CastCall)Expression;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
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
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var VarName = SelectElement.Variable.Name;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var Expression = SelectElement.Expression;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            ProcessSelectSetFragment(Expression, VarName);
        }
    }
}