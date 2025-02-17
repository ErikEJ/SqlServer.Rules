Create Procedure dbo.CursorTest
as
Set nocount on;

DECLARE vendor_cursor CURSOR FOR 
SELECT Col1, Col2
FROM [dbo].[TestTableSSDT]
WHERE Col3=1
ORDER BY Col1;

-- SML029, SRP0016
