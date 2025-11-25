using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Dac;
using SqlServer.Rules.Report.Properties;

namespace SqlServer.Rules.Report;

public class ReportFactory
{
    public delegate void NotifyHandler(string notificationMessage, NotificationType type);

#pragma warning disable CA1003 // Use generic event handler instances
    public event NotifyHandler Notify;
#pragma warning restore CA1003 // Use generic event handler instances

    public void Create(ReportRequest request)
    {
        var fileName = Path.GetFileNameWithoutExtension(request.InputPath);

        SendNotification($"Loading {request.FileName}.dacpac");
        var sw = Stopwatch.StartNew();

        // load the dacpac
        var model = TSqlModel.LoadFromDacpac(
                request.InputPath,
                new ModelLoadOptions
                {
                    LoadAsScriptBackedModel = true,
                    ModelStorageType = Microsoft.SqlServer.Dac.DacSchemaModelStorageType.Memory,
                });
        var factory = new CodeAnalysisServiceFactory();
        var service = factory.CreateAnalysisService(model);

        // surpress rules
        service.SetProblemSuppressor(request.Suppress);
        sw.Stop();
        SendNotification($"Loading {request.FileName}.dacpac complete, elapsed: {sw.Elapsed.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)}");

        SendNotification("Running rules");
        sw = Stopwatch.StartNew();

        // process non-suppressed rules
        var result = service.Analyze(model);

        if (!result.AnalysisSucceeded)
        {
            foreach (var err in result.InitializationErrors)
            {
                SendNotification(err.Message, NotificationType.Error);
            }

            foreach (var err in result.SuppressionErrors)
            {
                SendNotification(err.Message, NotificationType.Error);
            }

            foreach (var err in result.AnalysisErrors)
            {
                SendNotification(err.Message, NotificationType.Error);
            }

            return;
        }

        foreach (var err in result.Problems)
        {
            SendNotification(err.ErrorMessageString, NotificationType.Warning);
        }

        result.SerializeResultsToXml(GetOutputFileName(request, ReportOutputType.HTML));
        sw.Stop();
        SendNotification($"Running rules complete, elapsed: {sw.Elapsed.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)}");

        // create report object
        var report = new ReportEntity(
            request.Solution,
            GetIssueTypes(service.GetRules(), request.SuppressIssueTypes).ToList(),
            request.FileName,
            GetProblems(result.Problems).ToList());

        SendNotification("Writing report");
        sw = Stopwatch.StartNew();

        // write out the xml
        switch (request.ReportOutputType)
        {
            case ReportOutputType.HTML:
                var outFileName = GetOutputFileName(request, request.ReportOutputType);
                SerializeReport(report, outFileName);
                var outDir = GetOutputDirectory(request);
                var xlstPath = Path.Combine(outDir, "RulesTransform.xslt");
                if (!File.Exists(xlstPath))
                {
                    File.WriteAllText(xlstPath, Resources.RulesTransform);
                }

#pragma warning disable CA5372 // Use XmlReader for XPathDocument constructor
                var xPathDoc = new XPathDocument(outFileName);
#pragma warning restore CA5372 // Use XmlReader for XPathDocument constructor
                var xslTransform = new XslCompiledTransform();
                using (var xmlWriter = new XmlTextWriter(Path.Combine(outDir, $"{request.FileName}.html"), null))
                {
                    xslTransform.Load(xlstPath);
                    xslTransform.Transform(xPathDoc, null, xmlWriter);
                }

                break;
            case ReportOutputType.CSV:
                SerializeReportToCSV(report, GetOutputFileName(request, request.ReportOutputType));
                break;
            default:
                SendNotification($"Invalid report type: {request.ReportOutputType}");
                break;
        }

        sw.Stop();
        SendNotification($"Writing report complete, elapsed: {sw.Elapsed.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)}");

        SendNotification($"Done with {request.FileName}.dacpac");
    }

    private static string GetOutputDirectory(ReportRequest request)
    {
        var currentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var outDir = string.IsNullOrWhiteSpace(request.OutputDirectory) ? currentDir : request.OutputDirectory;

        // not sure where this " is coming from, but it throws an exception trying to use the path
        outDir = outDir!.Replace("\"", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"OUT DIRECTORY: ' {outDir} '");
        Console.ResetColor();

        if (!Path.IsPathRooted(outDir))
        {
            outDir = Path.Combine(currentDir!, outDir);
        }

        return outDir;
    }

