CREATE TABLE [Sales].[SalesReason] (
    [SalesReasonID] INT          IDENTITY (1, 1) NOT NULL,
    [Name]          [dbo].[Name] NOT NULL,
    [ReasonType]    [dbo].[Name] NOT NULL,
    [ModifiedDate]  DATETIME     CONSTRAINT [DF_SalesReason_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_SalesReason_SalesReasonID] PRIMARY KEY CLUSTERED ([SalesReasonID] ASC)
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesReason', @level2type = N'CONSTRAINT', @level2name = N'DF_SalesReason_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesReason', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Lookup table of customer purchase reasons.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesReason';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Category the sales reason belongs to.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesReason', @level2type = N'COLUMN', @level2name = N'ReasonType';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Sales reason description.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesReason', @level2type = N'COLUMN', @level2name = N'Name';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesReason', @level2type = N'CONSTRAINT', @level2name = N'PK_SalesReason_SalesReasonID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for SalesReason records.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesReason', @level2type = N'COLUMN', @level2name = N'SalesReasonID';


GO

