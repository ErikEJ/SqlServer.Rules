CREATE VIEW [dbo].[nonsargable2_view] AS
	SELECT table1_id FROM [dbo].[table1] WHERE [data] LIKE '%what am I doing?%'

GO