CREATE PROCEDURE dbo.GetOrdersByDateRange
    @StartDate DATE,
    @EndDate DATE
AS
SELECT *
FROM NorthWind2024.dbo.Orders
WHERE dbo.Orders.OrderDate BETWEEN @startDate AND @endDate
