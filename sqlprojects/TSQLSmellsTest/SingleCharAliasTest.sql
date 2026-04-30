CREATE PROCEDURE [dbo].[SingleCharAliasTest]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.Col1
    FROM (SELECT Col1 FROM [dbo].[TestTableSSDT]) AS a;
END;
RETURN 0;

-- SRD0078
