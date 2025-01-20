using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public static class FragmentTypeParser
    {
        public static string GetFragmentType(TSqlFragment Statement)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var Type = Statement.ToString();
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var TypeSplit = Type.Split('.');
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var StmtType = TypeSplit[TypeSplit.Length - 1];
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            return StmtType;
        }
    }
}