CREATE TABLE [dbo].[Table1_H]
(
	[Table1Id] UNIQUEIDENTIFIER NOT NULL, 
    [Table1Id2] INT NOT NULL IDENTITY(1000, 1), 
    [FirstName] VARCHAR(500) NOT NULL, 
    [LastName] VARCHAR(500) NOT NULL CONSTRAINT [DF_Table1_H_LastName] DEFAULT (''), 
    [Age] INT NOT NULL, 
    [City] VARCHAR(50) NULL, 
    [State] TEXT NULL, 
	[EnteredDate] DATETIME NOT NULL CONSTRAINT [DF_Table1_H_EnteredDate] DEFAULT (GETDATE()),
    CONSTRAINT [PK_Table1_H_Foo] PRIMARY KEY ([Table1Id], [Table1Id2]) 
)
