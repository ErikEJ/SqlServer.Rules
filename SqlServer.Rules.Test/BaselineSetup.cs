using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Utils;

namespace SqlServer.Rules.Test;

internal sealed class BaselineSetup : RuleTest
{
    private const string TestScriptsFolder = "TestScripts";
    private const string SetupScriptsFolder = "_Setup";
    private const string Output = "Output";
    private const string Baseline = "Baseline";
    private const string SqlExtension = ".sql";

    public string ScriptsFolder { get; private set; }

    public string SetupFolder { get; private set; }

    public string OutputFilePath { get; private set; }

    public string BaselineFilePath { get; private set; }

    public BaselineSetup(TestContext testContext, string testName, TSqlModelOptions databaseOptions, SqlServerVersion sqlServerVersion = SqlServerVersion.Sql150)
        : base(new List<Tuple<string, string>>(), databaseOptions, sqlServerVersion)
    {
        var folder = Path.Combine(GetBaseFolder(), TestScriptsFolder);
        ScriptsFolder = Directory.EnumerateDirectories(folder, testName, SearchOption.AllDirectories).FirstOrDefault();
        Assert.IsTrue(Directory.Exists(ScriptsFolder), $"Expected the test folder '{ScriptsFolder}' to exist");

        SetupFolder = Path.Combine(GetBaseFolder(), TestScriptsFolder, SetupScriptsFolder);

        var outputDir = testContext.TestResultsDirectory;
        var outputFilename = $"{testName}-{Output}.txt";
        OutputFilePath = Path.Combine(outputDir, testName, outputFilename);

        var baselineFilename = $"{testName}-{Baseline}.txt";
        BaselineFilePath = Path.Combine(ScriptsFolder, baselineFilename);
    }

    private string GetBaseFolder()
    {
        var testAssemply = GetType().Assembly;

        return Path.GetDirectoryName(testAssemply.Location);
    }

    public override void RunTest(string fullId, Action<CodeAnalysisResult, string> verify)
    {
        LoadTestScripts(SetupFolder);
        LoadTestScripts(ScriptsFolder);
        base.RunTest(fullId, verify);
    }

    public void RunTest(string fullId)
    {
        LoadTestScripts(SetupFolder);
        LoadTestScripts(ScriptsFolder);
        base.RunTest(fullId, RunVerification);
    }

    private void LoadTestScripts(string folder)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        var directoryInfo = new DirectoryInfo(folder);

        var scriptFilePaths = from file in directoryInfo.GetFiles("*" + SqlExtension)
                              where SqlExtension.Equals(file.Extension, StringComparison.OrdinalIgnoreCase)
                              select file.FullName;

        foreach (var scriptFile in scriptFilePaths)
        {
            try
            {
                var contents = RuleTestUtils.ReadFileToString(scriptFile);
                TestScripts.Add(Tuple.Create(contents, Path.GetFileName(scriptFile)));
                Console.WriteLine($"Test file '{scriptFile}' loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from file {scriptFile} with message '{ex.Message}'");
                Console.WriteLine("Execution will continue...");
            }
        }
    }

    private void RunVerification(CodeAnalysisResult result, string resultsString)
    {
        var baseline = RuleTestUtils.ReadFileToString(BaselineFilePath);
        RuleTestUtils.SaveStringToFile(resultsString, OutputFilePath);

        var loadedTestScriptFiles = ListScriptFilenames();

        if (string.Equals(resultsString, baseline, StringComparison.OrdinalIgnoreCase))
        {
            var failureMessage = new StringBuilder();

            failureMessage.AppendLine($"The result is not the same as expected. Please compare actual output to baseline.");
            failureMessage.AppendLine(string.Empty);
            failureMessage.AppendLine($"### Loaded Test Script Files ###");
            failureMessage.AppendLine(loadedTestScriptFiles);
            failureMessage.AppendLine(string.Empty);
            failureMessage.AppendLine($"### View Baseline ###");
            failureMessage.AppendLine(BaselineFilePath);
            failureMessage.AppendLine(string.Empty);
            failureMessage.AppendLine($"### View Action Output ###");
            failureMessage.AppendLine(OutputFilePath);
            failureMessage.AppendLine(string.Empty);
            failureMessage.AppendLine($"### Test Folder ###");
            failureMessage.AppendLine(ScriptsFolder);

            Assert.Fail(failureMessage.ToString());
        }
    }

    private string ListScriptFilenames()
    {
        var loadedTestScriptFiles = new StringBuilder();

        foreach (var scriptInfo in TestScripts)
        {
            var scriptPath = scriptInfo.Item2;
            loadedTestScriptFiles.AppendLine(scriptPath);
        }

        return loadedTestScriptFiles.ToString();
    }
}
