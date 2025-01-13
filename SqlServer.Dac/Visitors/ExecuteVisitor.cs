using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class ExecuteVisitor : BaseVisitor, IVisitor<ExecuteStatement>
    {
        private readonly IList<string> _procNames;
        public ExecuteVisitor()
        {
            _procNames = new List<string>();
        }

        public ExecuteVisitor(params string[] procNames)
        {
            _procNames = procNames.ToList();
        }

        public IList<ExecuteStatement> Statements { get; } = new List<ExecuteStatement>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(ExecuteStatement node)
        {
            if (!_procNames.Any())
            {
                Statements.Add(node);
            }
            else if (_procNames.Any(f => CheckProcName(node, f)))
            {
                Statements.Add(node);
            }
        }

        private static bool CheckProcName(ExecuteStatement exec, string name)
        {
            if (!(exec.ExecuteSpecification.ExecutableEntity is ExecutableProcedureReference execProc))
            {
                return false;
            }

            var procName = execProc.ProcedureReference.ProcedureReference.Name.GetName();
            return Regex.IsMatch(procName, name, RegexOptions.IgnoreCase);
        }
    }
}