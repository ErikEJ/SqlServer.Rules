CREATE SCHEMA [Production]
    AUTHORIZATION [dbo];


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Contains objects related to products, inventory, and manufacturing.', @level0type = N'SCHEMA', @level0name = N'Production';


GO

