CREATE PROCEDURE [dbo].[TempTableWithTableLevelNamedFKConstraint]
AS
SET NOCOUNT ON;

CREATE TABLE #Parent
(
    ID INT PRIMARY KEY
);

CREATE TABLE #TempWithNamedFKTableLevel
(
    ParentID INT,
    CONSTRAINT [FK_TempWithNamedFKTableLevel_Parent] FOREIGN KEY (ParentID) REFERENCES #Parent(ID)
);

RETURN 0;
