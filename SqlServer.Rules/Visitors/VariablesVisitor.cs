using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class VariablesVisitor : BaseVisitor, IBaseVisitor
    {
        public IList<DeclareVariableStatement> DeclareVariables { get; } = new List<DeclareVariableStatement>();

        public IList<ProcedureParameter> ProcedureParameters { get; } = new List<ProcedureParameter>();

        public IList<VariableReference> VariableReferences { get; } = new List<VariableReference>();

        public IList<SelectSetVariable> SelectSetVariables { get; } = new List<SelectSetVariable>();

        public int Count
        {
            get
            {
                return (DeclareVariables != null ? DeclareVariables.Count : 0)
                    + (ProcedureParameters != null ? ProcedureParameters.Count : 0)
                    + (VariableReferences != null ? VariableReferences.Count : 0)
                    + (SelectSetVariables != null ? SelectSetVariables.Count : 0);
            }
        }

        public override void ExplicitVisit(VariableReference node)
        {
            VariableReferences.Add(node);
        }

        public override void ExplicitVisit(SelectSetVariable node)
        {
            SelectSetVariables.Add(node);
        }

        public override void ExplicitVisit(DeclareVariableStatement node)
        {
            DeclareVariables.Add(node);
        }

        public override void ExplicitVisit(ProcedureParameter node)
        {
            ProcedureParameters.Add(node);
        }

        public IList<DataTypeView> GetVariables()
        {
            var ret = new List<DataTypeView>();

            var declares =
                from dv in DeclareVariables
                from d in dv.Declarations
                select new DataTypeView(d.VariableName.Value, d.DataType, DataTypeViewType.Declare);

            var parameters =
                from p in ProcedureParameters
                select new DataTypeView(p.VariableName.Value, p.DataType, DataTypeViewType.Parameter);

            ret.AddRange(parameters);
            ret.AddRange(declares);

            return ret;
        }
    }
}