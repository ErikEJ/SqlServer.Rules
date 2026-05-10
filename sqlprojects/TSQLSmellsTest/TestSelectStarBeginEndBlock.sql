
CREATE PROCEDURE dbo.TestSelectStarBeginEndBlock
as
set nocount on;
begin
	SELECT * FROM dbo.TestTableSSDT;
end;

-- SRD0067
