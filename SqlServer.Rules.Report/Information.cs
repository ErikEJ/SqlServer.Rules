using System;

namespace SqlServer.Rules.Report;

[Serializable]
public class Information
{
    public string Solution { get; set; }

    public DateTime ReportDate { get; set; } = DateTime.Now;

    public InspectionScope InspectionScope { get; set; }

    public Information()
    {
        InspectionScope = new InspectionScope();
    }
}