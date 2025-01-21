using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class ProcedureStatementBodyProcessor
    {
        private readonly Smells smells;
        public bool NoCountSet { get; set; }
        private IList<ProcedureParameter> parameterList;

        public ProcedureStatementBodyProcessor(Smells smells)
        {
            this.smells = smells;
        }

#pragma warning disable CA2227 // Collection properties should be read only
        public IList<ProcedureParameter> ParameterList
#pragma warning restore CA2227 // Collection properties should be read only
        {
            get { return parameterList; }
            set { parameterList = value; }
        }

        private void TestProcedureReference(ProcedureReference prcRef)
        {
            if (prcRef.Name.SchemaIdentifier == null)
            {
                smells.SendFeedBack(24, prcRef);
            }
        }

        public void ProcessProcedureStatementBody(ProcedureStatementBody statementBody)
        {
            smells.AssignmentList.Clear();

            TestProcedureReference(statementBody.ProcedureReference);
            ParameterList = statementBody.Parameters;

            NoCountSet = false;
            if (statementBody.StatementList != null)
            {
                foreach (TSqlFragment fragment in statementBody.StatementList.Statements)
                {
                    smells.ProcessTsqlFragment(fragment);
                }

                if (!NoCountSet)
                {
                    smells.SendFeedBack(30, statementBody.ProcedureReference);
                }
            }

            ParameterList = null;
        }
    }
}