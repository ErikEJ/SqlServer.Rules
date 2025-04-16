using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;

namespace TSQLSmellSCA
{
    public class TSQLSmellWorker
    {
        private readonly TSqlModel model;
        private readonly string ruleID;

        public TSQLSmellWorker(SqlRuleExecutionContext context, string ruleID)
        {
            model = context.SchemaModel;
            this.ruleID = ruleID;
        }

        public IList<SqlRuleProblem> Analyze()
        {
            var problems = new List<SqlRuleProblem>();

            var exFilter = new[] { ModelSchema.ExtendedProperty };

            var whiteList = new List<TSqlObject>();

            // [SqlTableBase].[dbo].[test].[WhiteList]
            foreach (var tSqlObject in model.GetObjects(DacQueryScopes.UserDefined, exFilter))
            {
                if (tSqlObject.Name.ToString().EndsWith("[WhiteList]", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var refer in tSqlObject.GetReferencedRelationshipInstances())
                    {
                        whiteList.Add(refer.Object);
                    }
                }
            }

            foreach (var tSqlObject in model.GetObjects(DacQueryScopes.UserDefined))
            {
                var isWhite = false;
                foreach (var whiteCheck in whiteList)
                {
                    if (whiteCheck.Equals(tSqlObject))
                    {
                        isWhite = true;
                    }
                }

                if (isWhite)
                {
                    continue;
                }

                problems.AddRange(DoSmells(tSqlObject));
            }

            return problems;
        }

        private List<SqlRuleProblem> DoSmells(TSqlObject sqlObject)
        {
            var problems = new List<SqlRuleProblem>();

            var smellprocess = new Smells();

#pragma warning disable CA1846 // Prefer 'AsSpan' over 'Substring'
            var iRule = int.Parse(ruleID.Substring(ruleID.Length - 3), CultureInfo.InvariantCulture);
#pragma warning restore CA1846 // Prefer 'AsSpan' over 'Substring'
            return smellprocess.ProcessObject(sqlObject, iRule);
        }
    }
}
