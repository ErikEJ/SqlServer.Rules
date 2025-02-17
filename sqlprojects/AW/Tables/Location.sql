CREATE TABLE [Production].[Location] (
    [LocationID]   SMALLINT       IDENTITY (1, 1) NOT NULL,
    [Name]         [dbo].[Name]   NOT NULL,
    [CostRate]     SMALLMONEY     CONSTRAINT [DF_Location_CostRate] DEFAULT ((0.00)) NOT NULL,
    [Availability] DECIMAL (8, 2) CONSTRAINT [DF_Location_Availability] DEFAULT ((0.00)) NOT NULL,
    [ModifiedDate] DATETIME       CONSTRAINT [DF_Location_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Location_LocationID] PRIMARY KEY CLUSTERED ([LocationID] ASC),
    CONSTRAINT [CK_Location_Availability] CHECK ([Availability]>=(0.00)),
    CONSTRAINT [CK_Location_CostRate] CHECK ([CostRate]>=(0.00))
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Location', @level2type = N'INDEX', @level2name = N'AK_Location_Name';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_Location_Name]
    ON [Production].[Location]([Name] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Location', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Location', @level2type = N'CONSTRAINT', @level2name = N'PK_Location_LocationID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [CostRate] >= (0.00)', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Location', @level2type = N'CONSTRAINT', @level2name = N'CK_Location_CostRate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Work capacity (in hours) of the manufacturing location.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Location', @level2type = N'COLUMN', @level2name = N'Availability';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0.0', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Location', @level2type = N'CONSTRAINT', @level2name = N'DF_Location_CostRate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [Availability] >= (0.00)', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Location', @level2type = N'CONSTRAINT', @level2name = N'CK_Location_Availability';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Standard hourly cost of the manufacturing location.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Location', @level2type = N'COLUMN', @level2name = N'CostRate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Location description.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Location', @level2type = N'COLUMN', @level2name = N'Name';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for Location records.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Location', @level2type = N'COLUMN', @level2name = N'LocationID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Product inventory and manufacturing locations.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Location';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Location', @level2type = N'CONSTRAINT', @level2name = N'DF_Location_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0.00', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'Location', @level2type = N'CONSTRAINT', @level2name = N'DF_Location_Availability';


GO

