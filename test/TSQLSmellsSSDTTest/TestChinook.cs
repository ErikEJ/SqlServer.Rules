using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestChinook : TestModel
{
    [TestMethod]
    public void TestChinookDatabase()
    {
        foreach (var fileName in Directory.GetFiles("../../../../../sqlprojects/Chinook/Tables", "*.sql"))
        {
            TestFiles.Add(fileName);
        }

        RunTest();
    }
}
