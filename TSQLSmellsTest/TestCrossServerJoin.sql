
CREATE PROCEDURE dbo.TestCrossServerJoin
AS
Set nocount on
SELECT NAME FROM [$(TestServer)].DataBaseName.SchemaName.MyTable

--SML001
