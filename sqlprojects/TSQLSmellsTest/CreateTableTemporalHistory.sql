CREATE TABLE [dbo].[Zone] (
    [Id]          INT                                                IDENTITY (1, 1) NOT NULL,
    [Name]        VARCHAR (50)                                       NOT NULL,
    [PeriodEnd]   DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN   NOT NULL,
    [PeriodStart] DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([PeriodStart], [PeriodEnd])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[dbo].[ZoneHistory], DATA_CONSISTENCY_CHECK=ON));

GO
CREATE TABLE [dbo].[ZoneHistory] (
    [Id]          INT           NOT NULL,
    [Name]        VARCHAR (50)  NOT NULL,
    [PeriodEnd]   DATETIME2 (7) NOT NULL,
    [PeriodStart] DATETIME2 (7) NOT NULL
);

GO
ALTER TABLE [dbo].[Zone]
    ADD CONSTRAINT [PK_Zone] PRIMARY KEY CLUSTERED ([Id] ASC);

GO
CREATE CLUSTERED INDEX [ix_ZoneHistory]
    ON [dbo].[ZoneHistory]([PeriodEnd] ASC, [PeriodStart] ASC);

GO