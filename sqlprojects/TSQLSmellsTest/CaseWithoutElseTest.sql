CREATE PROCEDURE [dbo].[TestCaseWithoutElse]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @status INT = 1;
    DECLARE @label NVARCHAR(50);

    SET @label = CASE @status
        WHEN 1 THEN 'Active'
        WHEN 2 THEN 'Inactive'
    END;

    DECLARE @category NVARCHAR(50);
    SET @category = CASE
        WHEN @status = 1 THEN 'Good'
        WHEN @status = 2 THEN 'Bad'
    END;
END;

-- SRD0071
