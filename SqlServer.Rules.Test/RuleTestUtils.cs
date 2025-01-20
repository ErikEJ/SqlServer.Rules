using System.IO;

namespace SqlServer.Rules.Test;

internal sealed class RuleTestUtils
{
    public static void SaveStringToFile(string contents, string filename)
    {
        FileStream fileStream = null;
        StreamWriter streamWriter = null;
        try
        {
            var directory = Path.GetDirectoryName(filename);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            fileStream = new FileStream(filename, FileMode.Create);
            streamWriter = new StreamWriter(fileStream);
            streamWriter.Write(contents);
        }
        finally
        {
            streamWriter?.Close();
            fileStream?.Close();
        }
    }

    public static string ReadFileToString(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        {
            return reader.ReadToEnd();
        }
    }
}