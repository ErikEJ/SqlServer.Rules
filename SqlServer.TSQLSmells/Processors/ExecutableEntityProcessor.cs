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
            foreach (TSqlFragment fragment in stringList.Strings)
            {
                switch (FragmentTypeParser.GetFragmentType(fragment))
                {
                    case "VariableReference":
                        var varRef = (VariableReference)fragment;
                        if (TestVariableAssigmentChain(varRef.Name))
                        {
                            return true;
                        }

                        break;
                }
            }

            return false;
        }

        public void ProcessExecutableEntity(ExecutableEntity executableEntity)
        {
            switch (FragmentTypeParser.GetFragmentType(executableEntity))
            {
                case "ExecutableProcedureReference":

                    var procReference = (ExecutableProcedureReference)executableEntity;

                    if (procReference.ProcedureReference.ProcedureReference.Name.SchemaIdentifier == null &&
                        !procReference.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value.StartsWith(
                            "sp_", StringComparison.OrdinalIgnoreCase))
                    {
                        smells.SendFeedBack(21, executableEntity);
                    }

                    if (
                        procReference.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value.Equals(
                            "sp_executesql", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var param in executableEntity.Parameters)
                        {
                            if (param.Variable.Name.Equals("@stmt", StringComparison.OrdinalIgnoreCase))
                            {
                                if (FragmentTypeParser.GetFragmentType(param.ParameterValue) == "VariableReference")
                                {
                                    var var = (VariableReference)param.ParameterValue;
                                    if (TestVariableAssigmentChain(var.Name))
                                    {
                                        smells.SendFeedBack(43, executableEntity);
                                    }
                                }
                            }
                        }
                    }

                    break;
                case "ExecutableStringList":

                    var stringList = (ExecutableStringList)executableEntity;

                    if (InjectionTesting(stringList))
                    {
                        smells.SendFeedBack(43, executableEntity);
                    }

                    break;
            }
        }

        public void ProcessExecuteStatement(ExecuteStatement fragment)
        {
            var executableEntity = fragment.ExecuteSpecification.ExecutableEntity;

            ProcessExecutableEntity(executableEntity);
        }

        public bool TestVariableAssigmentChain(string varName)
        {
            foreach (var param in smells.ProcedureStatementBodyProcessor.ParameterList)
            {
                if (param.VariableName.Value.Equals(varName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            foreach (var varOn in smells.AssignmentList)
            {
                if (varOn.VarName.Equals(varName, StringComparison.OrdinalIgnoreCase))
                {
                    if (TestVariableAssigmentChain(varOn.SrcName))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}