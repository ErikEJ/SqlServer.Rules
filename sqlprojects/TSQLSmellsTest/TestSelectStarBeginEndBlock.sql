
CREATE PROCEDURE dbo.TestSelectStarBeginEndBlock
as
set nocount on;
begin
	SELECT * FROM dbo.TestTableSSDT;
end;

-- SML005,SRD0067
