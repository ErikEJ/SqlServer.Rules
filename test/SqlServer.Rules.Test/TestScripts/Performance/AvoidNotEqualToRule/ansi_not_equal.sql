CREATE VIEW [dbo].[ansi_not_equal] AS
	SELECT table1_id FROM [dbo].[table1] WHERE [data] <> 'what am I doing?'

GO