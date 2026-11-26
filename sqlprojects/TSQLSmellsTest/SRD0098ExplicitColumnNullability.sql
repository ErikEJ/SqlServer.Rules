CREATE TABLE dbo.ExplicitNullability
(
    RequiredColumn INT NOT NULL,
    OptionalColumn INT NULL,
    UnspecifiedColumn INT,
    ComputedColumn AS (1 + 1),
    IdentityColumn INT IDENTITY(1, 1),
    SparseColumn INT SPARSE,
    TablePrimaryKeyColumn INT,
    CONSTRAINT PK_ExplicitNullability PRIMARY KEY CLUSTERED (TablePrimaryKeyColumn)
);
