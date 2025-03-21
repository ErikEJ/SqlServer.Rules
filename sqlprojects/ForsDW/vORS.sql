-- Auto Generated (Do not modify) 55A7B0F4E5E71381032AB8E0135EC05A521C43CF6D736227794CE4266B759EC7
CREATE VIEW vORS AS
SELECT TOP 10 *
FROM OPENROWSET( BULK 'https://pandemicdatalake.blob.core.windows.net/public/curated/covid-19/ecdc_cases/latest/ecdc_cases.parquet' ) 
AS r