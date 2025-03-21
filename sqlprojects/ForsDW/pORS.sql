CREATE PROCEDURE pORS AS
BEGIN
SELECT TOP 10 *
FROM OPENROWSET( BULK 'https://pandemicdatalake.blob.core.windows.net/public/curated/covid-19/ecdc_cases/latest/ecdc_cases.parquet' ) 
AS r
END