CREATE SCHEMA [Person]
    AUTHORIZATION [dbo];


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Contains objects related to names and addresses of customers, vendors, and employees', @level0type = N'SCHEMA', @level0name = N'Person';


GO

