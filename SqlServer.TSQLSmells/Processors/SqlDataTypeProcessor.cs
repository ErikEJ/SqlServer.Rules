using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class SqlDataTypeProcessor
    {
        private readonly Smells smells;

        public SqlDataTypeProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessSqlDataTypeReference(SqlDataTypeReference DataType)
        {
            if (DataType.SqlDataTypeOption == SqlDataTypeOption.Table)
            {
            }

            switch (DataType.SqlDataTypeOption)
            {
                case SqlDataTypeOption.Table:
                    break;
                case SqlDataTypeOption.Text:
                case SqlDataTypeOption.NText:
                case SqlDataTypeOption.Image:
                    smells.SendFeedBack(47, DataType);
                    break;
            }
        }
    }
}