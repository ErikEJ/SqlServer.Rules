using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TSQLSmellSCA
{
    public class FragmentTypeParser
    {
        public static string GetFragmentType(TSqlFragment Statement)
        {
            String Type = Statement.ToString();
            String[] TypeSplit = Type.Split('.');
            String StmtType = TypeSplit[TypeSplit.Length - 1];
            return (StmtType);
        }
    }
}