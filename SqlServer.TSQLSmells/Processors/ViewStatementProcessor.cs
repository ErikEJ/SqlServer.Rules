using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class ViewStatementProcessor
    {
        private readonly Smells smells;

        public ViewStatementProcessor(Smells smells)
        {
            this.smells = smells;
        }

        private void TestViewReference(SchemaObjectName ObjectName)
        {
            if (ObjectName.SchemaIdentifier == null)
            {
                smells.SendFeedBack(24, ObjectName);
            }
        }

        public void ProcessViewStatementBody(ViewStatementBody StatementBody)
        {
            TestViewReference(StatementBody.SchemaObjectName);
            new SelectStatementProcessor(smells).Process(StatementBody.SelectStatement, "VW", true);
        }
    }
}