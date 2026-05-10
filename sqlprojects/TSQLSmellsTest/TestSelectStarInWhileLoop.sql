CREATE PROCEDURE dbo.TestSelectStarInWhileLoop
AS
SET NOCOUNT on
WHILE(0=0) begin
	SELECT * FROM dbo.TestTableSSDT
end
