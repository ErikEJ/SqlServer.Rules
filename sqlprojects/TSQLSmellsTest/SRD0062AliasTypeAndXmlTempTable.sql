CREATE TYPE [dbo].[NameAlias] FROM NVARCHAR(128) NOT NULL;
GO
CREATE PROCEDURE [dbo].[SRD0062AliasTypeAndXmlTempTable]
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Names
    (
        [AliasName] [dbo].[NameAlias] NOT NULL,
        [XmlPayload] XML NOT NULL,
        [NameWithDatabaseDefault] NVARCHAR(128) COLLATE database_default NULL
    );
END;
