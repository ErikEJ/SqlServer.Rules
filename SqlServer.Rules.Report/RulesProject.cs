using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SqlServer.Rules.Report;

[XmlType("Project")]
[Serializable]
public class RulesProject
{
    [XmlAttribute]
    public string Name { get; set; }

    [XmlElement(ElementName = "Issue")]
#pragma warning disable CA2227 // Collection properties should be read only
    public List<Issue> Issues { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
}