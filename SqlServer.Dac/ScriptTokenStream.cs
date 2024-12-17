using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Dac
{
    public static class Utils
    {
        public static string ToScript(this IList<TSqlParserToken> scriptTokenStream)
        {
            if (scriptTokenStream == null) { return null; }
            return string.Join("", scriptTokenStream.Select(t => t.Text));

        }
    }
}
