CREATE PROCEDURE dbo.UpperFunction
AS
SET NOCOUNT ON;
SELECT user_name 
FROM dbo.MyTable
WHERE UPPER(first_name) = 'NATHAN';

-- SRP0009
