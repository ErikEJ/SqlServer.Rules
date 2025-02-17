CREATE PROCEDURE [dbo].[Procedure2]
	@param1 int = 0,
	@param2 int
AS
	

	BEGIN TRAN
	BEGIN TRY	
		UPDATE Table1 SET 
			[Table1Id] = @param1,
			FirstName = dbo.NoSchemaBinding(@param2)

	COMMIT TRAN
	END TRY
    BEGIN CATCH
		THROW	
	END CATCH

	UPDATE Table1 SET 
		[Table1Id] = @param1,
		FirstName = dbo.WithSchemaBinding(@param2)
	WHERE [Table1Id] NOT IN (1,2,3) 

	INSERT INTO dbo.Table1 
	( Table1Id, FirstName, LastName, Age )
	VALUES
	(	NULL, -- Id - uniqueidentifier
		'',	  -- FirstName - varchar(500)
		'',	  -- LastName - varchar(500)
		0	  -- Age - int
		)

	SET @param1 = @@IDENTITY

	SELECT @param1 = @@IDENTITY
RETURN 0
