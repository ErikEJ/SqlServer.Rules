CREATE FUNCTION [dbo].[HasUniqueMTNameInRegion]
(
	@mtName char(50),
    @mtGroupId int
)
RETURNS bit
AS
BEGIN
    IF EXISTS (
        SELECT COUNT(*)
        FROM [dbo].[MTGroup] AS [mtg]
        INNER JOIN [dbo].[MTGroup] AS [mtgr] on [mtg].[RegionId] = [mtgr].[RegionId]
        INNER JOIN [dbo].[MT] as [mt] ON [mtgr].[MTGroupId] = [mt].[MTGroupId]
        WHERE [mtg].[MTGroupId] = @MTGroupId AND [mt].[Name] = @MTName
    )
    BEGIN RETURN 1
    END RETURN 0
END;
GO
