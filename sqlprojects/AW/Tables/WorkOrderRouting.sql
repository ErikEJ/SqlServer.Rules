CREATE TABLE [Production].[WorkOrderRouting] (
    [WorkOrderID]        INT            NOT NULL,
    [ProductID]          INT            NOT NULL,
    [OperationSequence]  SMALLINT       NOT NULL,
    [LocationID]         SMALLINT       NOT NULL,
    [ScheduledStartDate] DATETIME       NOT NULL,
    [ScheduledEndDate]   DATETIME       NOT NULL,
    [ActualStartDate]    DATETIME       NULL,
    [ActualEndDate]      DATETIME       NULL,
    [ActualResourceHrs]  DECIMAL (9, 4) NULL,
    [PlannedCost]        MONEY          NOT NULL,
    [ActualCost]         MONEY          NULL,
    [ModifiedDate]       DATETIME       CONSTRAINT [DF_WorkOrderRouting_ModifiedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_WorkOrderRouting_WorkOrderID_ProductID_OperationSequence] PRIMARY KEY CLUSTERED ([WorkOrderID] ASC, [ProductID] ASC, [OperationSequence] ASC),
    CONSTRAINT [CK_WorkOrderRouting_ActualCost] CHECK ([ActualCost]>(0.00)),
    CONSTRAINT [CK_WorkOrderRouting_ActualEndDate] CHECK ([ActualEndDate]>=[ActualStartDate] OR [ActualEndDate] IS NULL OR [ActualStartDate] IS NULL),
    CONSTRAINT [CK_WorkOrderRouting_ActualResourceHrs] CHECK ([ActualResourceHrs]>=(0.0000)),
    CONSTRAINT [CK_WorkOrderRouting_PlannedCost] CHECK ([PlannedCost]>(0.00)),
    CONSTRAINT [CK_WorkOrderRouting_ScheduledEndDate] CHECK ([ScheduledEndDate]>=[ScheduledStartDate]),
    CONSTRAINT [FK_WorkOrderRouting_Location_LocationID] FOREIGN KEY ([LocationID]) REFERENCES [Production].[Location] ([LocationID]),
    CONSTRAINT [FK_WorkOrderRouting_WorkOrder_WorkOrderID] FOREIGN KEY ([WorkOrderID]) REFERENCES [Production].[WorkOrder] ([WorkOrderID])
);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Nonclustered index.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'INDEX', @level2name = N'IX_WorkOrderRouting_ProductID';


GO

CREATE NONCLUSTERED INDEX [IX_WorkOrderRouting_ProductID]
    ON [Production].[WorkOrderRouting]([ProductID] ASC);


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Default constraint value of GETDATE()', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'CONSTRAINT', @level2name = N'DF_WorkOrderRouting_ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Actual start date.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'COLUMN', @level2name = N'ActualStartDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Date and time the record was last updated.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'COLUMN', @level2name = N'ModifiedDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key. Foreign key to Product.ProductID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'COLUMN', @level2name = N'ProductID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [ActualCost] > (0.00)', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'CONSTRAINT', @level2name = N'CK_WorkOrderRouting_ActualCost';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Estimated manufacturing cost.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'COLUMN', @level2name = N'PlannedCost';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Planned manufacturing start date.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'COLUMN', @level2name = N'ScheduledStartDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [ActualResourceHrs] >= (0.0000)', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'CONSTRAINT', @level2name = N'CK_WorkOrderRouting_ActualResourceHrs';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [ScheduledEndDate] >= [ScheduledStartDate]', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'CONSTRAINT', @level2name = N'CK_WorkOrderRouting_ScheduledEndDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Actual end date.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'COLUMN', @level2name = N'ActualEndDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing WorkOrder.WorkOrderID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'CONSTRAINT', @level2name = N'FK_WorkOrderRouting_WorkOrder_WorkOrderID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Work order details.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key. Indicates the manufacturing process sequence.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'COLUMN', @level2name = N'OperationSequence';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Foreign key constraint referencing Location.LocationID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'CONSTRAINT', @level2name = N'FK_WorkOrderRouting_Location_LocationID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Actual manufacturing cost.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'COLUMN', @level2name = N'ActualCost';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Planned manufacturing end date.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'COLUMN', @level2name = N'ScheduledEndDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [ActualEndDate] >= [ActualStartDate] OR [ActualEndDate] IS NULL OR [ActualStartDate] IS NULL', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'CONSTRAINT', @level2name = N'CK_WorkOrderRouting_ActualEndDate';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key (clustered) constraint', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'CONSTRAINT', @level2name = N'PK_WorkOrderRouting_WorkOrderID_ProductID_OperationSequence';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key. Foreign key to WorkOrder.WorkOrderID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'COLUMN', @level2name = N'WorkOrderID';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Number of manufacturing hours used.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'COLUMN', @level2name = N'ActualResourceHrs';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Check constraint [PlannedCost] > (0.00)', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'CONSTRAINT', @level2name = N'CK_WorkOrderRouting_PlannedCost';


GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Manufacturing location where the part is processed. Foreign key to Location.LocationID.', @level0type = N'SCHEMA', @level0name = N'Production', @level1type = N'TABLE', @level1name = N'WorkOrderRouting', @level2type = N'COLUMN', @level2name = N'LocationID';


GO

