
CREATE FUNCTION [dbo].[ufnGetAccountingEndDate]()
RETURNS [datetime] 
AS 
BEGIN
    RETURN DATEADD(millisecond, -2, CONVERT(datetime, '20040701', 112));
END;

GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Scalar function used in the uSalesOrderHeader trigger to set the starting account date.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'FUNCTION', @level1name = N'ufnGetAccountingEndDate';


GO

