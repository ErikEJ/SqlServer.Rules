CREATE TABLE [Sales].[SalesOrderHeaderSalesReason] (
    [SalesOrderID]  INT      NOT NULL,
    [SalesReasonID] INT      NOT NULL,
    [ModifiedDate]  DATETIME CONSTRAINT [DF_SalesOrderHeaderSalesReason_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_SalesOrderHeaderSalesReason_SalesOrderID_SalesReasonID] PRIMARY KEY CLUSTERED ([SalesOrderID] ASC, [SalesReasonID] ASC),
    CONSTRAINT [FK_SalesOrderHeaderSalesReason_SalesOrderHeader_SalesOrderID] FOREIGN KEY ([SalesOrderID]) REFERENCES [Sales].[SalesOrderHeader] ([SalesOrderID]) ON DELETE CASCADE,
    CONSTRAINT [FK_SalesOrderHeaderSalesReason_SalesReason_SalesReasonID] FOREIGN KEY ([SalesReasonID]) REFERENCES [Sales].[SalesReason] ([SalesReasonID])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeaderSalesReason', @level2type = N'CONSTRAINT', @level2name = N'PK_SalesOrderHeaderSalesReason_SalesOrderID_SalesReasonID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key. Foreign key to SalesOrderHeader.SalesOrderID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeaderSalesReason', @level2type = N'COLUMN', @level2name = N'SalesOrderID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeaderSalesReason', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing SalesReason.SalesReasonID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeaderSalesReason', @level2type = N'CONSTRAINT', @level2name = N'FK_SalesOrderHeaderSalesReason_SalesReason_SalesReasonID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeaderSalesReason', @level2type = N'CONSTRAINT', @level2name = N'DF_SalesOrderHeaderSalesReason_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing SalesOrderHeader.SalesOrderID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeaderSalesReason', @level2type = N'CONSTRAINT', @level2name = N'FK_SalesOrderHeaderSalesReason_SalesOrderHeader_SalesOrderID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Cross-reference table mapping sales orders to sales reason codes.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeaderSalesReason';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key. Foreign key to SalesReason.SalesReasonID.', @level0type = N'SCHEMA', @level0name = N'Sales', @level1type = N'TABLE', @level1name = N'SalesOrderHeaderSalesReason', @level2type = N'COLUMN', @level2name = N'SalesReasonID';


GO

