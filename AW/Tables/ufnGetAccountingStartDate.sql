
CREATE FUNCTION [dbo].[ufnGetAccountingStartDate]()
RETURNS [datetime] 
AS 
BEGIN
    RETURN CONVERT(datetime, '20030701', 112);
END;

GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Scalar function used in the uSalesOrderHeader trigger to set the ending account date.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'FUNCTION', @level1name = N'ufnGetAccountingStartDate';


GO

