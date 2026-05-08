CREATE PROCEDURE [dbo].[OrderByInDerivedTableTest]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [dt].[Col1]
    FROM
    (
        SELECT TOP (10) [Col1]
        FROM [dbo].[TestTableSSDT]
        ORDER BY [Col1]
    ) AS [dt];
END;
RETURN 0;

-- SRD0091
