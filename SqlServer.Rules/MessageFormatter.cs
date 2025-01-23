namespace SqlServer.Rules
{
    internal static class MessageFormatter
    {
        public static string FormatMessage(string message, string ruleId)
        {
            // ruleId is in the format SqlServer.Rules.SRD0038
            var formattedId = ruleId.Replace("SqlServer.Rules.", string.Empty, System.StringComparison.Ordinal);

            var folderId = formattedId.Substring(2, 1);

            var folder = "Design";

            if (folderId == "P")
            {
                folder = "Performance";
            }
            else if (folderId == "N")
            {
                folder = "Naming";
            }

            return $"{message} (https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/{folder}/{formattedId}.md)";
        }
    }
}
