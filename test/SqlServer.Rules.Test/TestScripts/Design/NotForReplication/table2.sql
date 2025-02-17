CREATE TABLE [dbo].[table2] (
	[table2_id] INT IDENTITY (1000, 1) NOT NULL,
	[table1_id] INT NOT NULL,
	[data] VARCHAR(1000) NULL,
	[modified_date] DATETIME2(7) NOT NULL,
	CONSTRAINT [pk_table2] PRIMARY KEY CLUSTERED (table2_id),
	CONSTRAINT [CK_table2] CHECK (data IS NOT NULL)
)

GO