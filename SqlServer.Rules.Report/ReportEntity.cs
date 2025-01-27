using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SqlServer.Rules.Report;

[Serializable]
public class ReportEntity
{
    [XmlAttribute]
    public string ToolsVersion { get; set; }

    public Information Information { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
    public List<IssueType> IssueTypes { get; set; }

    public List<RulesProject> Issues { get; set; }

#pragma warning restore CA2227 // Collection properties should be read only
    public ReportEntity()
    {
    }

    public ReportEntity(string solutionName, List<IssueType> issueTypes, string projectName, List<Issue> problems)
    {
        ToolsVersion = typeof(ReportEntity).Assembly.GetName().Version.ToString();
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