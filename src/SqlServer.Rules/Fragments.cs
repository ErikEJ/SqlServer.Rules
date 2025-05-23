using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac.Visitors;

namespace SqlServer.Dac
{
    public static class Fragments
    {
        public static void Accept(this TSqlFragment fragment, params TSqlFragmentVisitor[] visitors)
        {
            foreach (var visitor in visitors)
            {
                fragment.Accept(visitor);
            }
        }

        public static TSqlFragment GetFragment(this SqlRuleExecutionContext ruleExecutionContext, bool forceParse = false)
        {
            // if forceparse is true, we don't care about the type, we want to parse the object so as to get the header comments as well
            if (!forceParse)
            {
                var fragment = ruleExecutionContext.ScriptFragment;
                if (!(
                    fragment.GetType() == typeof(TSqlStatement)
                    || fragment.GetType() == typeof(TSqlStatementSnippet)
                    || fragment.GetType() == typeof(TSqlScript)
                ))
                {
                    return fragment;
                }
            }

            return ruleExecutionContext.ModelElement.GetFragment();
        }

        public static TSqlFragment GetFragment(this TSqlObject obj)
        {
            return GetFragment(obj, out var parseErrors);
        }

        public static TSqlFragment GetFragment(this TSqlObject obj, out IList<ParseError> parseErrors)
        {
            var tsqlParser = new TSql140Parser(true);
            TSqlFragment fragment = null;

            if (!obj.TryGetAst(out var ast))
            {
                parseErrors = new List<ParseError>();
                return fragment;
            }

            if (!obj.TryGetScript(out var script))
            {
                parseErrors = new List<ParseError>();
                return fragment;
            }

            using (var stringReader = new StringReader(script))
            {
                fragment = tsqlParser.Parse(stringReader, out parseErrors);

                // so even after parsing, some scripts are coming back as T-SQL script, lets try to get the root object
                if (fragment != null && fragment.GetType() == typeof(TSqlScript))
                {
                    fragment = ((TSqlScript)fragment).Batches.FirstOrDefault()?.Statements.FirstOrDefault();
                }
            }

            return fragment;
        }

        public static TSqlFragment GetFragment(this TSqlFragment baseFragment, params Type[] typesToLookFor)
        {
            // for some odd reason, sometimes the fragments do not pass in properly to the rules....
            // this function can re-parse that fragment into its true fragment, and not a SQL script...
            if (!(baseFragment is TSqlScript script))
            {
                return baseFragment;
            }

            var stmt = script.Batches.FirstOrDefault()?.Statements.FirstOrDefault();
            if (stmt == null)
            {
                return script;
            }

            // we don't need to parse the fragment unless it is of type TSqlStatement or TSqlStatementSnippet.... just return the type it found
            if (!(stmt.GetType() == typeof(TSqlStatement) || stmt.GetType() == typeof(TSqlStatementSnippet)))
            {
                return stmt;
            }

            var tsqlParser = new TSql140Parser(true);
            using (var stringReader = new StringReader(((TSqlStatementSnippet)stmt).Script))
            {
                IList<ParseError> parseErrors = new List<ParseError>();
                var fragment = tsqlParser.Parse(stringReader, out parseErrors);
                if (parseErrors.Any())
                {
                    return script;
                }

                var visitor = new TypesVisitor(typesToLookFor);
                fragment.Accept(visitor);

                if (visitor.Statements.Any())
                {
                    return visitor.Statements.First();
                }
            }

            // if we got here, the object was tsqlscript, but was not parseable.... so we bail out
            return script;
        }
    }
}
