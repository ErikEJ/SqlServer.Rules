using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace TSQLSmellSCA
{
    public class ProcedureStatementBodyProcessor
    {
        private readonly Smells _smells;
        public bool NoCountSet = false;
        private IList<ProcedureParameter> _parameterList;

        public ProcedureStatementBodyProcessor(Smells smells)
        {
            _smells = smells;
        }

        public IList<ProcedureParameter> ParameterList
        {
            get { return _parameterList; }
            set { _parameterList = value; }
        }

        private void TestProcedureReference(ProcedureReference PrcRef)
        {
            if (PrcRef.Name.SchemaIdentifier == null)
            {
                _smells.SendFeedBack(24, PrcRef);
            }
        }

        public void ProcessProcedureStatementBody(ProcedureStatementBody StatementBody)
        {
            _smells.AssignmentList.Clear();

            TestProcedureReference(StatementBody.ProcedureReference);
            ParameterList = StatementBody.Parameters;

            NoCountSet = false;
            if (StatementBody.StatementList != null)
            {
                foreach (TSqlFragment Fragment in StatementBody.StatementList.Statements)
                {
                    _smells.ProcessTsqlFragment(Fragment);
                }

                if (!NoCountSet) _smells.SendFeedBack(30, StatementBody.ProcedureReference);
            }

            ParameterList = null;
        }
    }
}