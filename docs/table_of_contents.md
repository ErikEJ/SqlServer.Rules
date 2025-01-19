  
# Table of Contents
  
  
## CodeSmells
  
| Rule Id | Friendly Name | Ignorable | Description | Example? |
|----|----|----|----|----|
| [SML001](CodeSmells/SML001.md) | Avoid cross server joins |   | Avoid cross server joins | Yes |
| [SML002](CodeSmells/SML002.md) | Best practice is to use two part naming |   | Best practice is to use two part naming | Yes |
| [SML003](CodeSmells/SML003.md) | Dirty Reads cause consistency errors |   | Dirty Reads cause consistency errors | Yes |
| [SML004](CodeSmells/SML004.md) | Dont Override the optimizer |   | Dont Override the optimizer | Yes |
| [SML005](CodeSmells/SML005.md) | Avoid use of 'Select *' |   | Avoid use of 'Select *' | Yes |
| [SML006](CodeSmells/SML006.md) | Avoid Explicit Conversion of Columnar data |   | Avoid Explicit Conversion of Columnar data | Yes |
| [SML007](CodeSmells/SML007.md) | Avoid use of ordinal positions in ORDER BY Clauses |   | Avoid use of ordinal positions in ORDER BY Clauses |   |
| [SML008](CodeSmells/SML008.md) | Dont Change DateFormat |   | Dont Change DateFormat | Yes |
| [SML009](CodeSmells/SML009.md) | Dont Change DateFirst |   | Dont Change DateFirst | Yes |
| [SML010](CodeSmells/SML010.md) | ReadUnCommitted: Dirty reads can cause consistency errors |   | ReadUnCommitted: Dirty reads can cause consistency errors | Yes |
| [SML011](CodeSmells/SML011.md) | Single character aliases are poor practice |   | Single character aliases are poor practice | Yes |
| [SML012](CodeSmells/SML012.md) | Missing Column specifications on insert |   | Missing Column specifications on insert | Yes |
| [SML013](CodeSmells/SML013.md) | CONCAT_NULL_YIELDS_NULL should be on |   | CONCAT_NULL_YIELDS_NULL should be on | Yes |
| [SML014](CodeSmells/SML014.md) | ANSI_NULLS should be On |   | ANSI_NULLS should be On | Yes |
| [SML015](CodeSmells/SML015.md) | ANSI_PADDING should be On |   | ANSI_PADDING should be On | Yes |
| [SML016](CodeSmells/SML016.md) | ANSI_WARNINGS should be On |   | ANSI_WARNINGS should be On | Yes |
| [SML017](CodeSmells/SML017.md) | ARITHABORT should be On |   | ARITHABORT should be On | Yes |
| [SML018](CodeSmells/SML018.md) | NUMERIC_ROUNDABORT should be Off |   | NUMERIC_ROUNDABORT should be Off | Yes |
| [SML019](CodeSmells/SML019.md) | QUOTED_IDENTIFIER should be ON |   | QUOTED_IDENTIFIER should be ON | Yes |
| [SML020](CodeSmells/SML020.md) | FORCEPLAN should be OFF |   | FORCEPLAN should be OFF | Yes |
| [SML021](CodeSmells/SML021.md) | Use 2 part naming in EXECUTE statements |   | Use 2 part naming in EXECUTE statements | Yes |
| [SML022](CodeSmells/SML022.md) | Identity value should be agnostic |   | Identity value should be agnostic | Yes |
| [SML023](CodeSmells/SML023.md) | Avoid single line comments |   | Avoid single line comments |   |
| [SML024](CodeSmells/SML024.md) | Use two part naming |   | Use two part naming | Yes |
| [SML025](CodeSmells/SML025.md) | RANGE windows are much slower then ROWS (Explicit use) |   | RANGE windows are much slower then ROWS (Explicit use) | Yes |
| [SML026](CodeSmells/SML026.md) | RANGE windows are much slower then ROWS (Implicit use) |   | RANGE windows are much slower then ROWS (Implicit use) | Yes |
| [SML027](CodeSmells/SML027.md) | Create table statements should specify schema |   | Create table statements should specify schema | Yes |
| [SML028](CodeSmells/SML028.md) | Ordering in a view does not guarantee result set ordering |   | Ordering in a view does not guarantee result set ordering | Yes |
| [SML029](CodeSmells/SML029.md) | Cursors default to writable.  Specify FAST_FORWARD |   | Cursors default to writable.  Specify FAST_FORWARD | Yes |
| [SML030](CodeSmells/SML030.md) | Include SET NOCOUNT ON inside stored procedures |   | Include SET NOCOUNT ON inside stored procedures | Yes |
| [SML031](CodeSmells/SML031.md) | EXISTS/NOT EXISTS can be more performant than COUNT(*) |   | EXISTS/NOT EXISTS can be more performant than COUNT(*) |   |
| [SML032](CodeSmells/SML032.md) | Ordering in a derived table does not guarantee result set ordering |   | Ordering in a derived table does not guarantee result set ordering |   |
| [SML033](CodeSmells/SML033.md) | Single character variable names are poor practice |   | Single character variable names are poor practice | Yes |
| [SML034](CodeSmells/SML034.md) | Expression used with TOP should be wrapped in parenthises |   | Expression used with TOP should be wrapped in parenthises | Yes |
| [SML035](CodeSmells/SML035.md) | TOP(100) percent is ignored by the optimizer |   | TOP(100) percent is ignored by the optimizer |   |
| [SML036](CodeSmells/SML036.md) | Foreign Key Constraints should be named |   | Foreign Key Constraints should be named |   |
| [SML037](CodeSmells/SML037.md) | Check Constraints should be named |   | Check Constraints should be named |   |
| [SML038](CodeSmells/SML038.md) | Primary Key Constraints on temporary tables should not be named |   | Primary Key Constraints on temporary tables should not be named | Yes |
| [SML039](CodeSmells/SML039.md) | Default Constraints on temporary tables should not be named |   | Default Constraints on temporary tables should not be named | Yes |
| [SML040](CodeSmells/SML040.md) | Foreign Key Constraints on temporary tables should not be named |   | Foreign Key Constraints on temporary tables should not be named | Yes |
| [SML041](CodeSmells/SML041.md) | Check Constraints on temporary tables should not be named |   | Check Constraints on temporary tables should not be named |   |
| [SML042](CodeSmells/SML042.md) | Use of SET ROWCOUNT is deprecated : use TOP |   | Use of SET ROWCOUNT is deprecated : use TOP |   |
| [SML043](CodeSmells/SML043.md) | Potential SQL Injection Issue |   | Potential SQL Injection Issue | Yes |
| [SML044](CodeSmells/SML044.md) | Dont override the optimizer ( FORCESCAN ) |   | Dont override the optimizer ( FORCESCAN ) | Yes |
| [SML045](CodeSmells/SML045.md) | Dont override the optimizer ( Index Hint) |   | Dont override the optimizer ( Index Hint) | Yes |
| [SML046](CodeSmells/SML046.md) | "= Null" Comparison |   | "= Null" Comparison | Yes |
| [SML047](CodeSmells/SML047.md) | Use of deprecated data type |   | Use of deprecated data type | Yes |
  
