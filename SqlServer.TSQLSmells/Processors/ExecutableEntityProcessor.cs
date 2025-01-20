using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class ExecutableEntityProcessor
    {
        private readonly Smells smells;

        public ExecutableEntityProcessor(Smells smells)
        {
            this.smells = smells;
        }

        private bool InjectionTesting(ExecutableStringList stringList)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            foreach (TSqlFragment Fragment in stringList.Strings)
            {
                switch (FragmentTypeParser.GetFragmentType(Fragment))
                {
                    case "VariableReference":
                        var varRef = (VariableReference)Fragment;
                        if (TestVariableAssigmentChain(varRef.Name))
                        {
                            return true;
                        }

                        break;
                }
            }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter

            return false;
        }

        public void ProcessExecutableEntity(ExecutableEntity executableEntity)
        {
            switch (FragmentTypeParser.GetFragmentType(executableEntity))
            {
                case "ExecutableProcedureReference":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var ProcReference = (ExecutableProcedureReference)executableEntity;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    if (ProcReference.ProcedureReference.ProcedureReference.Name.SchemaIdentifier == null &&
                        !ProcReference.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value.StartsWith(
                            "sp_", StringComparison.OrdinalIgnoreCase))
                    {
                        smells.SendFeedBack(21, executableEntity);
                    }

                    if (
                        ProcReference.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value.Equals(
                            "sp_executesql", StringComparison.OrdinalIgnoreCase))
                    {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                        foreach (var Param in executableEntity.Parameters)
                        {
                            if (Param.Variable.Name.Equals("@stmt", StringComparison.OrdinalIgnoreCase))
                            {
                                if (FragmentTypeParser.GetFragmentType(Param.ParameterValue) == "VariableReference")
                                {
                                    var var = (VariableReference)Param.ParameterValue;
                                    if (TestVariableAssigmentChain(var.Name))
                                    {
                                        smells.SendFeedBack(43, executableEntity);
                                    }
                                }
                            }
                        }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    }

                    break;
                case "ExecutableStringList":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var StringList = (ExecutableStringList)executableEntity;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    if (InjectionTesting(StringList))
                    {
                        smells.SendFeedBack(43, executableEntity);
                    }

                    break;
            }
        }

        public void ProcessExecuteStatement(ExecuteStatement fragment)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var ExecutableEntity = fragment.ExecuteSpecification.ExecutableEntity;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            ProcessExecutableEntity(ExecutableEntity);
        }

        public bool TestVariableAssigmentChain(string varName)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            foreach (var Param in smells.ProcedureStatementBodyProcessor.ParameterList)
            {
                if (Param.VariableName.Value.Equals(varName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter

#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            foreach (var VarOn in smells.AssignmentList)
            {
                if (VarOn.VarName.Equals(varName, StringComparison.OrdinalIgnoreCase))
                {
                    if (TestVariableAssigmentChain(VarOn.SrcName))
                    {
                        return true;
                    }
                }
            }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter

            return false;
        }
    }
}