CREATE VIEW [dbo].[vw_Repro] AS
SELECT
    t.[Name] AS TagName
FROM STRING_SPLIT('1/2/3', '/') ss
LEFT JOIN [dbo].[Tag] t ON t.Gid = value;
