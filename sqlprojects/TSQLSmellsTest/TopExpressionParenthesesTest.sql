CREATE PROCEDURE [dbo].[TopExpressionParenthesesTest]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 100 Col1
    FROM [dbo].[TestTableSSDT]
    ORDER BY Col1;
END;
RETURN 0;

-- SRD0080
