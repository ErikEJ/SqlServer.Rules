CREATE TABLE [dbo].[Import] (
    [ImportId]         INT                                         IDENTITY (1, 1) NOT NULL,
    [SubregionId]               INT                                         NOT NULL,
    [Version]                   INT                                         NOT NULL,
    [ImportDate]                DATETIME                                    NOT NULL,
    CONSTRAINT [PK_Import] PRIMARY KEY NONCLUSTERED ([ImportId]),
    CONSTRAINT [CI_Import] UNIQUE CLUSTERED ([SubregionId] ASC, [Version] ASC)
)
GO
