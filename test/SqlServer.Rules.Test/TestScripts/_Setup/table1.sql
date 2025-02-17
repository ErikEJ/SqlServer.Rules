CREATE TABLE [dbo].[table1] (
	[table1_id] INT IDENTITY (1000, 1) NOT NULL,
	[data] VARCHAR(1000) NULL,
	[modified_date] DATETIME2(7) NOT NULL,

	CONSTRAINT [pk_table1] PRIMARY KEY CLUSTERED (table1_id)
)

GO