SELECT OrderID, CustomerIdentifier, EmployeeID, FORMAT(OrderDate, 'MM/dd/yyyy') AS OrderDate, ShipCountry FROM NorthWind2024.dbo.Orders
WHERE dbo.Orders.OrderDate BETWEEN '07/08/2014'  '07/15/2014'