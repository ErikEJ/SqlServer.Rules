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

        private void TestViewReference(SchemaObjectName objectName)
        {
            if (objectName.SchemaIdentifier == null)
            {
                smells.SendFeedBack(24, objectName);
            }
        }

        public void ProcessViewStatementBody(ViewStatementBody statementBody)
        {
            TestViewReference(statementBody.SchemaObjectName);
            new SelectStatementProcessor(smells).Process(statementBody.SelectStatement, "VW", true);
        }
    }
}