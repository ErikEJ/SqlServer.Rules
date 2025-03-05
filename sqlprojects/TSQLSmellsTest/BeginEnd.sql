CREATE PROCEDURE dbo.BeginEnd
    @parm INT
AS
SET NOCOUNT ON;

IF(@parm = 1)
  SELECT 'foo'
ELSE
  SELECT 'fix'

IF (@parm = 2)
BEGIN
  SELECT 'bar'
END

-- SRD0066
