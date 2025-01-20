using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class DeclareVariableProcessor
    {
        private readonly Smells smells;

        public DeclareVariableProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessDeclareVariableElement(DeclareVariableElement element)
        {
            if (element.VariableName.Value.Length <= 2)
            {
                smells.SendFeedBack(33, element);
            }

            smells.ProcessTsqlFragment(element.DataType);
            if (element.Value != null)
            {
                smells.ProcessTsqlFragment(element.Value);
            }
        }

        public void ProcessDeclareVariableStatement(DeclareVariableStatement statement)
        {
            foreach (var variable in statement.Declarations)
            {
                smells.ProcessTsqlFragment(variable);
            }
        }
    }
}