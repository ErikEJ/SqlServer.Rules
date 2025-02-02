CREATE TABLE [Purchasing].[PurchaseOrderHeader] (
    [PurchaseOrderID] INT      IDENTITY (1, 1) NOT NULL,
    [RevisionNumber]  TINYINT  CONSTRAINT [DF_PurchaseOrderHeader_RevisionNumber] DEFAULT ((0)) NOT NULL,
    [Status]          TINYINT  CONSTRAINT [DF_PurchaseOrderHeader_Status] DEFAULT ((1)) NOT NULL,
    [EmployeeID]      INT      NOT NULL,
    [VendorID]        INT      NOT NULL,
    [ShipMethodID]    INT      NOT NULL,
    [OrderDate]       DATETIME CONSTRAINT [DF_PurchaseOrderHeader_OrderDate] DEFAULT (getdate()) NOT NULL,
    [ShipDate]        DATETIME NULL,
    [SubTotal]        MONEY    CONSTRAINT [DF_PurchaseOrderHeader_SubTotal] DEFAULT ((0.00)) NOT NULL,
    [TaxAmt]          MONEY    CONSTRAINT [DF_PurchaseOrderHeader_TaxAmt] DEFAULT ((0.00)) NOT NULL,
    [Freight]         MONEY    CONSTRAINT [DF_PurchaseOrderHeader_Freight] DEFAULT ((0.00)) NOT NULL,
    [TotalDue]        AS       (isnull(([SubTotal]+[TaxAmt])+[Freight],(0))) PERSISTED NOT NULL,
    [ModifiedDate]    DATETIME CONSTRAINT [DF_PurchaseOrderHeader_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_PurchaseOrderHeader_PurchaseOrderID] PRIMARY KEY CLUSTERED ([PurchaseOrderID] ASC),
    CONSTRAINT [CK_PurchaseOrderHeader_Freight] CHECK ([Freight]>=(0.00)),
    CONSTRAINT [CK_PurchaseOrderHeader_ShipDate] CHECK ([ShipDate]>=[OrderDate] OR [ShipDate] IS NULL),
    CONSTRAINT [CK_PurchaseOrderHeader_Status] CHECK ([Status]>=(1) AND [Status]<=(4)),
    CONSTRAINT [CK_PurchaseOrderHeader_SubTotal] CHECK ([SubTotal]>=(0.00)),
    CONSTRAINT [CK_PurchaseOrderHeader_TaxAmt] CHECK ([TaxAmt]>=(0.00)),
    CONSTRAINT [FK_PurchaseOrderHeader_Employee_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES [HumanResources].[Employee] ([BusinessEntityID]),
    CONSTRAINT [FK_PurchaseOrderHeader_ShipMethod_ShipMethodID] FOREIGN KEY ([ShipMethodID]) REFERENCES [Purchasing].[ShipMethod] ([ShipMethodID]),
    CONSTRAINT [FK_PurchaseOrderHeader_Vendor_VendorID] FOREIGN KEY ([VendorID]) REFERENCES [Purchasing].[Vendor] ([BusinessEntityID])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'INDEX', @level2name = N'IX_PurchaseOrderHeader_EmployeeID';


GO

CREATE NONCLUSTERED INDEX [IX_PurchaseOrderHeader_EmployeeID]
    ON [Purchasing].[PurchaseOrderHeader]([EmployeeID] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'INDEX', @level2name = N'IX_PurchaseOrderHeader_VendorID';


GO

CREATE NONCLUSTERED INDEX [IX_PurchaseOrderHeader_VendorID]
    ON [Purchasing].[PurchaseOrderHeader]([VendorID] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'AFTER UPDATE trigger that updates the RevisionNumber and ModifiedDate columns in the PurchaseOrderHeader table.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'TRIGGER', @level2name = N'uPurchaseOrderHeader';


GO


CREATE TRIGGER [Purchasing].[uPurchaseOrderHeader] ON [Purchasing].[PurchaseOrderHeader] 
AFTER UPDATE AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN TRY
        -- Update RevisionNumber for modification of any field EXCEPT the Status.
        IF NOT UPDATE([Status])
        BEGIN
            UPDATE [Purchasing].[PurchaseOrderHeader]
            SET [Purchasing].[PurchaseOrderHeader].[RevisionNumber] = 
                [Purchasing].[PurchaseOrderHeader].[RevisionNumber] + 1
            WHERE [Purchasing].[PurchaseOrderHeader].[PurchaseOrderID] IN 
                (SELECT inserted.[PurchaseOrderID] FROM inserted);
        END;
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspPrintError];

        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;

GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Incremental number to track changes to the purchase order over time.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'COLUMN', @level2name = N'RevisionNumber';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0.0', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'DF_PurchaseOrderHeader_TaxAmt';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'DF_PurchaseOrderHeader_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'COLUMN', @level2name = N'PurchaseOrderID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'General purchase order information. See PurchaseOrderDetail.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [Freight] >= (0.00)', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'CK_PurchaseOrderHeader_Freight';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [Status] BETWEEN (1) AND (4)', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'CK_PurchaseOrderHeader_Status';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'DF_PurchaseOrderHeader_RevisionNumber';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Employee.EmployeeID.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'FK_PurchaseOrderHeader_Employee_EmployeeID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [TaxAmt] >= (0.00)', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'CK_PurchaseOrderHeader_TaxAmt';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'PK_PurchaseOrderHeader_PurchaseOrderID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0.0', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'DF_PurchaseOrderHeader_SubTotal';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 0.0', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'DF_PurchaseOrderHeader_Freight';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Vendor.VendorID.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'FK_PurchaseOrderHeader_Vendor_VendorID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [ShipDate] >= [OrderDate] OR [ShipDate] IS NULL', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'CK_PurchaseOrderHeader_ShipDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'DF_PurchaseOrderHeader_OrderDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [SubTotal] >= (0.00)', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'CK_PurchaseOrderHeader_SubTotal';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Purchase order subtotal. Computed as SUM(PurchaseOrderDetail.LineTotal)for the appropriate PurchaseOrderID.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'COLUMN', @level2name = N'SubTotal';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Tax amount.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'COLUMN', @level2name = N'TaxAmt';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Estimated shipment date from the vendor.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'COLUMN', @level2name = N'ShipDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Shipping cost.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'COLUMN', @level2name = N'Freight';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 1', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'DF_PurchaseOrderHeader_Status';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Vendor with whom the purchase order is placed. Foreign key to Vendor.BusinessEntityID.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'COLUMN', @level2name = N'VendorID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Total due to vendor. Computed as Subtotal + TaxAmt + Freight.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'COLUMN', @level2name = N'TotalDue';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Purchase order creation date.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'COLUMN', @level2name = N'OrderDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Shipping method. Foreign key to ShipMethod.ShipMethodID.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'COLUMN', @level2name = N'ShipMethodID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Order current status. 1 = Pending; 2 = Approved; 3 = Rejected; 4 = Complete', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'COLUMN', @level2name = N'Status';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Employee who created the purchase order. Foreign key to Employee.BusinessEntityID.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'COLUMN', @level2name = N'EmployeeID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing ShipMethod.ShipMethodID.', @level0type = N'SCHEMA', @level0name = N'Purchasing', @level1type = N'TABLE', @level1name = N'PurchaseOrderHeader', @level2type = N'CONSTRAINT', @level2name = N'FK_PurchaseOrderHeader_ShipMethod_ShipMethodID';


GO

