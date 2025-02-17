CREATE SCHEMA [HumanResources]
    AUTHORIZATION [dbo];


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Contains objects related to employees and departments.', @level0type = N'SCHEMA', @level0name = N'HumanResources';


GO

