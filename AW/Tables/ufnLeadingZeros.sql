
CREATE FUNCTION [dbo].[ufnLeadingZeros](
    @Value int
) 
RETURNS varchar(8) 
WITH SCHEMABINDING 
AS 
BEGIN
    DECLARE @ReturnValue varchar(8);

    SET @ReturnValue = CONVERT(varchar(8), @Value);
    SET @ReturnValue = REPLICATE('0', 8 - DATALENGTH(@ReturnValue)) + @ReturnValue;

    RETURN (@ReturnValue);
END;

GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Input parameter for the scalar function ufnLeadingZeros. Enter a valid integer.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'FUNCTION', @level1name = N'ufnLeadingZeros', @level2type = N'PARAMETER', @level2name = N'@Value';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Scalar function used by the Sales.Customer table to help set the account number.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'FUNCTION', @level1name = N'ufnLeadingZeros';


GO