## Design
  
| Rule Id | Friendly Name | Ignorable | Description | Example? |
|----|----|----|----|----|
| [SRD0001](Design/SRD0001.md) | Missing natural key |   | Table does not have a natural key. |   |
| [SRD0002](Design/SRD0002.md) | Missing primary key |   | Table does not have a primary key. |   |
| [SRD0003](Design/SRD0003.md) | Avoid wide primary keys |   | Primary Keys should avoid using GUIDS or wide VARCHAR columns. |   |
| [SRD0004](Design/SRD0004.md) | Index on Foreign Key |   | Columns on both sides of a foreign key should be indexed. |   |
| [SRD0005](Design/SRD0005.md) | Avoid long CHAR types | Yes | Avoid the (n)char column type except for short static length data. |   |
| [SRD0006](Design/SRD0006.md) | Avoid SELECT * | Yes | Avoid using SELECT *. |   |
| [SRD0009](Design/SRD0009.md) | Non-transactional body |   | Wrap multiple action statements within a transaction. |   |
| [SRD0010](Design/SRD0010.md) | Low identity seed value |   | Start identity column used in a primary key with a seed of 1000 or higher. |   |
| [SRD0011](Design/SRD0011.md) | Equality Compare With NULL Rule |   | Equality and inequality comparisons involving a NULL constant found. Use IS NULL or IS NOT NULL. |   |
| [SRD0012](Design/SRD0012.md) | Unused variable |   | Variable declared but never referenced or assigned. |   |
| [SRD0013](Design/SRD0013.md) | Expected error handeling |   | Wrap multiple action statements within a try catch. |   |
| [SRD0014](Design/SRD0014.md) | TOP without an ORDER BY |   | TOP clause used in a query without an ORDER BY clause. |   |
| [SRD0015](Design/SRD0015.md) | Implicit column list |   | Always use a column list in INSERT statements. |   |
| [SRD0016](Design/SRD0016.md) | Unused input parameter | Yes | Input parameter never used. Consider removing the parameter or using it. |   |
| [SRD0017](Design/SRD0017.md) | Avoid Deletes Without Where Rule | Yes | DELETE statement without row limiting conditions. |   |
| [SRD0018](Design/SRD0018.md) | Unbounded UPDATE | Yes | UPDATE statement without row limiting conditions. |   |
| [SRD0019](Design/SRD0019.md) | Avoid joining tables with views | Yes | Avoid joining tables with views. |   |
| [SRD0020](Design/SRD0020.md) | Incomplete or missing JOIN predicate |   | The query has issues with the join clause. It is either missing a backing foreign key or the join is missing one or more columns. |   |
| [SRD0021](Design/SRD0021.md) | Consider EXISTS Instead Of In Rule | Yes | Consider using EXISTS instead of IN when used with a subquery. |   |
| [SRD0024](Design/SRD0024.md) | Avoid EXEC or EXECUTE | Yes | Avoid EXEC and EXECUTE with string literals. Use parameterized sp_executesql instead. |   |
| [SRD0025](Design/SRD0025.md) | Avoid ORDER BY with numbers | Yes | Avoid using column numbers in ORDER BY clause. |   |
| [SRD0026](Design/SRD0026.md) | Unspecified type length |   | Do not use these data types (VARCHAR, NVARCHAR, CHAR, NCHAR) without specifying length. |   |
| [SRD0027](Design/SRD0027.md) | Unspecified precision or scale  |   | Do not use DECIMAL or NUMERIC data types without specifying precision and scale. |   |
| [SRD0028](Design/SRD0028.md) | Consider Column Prefix Rule | Yes | Consider prefixing column names with table name or table alias. |   |
| [SRD0030](Design/SRD0030.md) | Avoid Use of HINTS | Yes | Avoid using Hints to force a particular behavior. |   |
| [SRD0031](Design/SRD0031.md) | Avoid using CHARINDEX | Yes | Avoid using CHARINDEX function in WHERE clauses. |   |
| [SRD0032](Design/SRD0032.md) | Avoid use of OR in where clause | Yes | Try to avoid the OR operator in query where clauses if possible.  (Sargable) |   |
| [SRD0033](Design/SRD0033.md) | Avoid Cursors | Yes | Avoid using cursors. |   |
| [SRD0034](Design/SRD0034.md) | Use of NOLOCK |   | Do not use the NOLOCK clause. |   |
| [SRD0035](Design/SRD0035.md) | Forced delay |   | Do not use WAITFOR DELAY/TIME statement in stored procedures, functions, and triggers. |   |
| [SRD0036](Design/SRD0036.md) | Do not use SET ROWCOUNT | Yes | Do not use SET ROWCOUNT to restrict the number of rows. |   |
| [SRD0038](Design/SRD0038.md) | Alias Tables Rule | Yes | Consider aliasing all table sources in the query. |   |
| [SRD0039](Design/SRD0039.md) | Object not schema qualified |   | Use fully qualified object names in SELECT, UPDATE, DELETE, MERGE and EXECUTE statements. [schema].[name]. |   |
| [SRD0041](Design/SRD0041.md) | Avoid SELECT INTO temp or table variables | Yes | Avoid use of the SELECT INTO syntax. |   |
| [SRD0043](Design/SRD0043.md) | Possible side-effects implicit cast  |   | The arguments of the function '{0}' are not of the same datatype. |   |
| [SRD0044](Design/SRD0044.md) | Error handling requires SA permissions |   | The RAISERROR statement with severity above 18 requires the WITH LOG clause. |   |
| [SRD0045](Design/SRD0045.md) | Excessive indexes on table |   | Excessive number of indexes on table found on table. |   |
| [SRD0046](Design/SRD0046.md) | Use of approximate data type |   | Do not use the real or float data types for parameters or columns as they are approximate value data types. |   |
| [SRD0047](Design/SRD0047.md) | Ambiguous column name across design | Yes | Avoid using columns that match other columns by name, but are different in type or size. |   |
| [SRD0050](Design/SRD0050.md) | Expression reducible to constaint | Yes | The comparison expression always evaluates to TRUE or FALSE. | Yes |
| [SRD0051](Design/SRD0051.md) | Do Not Use Deprecated Types Rule |   | Don't use deprecated TEXT, NTEXT and IMAGE data types. |   |
| [SRD0052](Design/SRD0052.md) | Duplicate/Overlapping Index |   | Index has exact duplicate or borderline overlapping index. |   |
| [SRD0053](Design/SRD0053.md) | Explicit collation other | Yes | Object has different collation than the rest of the database. Try to avoid using a different collation unless by design. |   |
| [SRD0055](Design/SRD0055.md) | Object level option override |   | The object was created with invalid options. |   |
| [SRD0056](Design/SRD0056.md) | Unsafe identity retrieval | Yes | Use OUTPUT or SCOPE_IDENTITY() instead of @@IDENTITY. |   |
| [SRD0057](Design/SRD0057.md) | Do Not Mix DML With DDL Rule |   | Do not mix DML with DDL statements. Group DDL statements at the beginning of procedures followed by DML statements. |   |
| [SRD0058](Design/SRD0058.md) | Ordinal parameters used |   | Always use parameter names when calling stored procedures. |   |
| [SRD0060](Design/SRD0060.md) | Permission change in stored procedure |   | The procedure grants itself permissions. Possible missing GO command. | Yes |
| [SRD0061](Design/SRD0061.md) | Invalid database configured options |   | The database is configured with invalid options. |   |
| [SRD0062](Design/SRD0062.md) | Implicit collation |   | Create SQL Server temporary tables with the correct collation or use database default as the tempdb having a different collation than the database can cause issues and or data instability. |   |
| [SRD0063](Design/SRD0063.md) | Avoid wrapping SQL in IF statement | Yes | Do not use IF statements containing queries in stored procedures. |   |
| [SRD0064](Design/SRD0064.md) | Consider Caching Get Date To Variable | Yes | Cache multiple calls to GETDATE or SYSDATETIME into a variable. |   |
| [SRD0065](Design/SRD0065.md) | Avoid NOT FOR REPLICATION |   | Avoid 'NOT FOR REPLICATION' unless this is the desired behavior and replication is in use. |   |
  
