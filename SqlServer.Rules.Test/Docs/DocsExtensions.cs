using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using LoxSmoke.DocXml;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Design;
using TSQLSmellSCA;

namespace SqlServer.Rules.Tests.Docs;

public static class DocsExtensions
{
    public static string ToSentence(this string input)
    {
        var parts = Regex.Split(input, @"([A-Z]?[a-z]+)").Where(str => !string.IsNullOrEmpty(str));
        return string.Join(' ', parts);
    }

    public static string ToId(this string input)
    {
        return new string(input.Split('.').Last());
    }
}