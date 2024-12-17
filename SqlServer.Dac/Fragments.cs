using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac.Visitors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SqlServer.Dac
{
    public static class Fragments
    {
        #region Fragments
        public static void Accept(this TSqlFragment fragment, params TSqlFragmentVisitor[] visitors)
        {
            foreach (var visitor in visitors)
            {
                fragment.Accept(visitor);
            }
        }

        /// <summary>
        /// Recursively searches a fragment searching for another fragment and when found removes the fragment when it is found.
        /// </summary>
        /// <param name="fragment">The fragment to search.</param>
        /// <param name="remove">The fragment to remove when found.</param>
        /// <returns></returns>
        public static bool RemoveRecursive(this TSqlFragment fragment, TSqlFragment remove)
        {
            switch (fragment)
            {
                case TSqlScript script:
                    foreach (var bch in script.Batches)
                    {
                        if (bch.RemoveRecursive(remove)) { return true; }
                    }
                    break;
                case TSqlBatch batch:
                    if (RemoveStatementFromList(batch.Statements, remove)) { return true; }
                    break;
                case WhileStatement whileBlock:
                    if (whileBlock.Statement.RemoveRecursive(remove)) { return true; }
                    break;
                case StatementList stmts:
                    if (RemoveStatementFromList(stmts.Statements, remove)) { return true; }
                    break;
                case BeginEndBlockStatement beBlock:
                    if (RemoveStatementFromList(beBlock.StatementList.Statements, remove)) { return true; }
                    break;
                case IfStatement ifBlock:
                    if (ifBlock.ThenStatement.RemoveRecursive(remove)
                        || ifBlock.ElseStatement.RemoveRecursive(remove)) { return true; }
                    break;
                case TryCatchStatement tryBlock:
                    if (RemoveStatementFromList(tryBlock.TryStatements.Statements, remove)
                        || RemoveStatementFromList(tryBlock.CatchStatements.Statements, remove)) { return true; }
                    break;
                default:
                    Debug.WriteLine(fragment);
                    break;
            }

            return false;
        }

        private static bool RemoveStatementFromList(IList<TSqlStatement> statements, TSqlFragment remove)
        {
            foreach (var stmt in statements)
            {
                if (stmt == remove)
                {
                    return statements.Remove(stmt);
                }

                if (stmt.RemoveRecursive(remove)) { return true; }
            }
            return false;
        }

        /// <summary>
        /// Converts a tsql object into a fragment, if it is not already one.
        /// </summary>
        /// <param name="forceParse">If true will force the parsing of the sql into a fragment</param>
        /// <returns></returns>
        public static TSqlFragment GetFragment(this SqlRuleExecutionContext ruleExecutionContext, bool forceParse = false)
        {
            //if forceparse is true, we don't care about the type, we want to parse the object so as to get the header comments as well
            if (!forceParse)
            {
                var fragment = ruleExecutionContext.ScriptFragment;
                if (!(
                    fragment.GetType() == typeof(TSqlStatement)
                    || fragment.GetType() == typeof(TSqlStatementSnippet)
                    || fragment.GetType() == typeof(TSqlScript)
                )) { return fragment; }
            }

            return ruleExecutionContext.ModelElement.GetFragment();
        }

        public static TSqlFragment GetFragment(this TSqlObject obj)
        {
            return GetFragment(obj, out IList<ParseError> parseErrors);
        }

        /// <summary>
        /// Converts a tsql object into a fragment
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="parseErrors"></param>
        /// <returns></returns>
        public static TSqlFragment GetFragment(this TSqlObject obj, out IList<ParseError> parseErrors)
        {
            var tsqlParser = new TSql140Parser(true);
            TSqlFragment fragment = null;

            if (!obj.TryGetAst(out TSqlScript ast))
            {
                parseErrors = new List<ParseError>();
                return fragment;
            }

            if (!obj.TryGetScript(out string script))
            {
                parseErrors = new List<ParseError>();
                return fragment;
            }

            using (StringReader stringReader = new StringReader(script))
            {
                fragment = tsqlParser.Parse(stringReader, out parseErrors);

                //so even after parsing, some scripts are coming back as tsql script, lets try to get the root object
                if (fragment != null && fragment.GetType() == typeof(TSqlScript))
                {
                    fragment = ((TSqlScript)fragment).Batches.FirstOrDefault()?.Statements.FirstOrDefault();
                }
            }
            return fragment;
        }

        /// <summary>
        /// Searches the entire fragment, for specific types
        /// </summary>
        /// <param name="baseFragment"></param>
        /// <param name="typesToLookFor"></param>
        /// <returns></returns>
        public static TSqlFragment GetFragment(this TSqlFragment baseFragment, params Type[] typesToLookFor)
        {
            //for some odd reason, sometimes the fragments do not pass in properly to the rules....
            //this function can re-parse that fragment into its true fragment, and not a sql script...
            if (!(baseFragment is TSqlScript script)) { return baseFragment; }

            var stmt = script?.Batches.FirstOrDefault()?.Statements.FirstOrDefault();
            if (stmt == null) { return script; }
            //we don't need to parse the fragment unless it is of type TSqlStatement or TSqlStatementSnippet.... just return the type it found
            if (!(stmt.GetType() == typeof(TSqlStatement) || stmt.GetType() == typeof(TSqlStatementSnippet))) { return stmt; }

            var tsqlParser = new TSql140Parser(true);
            using (StringReader stringReader = new StringReader(((TSqlStatementSnippet)stmt).Script))
            {
                IList<ParseError> parseErrors = new List<ParseError>();
                var fragment = tsqlParser.Parse(stringReader, out parseErrors);
                if (parseErrors.Any()) { return script; }

                TypesVisitor visitor = new TypesVisitor(typesToLookFor);
                fragment.Accept(visitor);

                if (visitor.Statements.Any())
                {
                    return visitor.Statements.First();
                }
            }
            //if we got here, the object was tsqlscript, but was not parseable.... so we bail out
            return script;
        }

        public static TSqlFragment GetFragment(this FileInfo file)
        {
            TSqlFragment fragment;
            var tsqlParser = new TSql140Parser(true);
            using (TextReader textReader = file.OpenText())
            {
                fragment = tsqlParser.Parse(textReader, out IList<ParseError> parseErrors) as TSqlScript;
                if (fragment == null)
                {
                    throw new ApplicationException($"Unable to parse file {file}");
                }

                if (parseErrors.Any())
                {
                    throw new ApplicationException($"Unable to parse file {file}, errors: {string.Join("\r\n", parseErrors.Select(e => $"Line: {e.Line}, Error: {e.Message}"))}");
                }
            }
            return fragment;
        }

        /// <summary>
        /// Scripts the fragment to a string
        /// </summary>
        /// <param name="fragment"></param>
        /// <returns></returns>
        public static string GetScript(this TSqlFragment fragment)
        {
            var generator = new Sql140ScriptGenerator();
            generator.GenerateScript(fragment, out string sql);
            return sql;
        }

        /// <summary>
        /// Scripts the fragment to a text writer
        /// </summary>
        /// <param name="fragment"></param>
        /// <param name="writer"></param>
        public static void GetScript(this TSqlFragment fragment, TextWriter writer)
        {
            var generator = new Sql140ScriptGenerator();
            generator.GenerateScript(fragment, writer);
        }

        public static string GetScript(this IList<TSqlParserToken> scriptTokenStream)
        {
            var sb = new StringBuilder();
            foreach (var t in scriptTokenStream)
            {
                sb.Append(t.Text);
            }
            return sb.ToString();
        }
        #endregion Fragments

    }
}
