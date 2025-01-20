using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class CursorProcessor
    {
        private readonly Smells smells;

        public CursorProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessCursorStatement(DeclareCursorStatement cursorStatement)
        {
            if (cursorStatement.CursorDefinition == null || cursorStatement.CursorDefinition.Options.Count == 0)
            {
                smells.SendFeedBack(29, cursorStatement);
            }
        }
    }
}