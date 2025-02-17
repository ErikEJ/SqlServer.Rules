using System;

namespace SqlServer.Rules.Report;

[Serializable]
public class InspectionScope
{
    public string Element { get; set; }

    public InspectionScope()
    {
        Element = "Project";
    }
}