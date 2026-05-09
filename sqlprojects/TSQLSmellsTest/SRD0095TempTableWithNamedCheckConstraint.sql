CREATE PROCEDURE [dbo].[SRD0095TempTableWithNamedCheckConstraint]
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #TempOk
    (
        ColA INT CHECK (ColA > 1)
    );

    CREATE TABLE #TempBad
    (
        ColA INT CONSTRAINT [CK_TempBad_ColA] CHECK (ColA > 1)
    );
END

-- SRD0095
