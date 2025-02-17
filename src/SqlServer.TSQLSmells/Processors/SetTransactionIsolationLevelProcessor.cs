using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class SetTransactionIsolationLevelProcessor
    {
        private readonly Smells smells;

        public SetTransactionIsolationLevelProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessSetTransactionIolationLevelStatement(SetTransactionIsolationLevelStatement statement)
        {
            switch (statement.Level)
            {
                case IsolationLevel.ReadUncommitted:
                    smells.SendFeedBack(10, statement);
                    break;
            }
        }
    }
}