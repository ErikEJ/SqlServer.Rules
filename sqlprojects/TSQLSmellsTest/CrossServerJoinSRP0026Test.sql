CREATE PROCEDURE dbo.CrossServerJoinSRP0026Test
AS
Set nocount on
SELECT NAME FROM [$(TestServer)].DataBaseName.SchemaName.MyTable

-- SRP0026
