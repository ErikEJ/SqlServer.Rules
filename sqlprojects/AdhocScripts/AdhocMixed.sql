-- a real object definition mixed with an ad-hoc batch.
-- the CREATE TABLE batch is left untouched (added as a real object),
-- while the SELECT * batch is wrapped so design rules fire on it.
CREATE TABLE dbo.AdhocMixedFoo
(
    Id INT NOT NULL,
    CONSTRAINT PK_AdhocMixedFoo PRIMARY KEY CLUSTERED (Id)
);
GO
SELECT *
FROM dbo.AdhocMixedFoo;
GO
