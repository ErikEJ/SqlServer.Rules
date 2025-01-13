using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SqlServer.Rules.Report
{
    [Serializable]
    public class Report
    {
        [XmlAttribute]
        public string ToolsVersion { get; set; }

        public Information Information { get; set; }
        public List<IssueType> IssueTypes { get; set; }
        public List<RulesProject> Issues { get; set; }

        public Report()
        {
        }

        public Report(string solutionName, List<IssueType> issueTypes, string projectName, List<Issue> problems)
        {
            ToolsVersion = typeof(Report).Assembly.GetName().Version.ToString();
            Information = new Information { Solution = $"{solutionName}.sln" };
            IssueTypes = issueTypes;
            Issues =
            [
                new RulesProject
                {
                    Name = projectName,
                    Issues = problems,
                },
            ];
        }
    }
}