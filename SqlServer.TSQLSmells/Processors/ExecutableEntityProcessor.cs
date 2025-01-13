using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class ExecutableEntityProcessor
    {
        private readonly Smells _smells;

        public ExecutableEntityProcessor(Smells smells)
        {
            _smells = smells;
        }

        private bool InjectionTesting(ExecutableStringList StringList)
        {
            foreach (TSqlFragment Fragment in StringList.Strings)
            {
                switch (FragmentTypeParser.GetFragmentType(Fragment))
                {
                    case "VariableReference":
                        var varRef = (VariableReference) Fragment;
                        if (TestVariableAssigmentChain(varRef.Name))
                        {
                            return true;
                        }

                        break;
                }
            }

            return false;
        }

        public void ProcessExecutableEntity(ExecutableEntity ExecutableEntity)
        {
            switch (FragmentTypeParser.GetFragmentType(ExecutableEntity))
            {
                case "ExecutableProcedureReference":
                    var ProcReference = (ExecutableProcedureReference) ExecutableEntity;
                    if (ProcReference.ProcedureReference.ProcedureReference.Name.SchemaIdentifier == null &&
                        !ProcReference.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value.StartsWith(
                            "sp_", StringComparison.OrdinalIgnoreCase))
                    {
                        _smells.SendFeedBack(21, ExecutableEntity);
                    }

                    if (
                        ProcReference.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value.Equals(
                            "sp_executesql", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var Param in ExecutableEntity.Parameters)
                        {
                            if (Param.Variable.Name.Equals("@stmt", StringComparison.OrdinalIgnoreCase))
                            {
                                if (FragmentTypeParser.GetFragmentType(Param.ParameterValue) == "VariableReference")
                                {
                                    var var = (VariableReference) Param.ParameterValue;
                                    if (TestVariableAssigmentChain(var.Name))
                                    {
                                        _smells.SendFeedBack(43, ExecutableEntity);
                                    }
                                }
                            }
                        }
                    }

                    break;
                case "ExecutableStringList":
                    var StringList = (ExecutableStringList) ExecutableEntity;
                    if (InjectionTesting(StringList))
                    {
                        _smells.SendFeedBack(43, ExecutableEntity);
                    }

                    break;
            }
        }

        public void ProcessExecuteStatement(ExecuteStatement Fragment)
        {
            var ExecutableEntity = Fragment.ExecuteSpecification.ExecutableEntity;
            ProcessExecutableEntity(ExecutableEntity);
        }

        public bool TestVariableAssigmentChain(string VarName)
        {
            foreach (var Param in _smells.ProcedureStatementBodyProcessor.ParameterList)
            {
                if (Param.VariableName.Value.Equals(VarName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            foreach (var VarOn in _smells.AssignmentList)
            {
                if (VarOn.VarName.Equals(VarName, StringComparison.OrdinalIgnoreCase))
                {
                    if (TestVariableAssigmentChain(VarOn.SrcName))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}