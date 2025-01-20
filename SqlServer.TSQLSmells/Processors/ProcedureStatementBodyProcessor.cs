using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class ProcedureStatementBodyProcessor
    {
        private readonly Smells smells;
        public bool NoCountSet { get; set; }
        private IList<ProcedureParameter> _parameterList;

        public ProcedureStatementBodyProcessor(Smells smells)
        {
            this.smells = smells;
        }

#pragma warning disable CA2227 // Collection properties should be read only
        public IList<ProcedureParameter> ParameterList
#pragma warning restore CA2227 // Collection properties should be read only
        {
            get { return _parameterList; }
            set { _parameterList = value; }
        }

        private void TestProcedureReference(ProcedureReference PrcRef)
        {
            if (PrcRef.Name.SchemaIdentifier == null)
            {
                smells.SendFeedBack(24, PrcRef);
            }
        }

        public void ProcessProcedureStatementBody(ProcedureStatementBody StatementBody)
        {
            smells.AssignmentList.Clear();

            TestProcedureReference(StatementBody.ProcedureReference);
            ParameterList = StatementBody.Parameters;

            NoCountSet = false;
            if (StatementBody.StatementList != null)
            {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                foreach (TSqlFragment Fragment in StatementBody.StatementList.Statements)
                {
                    smells.ProcessTsqlFragment(Fragment);
                }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter

                if (!NoCountSet)
                {
                    smells.SendFeedBack(30, StatementBody.ProcedureReference);
                }
            }

            ParameterList = null;
        }
    }
}