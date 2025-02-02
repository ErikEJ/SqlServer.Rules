


CREATE FUNCTION [dbo].[ufnGetProductDealerPrice](@ProductID [int], @OrderDate [datetime])
RETURNS [money] 
AS 
-- Returns the dealer price for the product on a specific date.
BEGIN
    DECLARE @DealerPrice money;
    DECLARE @DealerDiscount money;

    SET @DealerDiscount = 0.60  -- 60% of list price

    SELECT @DealerPrice = plph.[ListPrice] * @DealerDiscount 
    FROM [Production].[Product] p 
        INNER JOIN [Production].[ProductListPriceHistory] plph 
        ON p.[ProductID] = plph.[ProductID] 
            AND p.[ProductID] = @ProductID 
            AND @OrderDate BETWEEN plph.[StartDate] AND COALESCE(plph.[EndDate], CONVERT(datetime, '99991231', 112)); -- Make sure we get all the prices!

    RETURN @DealerPrice;
END;

GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Scalar function returning the dealer price for a given product on a particular order date.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'FUNCTION', @level1name = N'ufnGetProductDealerPrice';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Input parameter for the scalar function ufnGetProductDealerPrice. Enter a valid order date.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'FUNCTION', @level1name = N'ufnGetProductDealerPrice', @level2type = N'PARAMETER', @level2name = N'@OrderDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Input parameter for the scalar function ufnGetProductDealerPrice. Enter a valid ProductID from the Production.Product table.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'FUNCTION', @level1name = N'ufnGetProductDealerPrice', @level2type = N'PARAMETER', @level2name = N'@ProductID';


GO

