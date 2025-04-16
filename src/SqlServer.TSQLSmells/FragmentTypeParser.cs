using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public static class FragmentTypeParser
    {
        public static string GetFragmentType(TSqlFragment statement)
        {
            var type = statement.ToString();
            var typeSplit = type!.Split('.');
            var stmtType = typeSplit[typeSplit.Length - 1];
            return stmtType;
        }
    }
}