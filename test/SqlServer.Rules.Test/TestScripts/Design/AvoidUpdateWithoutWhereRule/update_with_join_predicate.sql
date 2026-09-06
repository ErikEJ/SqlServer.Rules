CREATE PROCEDURE [dbo].[update_with_join_predicate]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @someTable TABLE (
        [table1_id] INT,
        [data] VARCHAR(1000)
    );

    UPDATE dst
    SET [data] = bodyItems.[data]
    FROM [dbo].[table1] dst
    INNER JOIN @someTable bodyItems
        ON bodyItems.[table1_id] = dst.[table1_id];
END
GO
