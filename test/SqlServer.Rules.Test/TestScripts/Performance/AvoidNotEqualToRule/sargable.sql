CREATE VIEW [dbo].[sargable_view] AS
	SELECT table1_id FROM [dbo].[table1] WHERE [data] = 'this is what I''m doing'

GO