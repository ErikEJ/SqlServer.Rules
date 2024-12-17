using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Test;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SqlServer.Rules.Tests
{
    [TestClass]
    public class TestCasesBase
    {
        protected const SqlServerVersion SqlVersion = SqlServerVersion.Sql150;
        protected StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

        public virtual TestContext TestContext { get; set; }

        protected ReadOnlyCollection<SqlRuleProblem> GetTestCaseProblems(string testCases, string ruleId)
        {
            ReadOnlyCollection<SqlRuleProblem> problems = new ReadOnlyCollection<SqlRuleProblem>(new List<SqlRuleProblem>());

            using (var test = new BaselineSetup(TestContext, testCases, new TSqlModelOptions(), SqlVersion))
            {
                try
                {
                    test.RunTest(ruleId, (result, problemString) => problems = result.Problems);
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Exception thrown for ruleId '{ruleId}' for test cases '{testCases}': {ex.Message}");
                }
            }

            return problems;
        }
    }
}