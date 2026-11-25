CREATE TABLE dbo.ExplicitNullability
(
    RequiredColumn INT NOT NULL,
    OptionalColumn INT NULL,
    UnspecifiedColumn INT,
    ComputedColumn AS (1 + 1),
    CONSTRAINT PK_ExplicitNullability PRIMARY KEY CLUSTERED (RequiredColumn)
);