## Naming
  
| Rule Id | Friendly Name | Ignorable | Description | Example? |
|----|----|----|----|----|
| [SRN0001](Naming/SRN0001.md) | UDF with System prefix | Yes | Avoid 'fn_' prefix when naming functions. |   |
| [SRN0002](Naming/SRN0002.md) | Procedure name may conflict system name | Yes | Avoid 'sp_' prefix when naming stored procedures. |   |
| [SRN0006](Naming/SRN0006.md) | Use of default schema |   | Two part naming on objects is required. |   |
| [SRN0007](Naming/SRN0007.md) | Name standard |   | General naming rules. |   |
  
## Performance
  
| Rule Id | Friendly Name | Ignorable | Description | Example? |
|----|----|----|----|----|
| [SRP0001](Performance/SRP0001.md) | Nested Views |   | Views should not use other views as a data source. |   |
| [SRP0002](Performance/SRP0002.md) | Unanchored string pattern | Yes | Try to avoid using patterns that start with '%' when using the LIKE keyword if possible.  (Sargable) |   |
| [SRP0003](Performance/SRP0003.md) | Aggregate of unique set | Yes | Avoid using DISTINCT keyword inside of aggregate functions. |   |
| [SRP0004](Performance/SRP0004.md) | Noisy trigger | Yes | Avoid returning results in triggers. |   |
| [SRP0005](Performance/SRP0005.md) | Noisy trigger | Yes | SET NOCOUNT ON is recommended to be enabled in stored procedures and triggers. |   |
| [SRP0006](Performance/SRP0006.md) | Use of inequality | Yes | Try to avoid using not equal operator (<>,!=) in the WHERE clause if possible. (Sargable) |   |
| [SRP0007](Performance/SRP0007.md) | Dangling cursor |   | Local cursor not closed. |   |
| [SRP0008](Performance/SRP0008.md) | Unfreed cursor |   | Local cursor not explicitly deallocated. |   |
| [SRP0009](Performance/SRP0009.md) | Filtering on calculated value | Yes | Avoid wrapping columns within a function in the WHERE clause. (Sargable) |   |
| [SRP0010](Performance/SRP0010.md) | Function in data modification | Yes | Avoid the use of user defined functions with UPDATE/INSERT/DELETE statements. (Halloween Protection) |   |
| [SRP0011](Performance/SRP0011.md) | Non-member test in predicate | Yes | Avoid using the NOT IN predicate in a WHERE clause. (Sargable) |   |
| [SRP0012](Performance/SRP0012.md) | Un-indexed membership test | Yes | Consider indexing the columns referenced by IN predicates in order to avoid table scans. |   |
| [SRP0013](Performance/SRP0013.md) | Existence tested with JOIN | Yes | Consider replacing the OUTER JOIN with EXISTS. | Yes |
| [SRP0014](Performance/SRP0014.md) | Table variable in JOIN | Yes | Avoid the use of table variables in join clauses. |   |
| [SRP0015](Performance/SRP0015.md) | Avoid Column Calculations | Yes | Avoid the use of calculations on columns in the where clause. (Sargable) |   |
| [SRP0016](Performance/SRP0016.md) | Equality test with mismatched types |   | Data types on both sides of an equality check should be the same in the where clause. (Sargable) |   |
| [SRP0017](Performance/SRP0017.md) | Update of Primary key | Yes | Avoid updating columns that are part of the primary key.  (Halloween Protection) |   |
| [SRP0018](Performance/SRP0018.md) | High join count |   | Query uses a high number of joins.  |   |
| [SRP0020](Performance/SRP0020.md) | Missing Clustered index |   | Table does not have a clustered index. |   |
| [SRP0021](Performance/SRP0021.md) | Manipulated parameter value | Yes | Avoid modification of parameters in a stored procedure prior to use in a select query. |   |
| [SRP0022](Performance/SRP0022.md) | Procedure level recompile option | Yes | Consider using RECOMPILE query hint instead of the WITH RECOMPILE option. | Yes |
| [SRP0023](Performance/SRP0023.md) | Enumerating for existence check | Yes | When checking for existence use EXISTS instead of COUNT |   |
| [SRP0024](Performance/SRP0024.md) | Correlated subquery | Yes | Avoid the use of correlated subqueries except for very small tables. |   |
  
<sub><sup>Generated by a tool</sup></sub>
