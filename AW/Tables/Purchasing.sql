CREATE SCHEMA [Purchasing]
    AUTHORIZATION [dbo];


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Contains objects related to vendors and purchase orders.', @level0type = N'SCHEMA', @level0name = N'Purchasing';


GO

