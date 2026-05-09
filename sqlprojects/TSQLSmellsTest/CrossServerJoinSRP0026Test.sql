CREATE PROCEDURE dbo.CrossServerJoinSRP0026Test
AS
SET NOCOUNT ON;
SELECT NAME FROM [$(TestServer)].DataBaseName.SchemaName.MyTable;

-- SRP0026
