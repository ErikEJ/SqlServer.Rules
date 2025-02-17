
CREATE PROCEDURE [HumanResources].[uspUpdateEmployeePersonalInfo]
    @BusinessEntityID [int], 
    @NationalIDNumber [nvarchar](15), 
    @BirthDate [datetime], 
    @MaritalStatus [nchar](1), 
    @Gender [nchar](1)
WITH EXECUTE AS CALLER
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        UPDATE [HumanResources].[Employee] 
        SET [NationalIDNumber] = @NationalIDNumber 
            ,[BirthDate] = @BirthDate 
            ,[MaritalStatus] = @MaritalStatus 
            ,[Gender] = @Gender 
        WHERE [BusinessEntityID] = @BusinessEntityID;
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;

GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter a national ID for the employee.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'PROCEDURE', @level1name = N'uspUpdateEmployeePersonalInfo', @level2type = N'PARAMETER', @level2name = N'@NationalIDNumber';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter a marital status for the employee.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'PROCEDURE', @level1name = N'uspUpdateEmployeePersonalInfo', @level2type = N'PARAMETER', @level2name = N'@MaritalStatus';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Updates the Employee table with the values specified in the input parameters for the given EmployeeID.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'PROCEDURE', @level1name = N'uspUpdateEmployeePersonalInfo';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter a gender for the employee.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'PROCEDURE', @level1name = N'uspUpdateEmployeePersonalInfo', @level2type = N'PARAMETER', @level2name = N'@Gender';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Input parameter for the stored procedure uspUpdateEmployeeHireInfo. Enter a birth date for the employee.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'PROCEDURE', @level1name = N'uspUpdateEmployeePersonalInfo', @level2type = N'PARAMETER', @level2name = N'@BirthDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Input parameter for the stored procedure uspUpdateEmployeePersonalInfo. Enter a valid BusinessEntityID from the HumanResources.Employee table.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'PROCEDURE', @level1name = N'uspUpdateEmployeePersonalInfo', @level2type = N'PARAMETER', @level2name = N'@BusinessEntityID';


GO

