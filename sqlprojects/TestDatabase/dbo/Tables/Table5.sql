CREATE TABLE [dbo].[Table5]
(
	[Table5_Id1] INT NOT NULL,
	[Table5_Id2] VARCHAR(100) NOT NULL,
	[Data] VARCHAR(1000) NULL, 
	[Data2] VARCHAR NULL, 
    CONSTRAINT [PK_Table5] PRIMARY KEY ([Table5_Id1], [Table5_Id2])
)
