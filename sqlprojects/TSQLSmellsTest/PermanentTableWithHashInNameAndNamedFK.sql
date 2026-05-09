CREATE PROCEDURE [dbo].[PermanentTableWithHashInNameAndNamedFK]
AS
SET NOCOUNT ON;

CREATE TABLE [dbo].[Parent#Table]
(
    ID INT PRIMARY KEY
);

CREATE TABLE [dbo].[Child#Table]
(
    ParentID INT CONSTRAINT [FK_ChildHash_ParentHash] FOREIGN KEY REFERENCES [dbo].[Parent#Table](ID)
);

RETURN 0;
