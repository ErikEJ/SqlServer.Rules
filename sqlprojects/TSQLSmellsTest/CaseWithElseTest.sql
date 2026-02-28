CREATE PROCEDURE [dbo].[TestCaseWithElse]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @status INT = 1;
    DECLARE @label NVARCHAR(50);

    SET @label = CASE @status
        WHEN 1 THEN 'Active'
        WHEN 2 THEN 'Inactive'
        ELSE 'Unknown'
    END;
END;
