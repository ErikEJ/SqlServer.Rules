CREATE TABLE [dbo].[Region] (
    [RegionId]            INT           IDENTITY (1, 1) NOT NULL,
    [Name]                NVARCHAR (50) NOT NULL,
    [CountryCode]         NVARCHAR (5)  CONSTRAINT [DF_Region_CountryCode]         DEFAULT (('DNK')) NOT NULL,
    CONSTRAINT [PK_Region] PRIMARY KEY CLUSTERED ([RegionId] ASC),
    CONSTRAINT [UC_Region_Name] UNIQUE NONCLUSTERED ([Name] ASC)
);
GO

CREATE TABLE [dbo].[MTGroup] (
    [MTGroupId] INT                                         IDENTITY (1, 1) NOT NULL,
    [Name]            NVARCHAR (50)                               NOT NULL,
    [RegionId]        INT                                         NOT NULL,
    [Grade]           INT                                         NOT NULL CONSTRAINT [DF_MTGroup_Grade] DEFAULT 0,
    CONSTRAINT [PK_MTGroup] PRIMARY KEY CLUSTERED ([MTGroupId] ASC),
    CONSTRAINT [FK_MTGroup_Region] FOREIGN KEY ([RegionId]) REFERENCES [dbo].[Region] ([RegionId]),
    CONSTRAINT [UC_MTGroup] UNIQUE NONCLUSTERED ([Name] ASC, [RegionId] ASC)
)
GO

CREATE TABLE [dbo].[MT] (
    [MTId]      INT                                         IDENTITY (1, 1) NOT NULL,
    [Name]            NVARCHAR (50)                               NOT NULL,
    [MTGroupId] INT                                         NOT NULL,
    CONSTRAINT [PK_MT] PRIMARY KEY CLUSTERED ([MTId] ASC),
    CONSTRAINT [FK_MT_MTGroupId] FOREIGN KEY ([MTGroupId]) REFERENCES [dbo].[MTGroup] ([MTGroupId]),
    CONSTRAINT [UQ_MT] UNIQUE NONCLUSTERED ([MTGroupId] ASC, [Name] ASC)
)
GO
