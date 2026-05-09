CREATE PROCEDURE [dbo].[NamedPKOnTempTable]
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Good
    (
        ID INT PRIMARY KEY
    );

    CREATE TABLE #GoodTableLevel
    (
        ID INT,
        PRIMARY KEY (ID)
    );

    CREATE TABLE #Bad
    (
        ID INT,
        CONSTRAINT [PKID] PRIMARY KEY (ID)
    );
END;

-- SRD0092
