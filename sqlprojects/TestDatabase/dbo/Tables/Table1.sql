CREATE TABLE [dbo].[Table1]
(
	[Table1Id] UNIQUEIDENTIFIER NOT NULL, 
    [Table1Id2] INT NOT NULL IDENTITY(1000, 1), 
    [FirstName] VARCHAR(500) NOT NULL, 
    [LastName] VARCHAR(500) NOT NULL CONSTRAINT [DF_LastName] DEFAULT (''), 
    [Age] INT NOT NULL, 
    [City] VARCHAR(50) NULL, 
    [State] TEXT NULL, 
    [Zip] VARCHAR(12) NULL, 
    CONSTRAINT [PK_Table1_Foo] PRIMARY KEY ([Table1Id], [Table1Id2]) 
)

GO

CREATE TRIGGER [dbo].[trg_deleted]
    ON [dbo].[Table1]
    FOR DELETE
    AS
    BEGIN
        SELECT * FROM Deleted
    END
GO

CREATE TRIGGER [dbo].[trg_upsert]
    ON [dbo].[Table1]
    FOR INSERT, UPDATE
    AS
    BEGIN
		INSERT INTO table1_H (
			[Table1Id], 
			[Table1Id2], 
			[FirstName], 
			[LastName], 
			[Age], 
			[City], 
			[State], 
			[EnteredDate]
		)
        SELECT 
			[Table1Id], 
			[Table1Id2], 
			[FirstName], 
			[LastName], 
			[Age], 
			[City], 
			[State], 
			GETDATE()
		FROM Inserted
    END
GO

CREATE NONCLUSTERED INDEX [IX01_Table1] ON [dbo].[Table1] (FirstName) INCLUDE (City, State)
GO
CREATE NONCLUSTERED INDEX [IX02_Table1] ON [dbo].[Table1] (FirstName, LastName)
GO
CREATE NONCLUSTERED INDEX [IX03_Table1] ON [dbo].[Table1] (FirstName, LastName, Age)
GO
CREATE NONCLUSTERED INDEX [IX04_Table1] ON [dbo].[Table1] (Age, FirstName, LastName)
GO
CREATE NONCLUSTERED INDEX [IX05_Table1] ON [dbo].[Table1] (LastName, Age, FirstName)
GO
CREATE NONCLUSTERED INDEX [IX06_Table1] ON [dbo].[Table1] (FirstName, LastName, Age)
GO

CREATE NONCLUSTERED INDEX [IX07_Table1] ON [dbo].[Table1] (FirstName)
GO
CREATE NONCLUSTERED INDEX [IX08_Table1] ON [dbo].[Table1] (FirstName, LastName)
GO
CREATE NONCLUSTERED INDEX [IX09_Table1] ON [dbo].[Table1] (FirstName, LastName, Age)
GO
CREATE NONCLUSTERED INDEX [IX10_Table1] ON [dbo].[Table1] (Age, FirstName, LastName)
GO
CREATE NONCLUSTERED INDEX [IX11_Table1] ON [dbo].[Table1] (LastName, Age, FirstName)
GO
CREATE NONCLUSTERED INDEX [IX12_Table1] ON [dbo].[Table1] (FirstName, LastName, Age)
GO

CREATE NONCLUSTERED INDEX [IX13_Table1] ON [dbo].[Table1] (FirstName)
GO
CREATE NONCLUSTERED INDEX [IX14_Table1] ON [dbo].[Table1] (FirstName, LastName)
GO
CREATE NONCLUSTERED INDEX [IX15_Table1] ON [dbo].[Table1] (FirstName, LastName, Age)
GO
CREATE NONCLUSTERED INDEX [IX16_Table1] ON [dbo].[Table1] (Age, FirstName, LastName)
GO
CREATE NONCLUSTERED INDEX [IX17_Table1] ON [dbo].[Table1] (LastName, Age, FirstName)
GO
CREATE NONCLUSTERED INDEX [IX18_Table1] ON [dbo].[Table1] (FirstName, LastName, Age)
GO
CREATE NONCLUSTERED INDEX [IX19_Table1] ON [dbo].[Table1] (City, State)
GO
CREATE NONCLUSTERED INDEX [IX20_Table1] ON [dbo].[Table1] (city, Age)
GO

