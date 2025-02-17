CREATE VIEW [dbo].[nonsargable_view] AS
	SELECT table1_id FROM [dbo].[table1] WHERE [data] LIKE '%what am I doing?'

GO