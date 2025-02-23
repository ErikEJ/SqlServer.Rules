..\bin\Debug\net8.0\ErikEJ.TSQLAnalyzerCli.exe
pause

..\bin\Debug\net8.0\ErikEJ.TSQLAnalyzerCli.exe -i sproc.sql
pause

..\bin\Debug\net8.0\ErikEJ.TSQLAnalyzerCli.exe -i sproc.sql -o output.xml
pause

..\bin\Debug\net8.0\ErikEJ.TSQLAnalyzerCli.exe -i Chinook.dacpac
pause

..\bin\Debug\net8.0\ErikEJ.TSQLAnalyzerCli.exe -c "Data Source=.\SQLEXPRESS;Initial Catalog=Chinook;Integrated Security=True;Encrypt=false"
pause
