using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac.Visitors;

namespace SqlServer.Rules.Globals
{
    public static class Ignorables
    {
        public static IEnumerable<T> NotIgnoredStatements<T>(this IVisitor<T> visitor, string ruleId)
            where T : TSqlFragment
        {
            var scriptTokenStream = visitor.Statements.FirstOrDefault()?.ScriptTokenStream;
            if (scriptTokenStream == null)
            {
                return visitor.Statements;
            }

            return from s in visitor.Statements
                   where ShouldNotIgnoreRule(scriptTokenStream, ruleId, s.StartLine)
                   select s;
        }

        public static IEnumerable<T> NotIgnoredStatements<T>(this IVisitor<T> visitor, IList<TSqlParserToken> scriptTokenStream, string ruleId)
            where T : TSqlFragment
        {
            return from s in visitor.Statements
                   where ShouldNotIgnoreRule(scriptTokenStream, ruleId, s.StartLine)
                   select s;
        }

        private static readonly char[] Separator = new char[] { '.' };

        public static bool ShouldNotIgnoreRule(IList<TSqlParserToken> scriptTokenStream, string ruleId, int lineNumber)
        {
            if (scriptTokenStream == null)
            {
                return false;
            }

            var baseRuleId = ruleId.Split(Separator, StringSplitOptions.RemoveEmptyEntries).Last();
            var ignoreRegex = $@"\bIGNORE\b.*\b{baseRuleId}\b";
            var globalIgnoreRegex = $@"\bGLOBAL\b\s*\bIGNORE\b.*\b{baseRuleId}\b";

            var result =
                from t in scriptTokenStream
                where (t.TokenType == TSqlTokenType.SingleLineComment || t.TokenType == TSqlTokenType.MultilineComment)
                    && (
                        ((t.Line == lineNumber || t.Line == lineNumber - 1) && Regex.IsMatch(t.Text, ignoreRegex, RegexOptions.IgnoreCase))
                        || Regex.IsMatch(t.Text, globalIgnoreRegex, RegexOptions.IgnoreCase)
                    )
                select t.Text;

            return !result.Any();
        }
    }
}