    private static string GetOutputFileName(ReportRequest request, ReportOutputType outputType)
    {
        var ext = outputType == ReportOutputType.HTML ? ".xml" : ".csv";
        var outDir = GetOutputDirectory(request);
        if (!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }

        return Path.Combine(outDir, Path.GetFileNameWithoutExtension(request.OutputFileName) + ext);
    }

    private void SendNotification(string message, NotificationType type = NotificationType.Information)
    {
        Notify?.Invoke(message, type);
    }

    private static IEnumerable<IssueType> GetIssueTypes(IList<RuleDescriptor> rules, Func<RuleDescriptor, bool> suppressIssueTypes)
    {
        return (from r in rules
                where suppressIssueTypes == null ? true : !suppressIssueTypes.Invoke(r)
                select new IssueType
                {
                    Severity = r.Severity.ToString(),
                    Description = r.DisplayDescription,
                    Category = $"{r.Namespace}.{r.Metadata.Category}", // as we are including msft rules now too, we need to include the namespace in the category
                    Id = r.ShortRuleId,
                }).Distinct(new IssueTypeComparer());
    }

    private static IEnumerable<Issue> GetProblems(IEnumerable<SqlRuleProblem> problems)
    {
        return from p in problems
               select new Issue
               {
                   File = !string.IsNullOrWhiteSpace(p.SourceName) ? p.SourceName : p.ModelElement.Name.GetName(),
                   Line = p.StartLine,
                   Message = p.Description, // p.ErrorMessageString,
                   Offset = p.StartColumn.ToString(CultureInfo.InvariantCulture),
                   TypeId = p.Rule(),
               };
    }

    private static void SerializeReport(ReportEntity report, string outputPath)
    {
        var serializer = new XmlSerializer(typeof(ReportEntity));
        var ns = new XmlSerializerNamespaces([new XmlQualifiedName(string.Empty, string.Empty)]);
        var xmlSettings = new XmlWriterSettings
        {            
            Indent = true,
            IndentChars = "\t",
            OmitXmlDeclaration = true,
        };
        using (var writer = XmlWriter.Create(outputPath, xmlSettings))
        {
            writer.WriteProcessingInstruction("xml-stylesheet", "type='text/xsl' href='RulesTransform.xslt'");
            serializer.Serialize(writer, report, ns);
            writer.Close();
        }
    }

    private static void SerializeReportToCSV(ReportEntity report, string outputPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Issue Id,Message,Line/Offset,File Name");
        foreach (var line in report.Issues)
        {
            foreach (var issue in line.Issues)
            {
#pragma warning disable CA1305 // Specify IFormatProvider
                sb.AppendLine($"\"{issue.TypeId}\",\"{issue.Message}\",\"{issue.Line}/{issue.Offset}\",\"{issue.File}\"");
#pragma warning restore CA1305 // Specify IFormatProvider
            }
        }

        File.WriteAllText(outputPath, sb.ToString());

        // var issuesMap = new ColumnInfoList<Issue>();
        // issuesMap.Add("A", "Issue Id", (obj) => obj.TypeId, updateHeader: true);
        // issuesMap.Add("B", "Message", (obj) => obj.Message, updateHeader: true);
        // issuesMap.Add("C", "Line,Offset", (obj) => $"{obj.Line},{obj.Offset}", updateHeader: true);
        // issuesMap.Add("D", "File Name", (obj) => obj.File, updateHeader: true);

        // using (ExcelWriter writer = File.Exists(outputPath) ? new ExcelWriter(outputPath, 1, 2) : new ExcelWriter(1, 2))
        // {
        //    writer.CreateSheetIfNotFound = true;
        //    foreach (var issue in report.Issues)
        //    {
        //        SendNotification($"Writing sheet: {issue.Name}, with {issue.Issues.Count.ToString("N0")} issues");
        //        writer.WriteDataToSheet(issue.Name, issue.Issues, issuesMap);
        //    }

        // writer.WriteTo(outputPath);
        // }
    }
}
