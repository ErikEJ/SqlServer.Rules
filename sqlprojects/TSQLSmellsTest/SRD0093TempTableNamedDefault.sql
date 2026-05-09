CREATE PROCEDURE [dbo].[SRD0093TempTableNamedDefault]
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #NoName
    (
        cola INT DEFAULT (1)
    );

    CREATE TABLE #HasName
    (
        cola INT CONSTRAINT [ColaDef] DEFAULT (1)
    );
END;
RETURN 0;

-- SRD0093
