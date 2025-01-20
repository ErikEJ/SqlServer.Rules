using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SqlServer.Rules.Report;


public class IssueTypeComparer : IEqualityComparer<IssueType>
{
    public bool Equals(IssueType x, IssueType y)
    {
        return x.Id == y.Id;
    }

    public int GetHashCode(IssueType obj)
    {
        return obj.Id.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}