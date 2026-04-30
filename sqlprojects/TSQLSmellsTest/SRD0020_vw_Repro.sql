CREATE VIEW [dbo].[vw_Repro] AS
SELECT
    tag.[Name] AS TagName
FROM STRING_SPLIT('1/2/3', '/') ss
LEFT JOIN [dbo].[Tag] tag ON tag.Gid = value;
