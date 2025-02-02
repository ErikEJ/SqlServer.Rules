CREATE TABLE [HumanResources].[Department] (
    [DepartmentID] SMALLINT     IDENTITY (1, 1) NOT NULL,
    [Name]         [dbo].[Name] NOT NULL,
    [GroupName]    [dbo].[Name] NOT NULL,
    [ModifiedDate] DATETIME     CONSTRAINT [DF_Department_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Department_DepartmentID] PRIMARY KEY CLUSTERED ([DepartmentID] ASC)
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Department', @level2type = N'INDEX', @level2name = N'AK_Department_Name';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_Department_Name]
    ON [HumanResources].[Department]([Name] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for Department records.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Department', @level2type = N'COLUMN', @level2name = N'DepartmentID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Department', @level2type = N'CONSTRAINT', @level2name = N'DF_Department_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Name of the department.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Department', @level2type = N'COLUMN', @level2name = N'Name';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Lookup table containing the departments within the Adventure Works Cycles company.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Department';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Name of the group to which the department belongs.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Department', @level2type = N'COLUMN', @level2name = N'GroupName';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Department', @level2type = N'CONSTRAINT', @level2name = N'PK_Department_DepartmentID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Department', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

