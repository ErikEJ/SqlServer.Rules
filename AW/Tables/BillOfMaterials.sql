CREATE TABLE [Production].[BillOfMaterials] (
    [BillOfMaterialsID] INT            IDENTITY (1, 1) NOT NULL,
    [ProductAssemblyID] INT            NULL,
    [ComponentID]       INT            NOT NULL,
    [StartDate]         DATETIME       CONSTRAINT [DF_BillOfMaterials_StartDate] DEFAULT (getdate()) NOT NULL,
    [EndDate]           DATETIME       NULL,
    [UnitMeasureCode]   NCHAR (3)      NOT NULL,
    [BOMLevel]          SMALLINT       NOT NULL,
    [PerAssemblyQty]    DECIMAL (8, 2) CONSTRAINT [DF_BillOfMaterials_PerAssemblyQty] DEFAULT ((1.00)) NOT NULL,
    [ModifiedDate]      DATETIME       CONSTRAINT [DF_BillOfMaterials_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_BillOfMaterials_BillOfMaterialsID] PRIMARY KEY NONCLUSTERED ([BillOfMaterialsID] ASC),
    CONSTRAINT [CK_BillOfMaterials_BOMLevel] CHECK ([ProductAssemblyID] IS NULL AND [BOMLevel]=(0) AND [PerAssemblyQty]=(1.00) OR [ProductAssemblyID] IS NOT NULL AND [BOMLevel]>=(1)),
    CONSTRAINT [CK_BillOfMaterials_EndDate] CHECK ([EndDate]>[StartDate] OR [EndDate] IS NULL),
    CONSTRAINT [CK_BillOfMaterials_PerAssemblyQty] CHECK ([PerAssemblyQty]>=(1.00)),
    CONSTRAINT [CK_BillOfMaterials_ProductAssemblyID] CHECK ([ProductAssemblyID]<>[ComponentID]),
    CONSTRAINT [FK_BillOfMaterials_Product_ComponentID] FOREIGN KEY ([ComponentID]) REFERENCES [Production].[Product] ([ProductID]),
    CONSTRAINT [FK_BillOfMaterials_Product_ProductAssemblyID] FOREIGN KEY ([ProductAssemblyID]) REFERENCES [Production].[Product] ([ProductID]),
    CONSTRAINT [FK_BillOfMaterials_UnitMeasure_UnitMeasureCode] FOREIGN KEY ([UnitMeasureCode]) REFERENCES [Production].[UnitMeasure] ([UnitMeasureCode])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Clustered index.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'INDEX', @level2name = N'AK_BillOfMaterials_ProductAssemblyID_ComponentID_StartDate';


GO

CREATE UNIQUE CLUSTERED INDEX [AK_BillOfMaterials_ProductAssemblyID_ComponentID_StartDate]
    ON [Production].[BillOfMaterials]([ProductAssemblyID] ASC, [ComponentID] ASC, [StartDate] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'INDEX', @level2name = N'IX_BillOfMaterials_UnitMeasureCode';


GO

CREATE NONCLUSTERED INDEX [IX_BillOfMaterials_UnitMeasureCode]
    ON [Production].[BillOfMaterials]([UnitMeasureCode] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'CONSTRAINT', @level2name = N'DF_BillOfMaterials_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'CONSTRAINT', @level2name = N'DF_BillOfMaterials_StartDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [PerAssemblyQty] >= (1.00)', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'CONSTRAINT', @level2name = N'CK_BillOfMaterials_PerAssemblyQty';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Component identification number. Foreign key to Product.ProductID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'COLUMN', @level2name = N'ComponentID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Quantity of the component needed to create the assembly.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'COLUMN', @level2name = N'PerAssemblyQty';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Parent product identification number. Foreign key to Product.ProductID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'COLUMN', @level2name = N'ProductAssemblyID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint EndDate] > [StartDate] OR [EndDate] IS NULL', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'CONSTRAINT', @level2name = N'CK_BillOfMaterials_EndDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Indicates the depth the component is from its parent (AssemblyID).', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'COLUMN', @level2name = N'BOMLevel';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Product.ComponentID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'CONSTRAINT', @level2name = N'FK_BillOfMaterials_Product_ComponentID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Standard code identifying the unit of measure for the quantity.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'COLUMN', @level2name = N'UnitMeasureCode';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for BillOfMaterials records.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'COLUMN', @level2name = N'BillOfMaterialsID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of 1.0', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'CONSTRAINT', @level2name = N'DF_BillOfMaterials_PerAssemblyQty';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Product.ProductAssemblyID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'CONSTRAINT', @level2name = N'FK_BillOfMaterials_Product_ProductAssemblyID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing UnitMeasure.UnitMeasureCode.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'CONSTRAINT', @level2name = N'FK_BillOfMaterials_UnitMeasure_UnitMeasureCode';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [ProductAssemblyID] IS NULL AND [BOMLevel] = (0) AND [PerAssemblyQty] = (1) OR [ProductAssemblyID] IS NOT NULL AND [BOMLevel] >= (1)', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'CONSTRAINT', @level2name = N'CK_BillOfMaterials_BOMLevel';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Items required to make bicycles and bicycle subassemblies. It identifies the heirarchical relationship between a parent product and its components.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date the component stopped being used in the assembly item.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'COLUMN', @level2name = N'EndDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date the component started being used in the assembly item.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'COLUMN', @level2name = N'StartDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [ProductAssemblyID] <> [ComponentID]', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'CONSTRAINT', @level2name = N'CK_BillOfMaterials_ProductAssemblyID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'BillOfMaterials', @level2type = N'CONSTRAINT', @level2name = N'PK_BillOfMaterials_BillOfMaterialsID';


GO

