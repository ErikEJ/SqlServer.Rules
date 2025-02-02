CREATE TABLE [HumanResources].[Employee] (
    [BusinessEntityID]  INT                 NOT NULL,
    [NationalIDNumber]  NVARCHAR (15)       NOT NULL,
    [LoginID]           NVARCHAR (256)      NOT NULL,
    [OrganizationNode]  [sys].[hierarchyid] NULL,
    [OrganizationLevel] AS                  ([OrganizationNode].[GetLevel]()),
    [JobTitle]          NVARCHAR (50)       NOT NULL,
    [BirthDate]         DATE                NOT NULL,
    [MaritalStatus]     NCHAR (1)           NOT NULL,
    [Gender]            NCHAR (1)           NOT NULL,
    [HireDate]          DATE                NOT NULL,
    [SalariedFlag]      [dbo].[Flag]        CONSTRAINT [DF_Employee_SalariedFlag] DEFAULT ((1)) NOT NULL,
    [VacationHours]     SMALLINT            CONSTRAINT [DF_Employee_VacationHours] DEFAULT ((0)) NOT NULL,
    [SickLeaveHours]    SMALLINT            CONSTRAINT [DF_Employee_SickLeaveHours] DEFAULT ((0)) NOT NULL,
    [CurrentFlag]       [dbo].[Flag]        CONSTRAINT [DF_Employee_CurrentFlag] DEFAULT ((1)) NOT NULL,
    [rowguid]           UNIQUEIDENTIFIER    CONSTRAINT [DF_Employee_rowguid] DEFAULT (newid()) ROWGUIDCOL NOT NULL,
    [ModifiedDate]      DATETIME            CONSTRAINT [DF_Employee_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Employee_BusinessEntityID] PRIMARY KEY CLUSTERED ([BusinessEntityID] ASC),
    CONSTRAINT [CK_Employee_BirthDate] CHECK ([BirthDate]>='1930-01-01' AND [BirthDate]<=dateadd(year,(-18),getdate())),
    CONSTRAINT [CK_Employee_Gender] CHECK (upper([Gender])='F' OR upper([Gender])='M'),
    CONSTRAINT [CK_Employee_HireDate] CHECK ([HireDate]>='1996-07-01' AND [HireDate]<=dateadd(day,(1),getdate())),
    CONSTRAINT [CK_Employee_MaritalStatus] CHECK (upper([MaritalStatus])='S' OR upper([MaritalStatus])='M'),
    CONSTRAINT [CK_Employee_SickLeaveHours] CHECK ([SickLeaveHours]>=(0) AND [SickLeaveHours]<=(120)),
    CONSTRAINT [CK_Employee_VacationHours] CHECK ([VacationHours]>=(-40) AND [VacationHours]<=(240)),
    CONSTRAINT [FK_Employee_Person_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Person].[Person] ([BusinessEntityID])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'INDEX', @level2name = N'IX_Employee_OrganizationNode';


GO

CREATE NONCLUSTERED INDEX [IX_Employee_OrganizationNode]
    ON [HumanResources].[Employee]([OrganizationNode] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index. Used to support replication samples.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'INDEX', @level2name = N'AK_Employee_rowguid';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_Employee_rowguid]
    ON [HumanResources].[Employee]([rowguid] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'INDEX', @level2name = N'AK_Employee_LoginID';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_Employee_LoginID]
    ON [HumanResources].[Employee]([LoginID] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'INDEX', @level2name = N'AK_Employee_NationalIDNumber';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_Employee_NationalIDNumber]
    ON [HumanResources].[Employee]([NationalIDNumber] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'INDEX', @level2name = N'IX_Employee_OrganizationLevel_OrganizationNode';


GO

CREATE NONCLUSTERED INDEX [IX_Employee_OrganizationLevel_OrganizationNode]
    ON [HumanResources].[Employee]([OrganizationLevel] ASC, [OrganizationNode] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'INSTEAD OF DELETE trigger which keeps Employees from being deleted.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'TRIGGER', @level2name = N'dEmployee';


GO


CREATE TRIGGER [HumanResources].[dEmployee] ON [HumanResources].[Employee] 
INSTEAD OF DELETE NOT FOR REPLICATION AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN
        RAISERROR
            (N'Employees cannot be deleted. They can only be marked as not current.', -- Message
            10, -- Severity.
            1); -- State.

        -- Rollback any active or uncommittable transactions
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END
    END;
END;

GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [BirthDate] >= ''1930-01-01'' AND [BirthDate] <= dateadd(year,(-18),GETDATE())', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'CONSTRAINT', @level2name = N'CK_Employee_BirthDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Work title such as Buyer or Sales Representative.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'COLUMN', @level2name = N'JobTitle';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of NEWID()', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'CONSTRAINT', @level2name = N'DF_Employee_rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Number of available vacation hours.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'COLUMN', @level2name = N'VacationHours';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for Employee records.  Foreign key to BusinessEntity.BusinessEntityID.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'COLUMN', @level2name = N'BusinessEntityID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Number of available sick leave hours.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'COLUMN', @level2name = N'SickLeaveHours';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 1 (TRUE)', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'CONSTRAINT', @level2name = N'DF_Employee_SalariedFlag';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date of birth.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'COLUMN', @level2name = N'BirthDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [MaritalStatus]=''s'' OR [MaritalStatus]=''m'' OR [MaritalStatus]=''S'' OR [MaritalStatus]=''M''', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'CONSTRAINT', @level2name = N'CK_Employee_MaritalStatus';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 = Inactive, 1 = Active', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'COLUMN', @level2name = N'CurrentFlag';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Person.BusinessEntityID.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'CONSTRAINT', @level2name = N'FK_Employee_Person_BusinessEntityID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 1', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'CONSTRAINT', @level2name = N'DF_Employee_CurrentFlag';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'M = Married, S = Single', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'COLUMN', @level2name = N'MaritalStatus';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'COLUMN', @level2name = N'rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique national identification number such as a social security number.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'COLUMN', @level2name = N'NationalIDNumber';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'M = Male, F = Female', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'COLUMN', @level2name = N'Gender';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Network login.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'COLUMN', @level2name = N'LoginID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'CONSTRAINT', @level2name = N'DF_Employee_SickLeaveHours';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [Gender]=''f'' OR [Gender]=''m'' OR [Gender]=''F'' OR [Gender]=''M''', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'CONSTRAINT', @level2name = N'CK_Employee_Gender';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [SickLeaveHours] >= (0) AND [SickLeaveHours] <= (120)', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'CONSTRAINT', @level2name = N'CK_Employee_SickLeaveHours';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'CONSTRAINT', @level2name = N'DF_Employee_VacationHours';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Where the employee is located in corporate hierarchy.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'COLUMN', @level2name = N'OrganizationNode';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'CONSTRAINT', @level2name = N'DF_Employee_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Employee hired on this date.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'COLUMN', @level2name = N'HireDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [VacationHours] >= (-40) AND [VacationHours] <= (240)', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'CONSTRAINT', @level2name = N'CK_Employee_VacationHours';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Employee information such as salary, department, and title.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The depth of the employee in the corporate hierarchy.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'COLUMN', @level2name = N'OrganizationLevel';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [HireDate] >= ''1996-07-01'' AND [HireDate] <= dateadd(day,(1),GETDATE())', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'CONSTRAINT', @level2name = N'CK_Employee_HireDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Job classification. 0 = Hourly, not exempt from collective bargaining. 1 = Salaried, exempt from collective bargaining.', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'COLUMN', @level2name = N'SalariedFlag';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'HumanResources', @level1type = N'TABLE', @level1name = N'Employee', @level2type = N'CONSTRAINT', @level2name = N'PK_Employee_BusinessEntityID';


GO

