
CREATE FUNCTION [dbo].[ufnGetPurchaseOrderStatusText](@Status [tinyint])
RETURNS [nvarchar](15) 
AS 
-- Returns the sales order status text representation for the status value.
BEGIN
    DECLARE @ret [nvarchar](15);

    SET @ret = 
        CASE @Status
            WHEN 1 THEN 'Pending'
            WHEN 2 THEN 'Approved'
            WHEN 3 THEN 'Rejected'
            WHEN 4 THEN 'Complete'
            ELSE '** Invalid **'
        END;
    
    RETURN @ret
END;

GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Input parameter for the scalar function ufnGetPurchaseOrdertStatusText. Enter a valid integer.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'FUNCTION', @level1name = N'ufnGetPurchaseOrderStatusText', @level2type = N'PARAMETER', @level2name = N'@Status';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Scalar function returning the text representation of the Status column in the PurchaseOrderHeader table.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'FUNCTION', @level1name = N'ufnGetPurchaseOrderStatusText';


GO

