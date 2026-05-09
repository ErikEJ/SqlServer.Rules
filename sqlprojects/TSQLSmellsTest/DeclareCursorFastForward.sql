Create Procedure dbo.CursorFastForwardTest
as
Set nocount on;

DECLARE vendor_cursor CURSOR FAST_FORWARD FOR 
SELECT Col1, Col2
FROM [dbo].[TestTableSSDT]
WHERE Col3=1
ORDER BY Col1;
