CREATE TABLE [Purchasing].[ShipMethod] (
    [ShipMethodID] INT              IDENTITY (1, 1) NOT NULL,
    [Name]         [dbo].[Name]     NOT NULL,
    [ShipBase]     MONEY            CONSTRAINT [DF_ShipMethod_ShipBase] DEFAULT ((0.00)) NOT NULL,
    [ShipRate]     MONEY            CONSTRAINT [DF_ShipMethod_ShipRate] DEFAULT ((0.00)) NOT NULL,
    [rowguid]      UNIQUEIDENTIFIER CONSTRAINT [DF_ShipMethod_rowguid] DEFAULT (newid()) ROWGUIDCOL NOT NULL,
    [ModifiedDate] DATETIME         CONSTRAINT [DF_ShipMethod_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ShipMethod_ShipMethodID] PRIMARY KEY CLUSTERED ([ShipMethodID] ASC),
    CONSTRAINT [CK_ShipMethod_ShipBase] CHECK ([ShipBase]>(0.00)),
    CONSTRAINT [CK_ShipMethod_ShipRate] CHECK ([ShipRate]>(0.00))
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index. Used to support replication samples.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ShipMethod', @level2type = N'INDEX', @level2name = N'AK_ShipMethod_rowguid';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_ShipMethod_rowguid]
    ON [Purchasing].[ShipMethod]([rowguid] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ShipMethod', @level2type = N'INDEX', @level2name = N'AK_ShipMethod_Name';


GO

CREATE UNIQUE NONCLUSTERED INDEX [AK_ShipMethod_Name]
    ON [Purchasing].[ShipMethod]([Name] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Minimum shipping charge.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ShipMethod', @level2type = N'COLUMN', @level2name = N'ShipBase';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Shipping company name.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ShipMethod', @level2type = N'COLUMN', @level2name = N'Name';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Shipping charge per pound.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ShipMethod', @level2type = N'COLUMN', @level2name = N'ShipRate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ShipMethod', @level2type = N'CONSTRAINT', @level2name = N'PK_ShipMethod_ShipMethodID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ShipMethod', @level2type = N'COLUMN', @level2name = N'rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ShipMethod', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of NEWID()', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ShipMethod', @level2type = N'CONSTRAINT', @level2name = N'DF_ShipMethod_rowguid';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [ShipRate] > (0.00)', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ShipMethod', @level2type = N'CONSTRAINT', @level2name = N'CK_ShipMethod_ShipRate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0.0', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ShipMethod', @level2type = N'CONSTRAINT', @level2name = N'DF_ShipMethod_ShipRate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ShipMethod', @level2type = N'CONSTRAINT', @level2name = N'DF_ShipMethod_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [ShipBase] > (0.00)', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ShipMethod', @level2type = N'CONSTRAINT', @level2name = N'CK_ShipMethod_ShipBase';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Shipping company lookup table.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ShipMethod';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0.0', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ShipMethod', @level2type = N'CONSTRAINT', @level2name = N'DF_ShipMethod_ShipBase';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for ShipMethod records.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'ShipMethod', @level2type = N'COLUMN', @level2name = N'ShipMethodID';


GO

