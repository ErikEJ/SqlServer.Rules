# 8 New T-SQL Analysis Rules to Catch Bugs Before They Reach Production

Static analysis catches an entire class of bugs during the build process, long before code ever runs in production. The SqlServer.Rules library has just grown with 8 new rules — 7 design rules and 1 performance rule — each targeting a real category of mistake found in T-SQL stored procedures, functions, and views.

This post walks through every new rule, shows what problematic code looks like, explains why it matters, and describes the many ways you can integrate the rule set into your workflow.

---

## The New Rules at a Glance

| Rule ID | Category | Friendly Name | What It Detects |
|---------|----------|---------------|-----------------|
| [SRD0071](docs/Design/SRD0071.md) | Design | CASE without ELSE | `CASE` expression with no `ELSE` clause |
| [SRD0072](docs/Design/SRD0072.md) | Design | Variable self-assignment | `SET @x = @x` — a no-op assignment |
| [SRD0073](docs/Design/SRD0073.md) | Design | Repeated NOT operator | `NOT NOT condition` — a double negation |
| [SRD0074](docs/Design/SRD0074.md) | Design | Weak hashing algorithm | `HASHBYTES` with MD2, MD4, MD5, SHA, or SHA1 |
| [SRD0075](docs/Design/SRD0075.md) | Design | Hard-coded credentials | String literals assigned to `@password`, `@pwd`, `@secret`, or `@apikey` variables |
| [SRD0076](docs/Design/SRD0076.md) | Design | Identical expressions on both sides | `WHERE col = col` or `IF @x = @x` |
| [SRD0077](docs/Design/SRD0077.md) | Design | FETCH variable count mismatch | `FETCH INTO` variable count ≠ cursor `SELECT` column count |
| [SRP0025](docs/Performance/SRP0025.md) | Performance | SELECT * in EXISTS | `EXISTS (SELECT * …)` instead of `EXISTS (SELECT 1 …)` |

---

## Deep Dive: Each New Rule

### SRD0071 — CASE without ELSE

A `CASE` expression without an `ELSE` clause silently returns `NULL` when no `WHEN` branch matches. This often surprises developers who expect a default value.

```sql
-- ❌ Triggers SRD0071
DECLARE @status INT = 3;
DECLARE @label NVARCHAR(50);

SET @label = CASE @status
    WHEN 1 THEN 'Active'
    WHEN 2 THEN 'Inactive'
END;
-- @label is NULL when @status = 3, with no warning

-- ✅ Fixed
SET @label = CASE @status
    WHEN 1 THEN 'Active'
    WHEN 2 THEN 'Inactive'
    ELSE 'Unknown'
END;
```

Add an explicit `ELSE` clause — even `ELSE NULL` — to make the intent unmistakable.

---

### SRD0072 — Variable Self-Assignment

Assigning a variable to itself (`SET @x = @x`) is a no-op. It compiles and runs without error, but does nothing. It almost always indicates a copy-paste mistake where the wrong variable was used on the right-hand side.

```sql
-- ❌ Triggers SRD0072
DECLARE @customerId INT = 42;
DECLARE @orderId INT;
SET @orderId = @orderId;   -- copy-paste error, likely meant @customerId

-- ✅ Fixed
SET @orderId = @customerId;
```

Note: the rule only fires for pure self-assignment — `SET @x = @x + 1` is **not** flagged.

---

### SRD0073 — Repeated NOT Operator

A double negation (`NOT NOT condition`) is logically equivalent to the original condition and is almost certainly either a typo or a logic error. SQL Server accepts the syntax without complaint.

```sql
-- ❌ Triggers SRD0073
IF NOT NOT @isActive = 1
BEGIN
    PRINT 'Active';
END;

-- ✅ Fixed — negate once
IF NOT @isActive = 1
BEGIN
    PRINT 'Not active';
END;

-- ✅ Or remove both NOTs if no negation is intended
IF @isActive = 1
BEGIN
    PRINT 'Active';
END;
```

---

### SRD0074 — Weak Hashing Algorithm

`HASHBYTES` with `MD2`, `MD4`, `MD5`, `SHA`, or `SHA1` uses algorithms that are considered cryptographically broken and vulnerable to collision attacks. Use `SHA2_256` or `SHA2_512` for any security-sensitive hashing.

```sql
-- ❌ Triggers SRD0074
DECLARE @hash VARBINARY(8000);
SET @hash = HASHBYTES('MD5', 'sensitive data');
SET @hash = HASHBYTES('SHA1', 'sensitive data');

-- ✅ Fixed
SET @hash = HASHBYTES('SHA2_256', 'sensitive data');
-- or for higher security
SET @hash = HASHBYTES('SHA2_512', 'sensitive data');
```

This rule applies wherever `HASHBYTES` is used in a procedure, function, or view.

---

### SRD0075 — Hard-Coded Credentials

Embedding passwords, API keys, and other secrets as string literals in T-SQL is a security risk. Anyone with access to the source repository, deployment scripts, or database system tables can read them.

```sql
-- ❌ Triggers SRD0075
DECLARE @Password NVARCHAR(100) = 'MySecret123';
DECLARE @ApiKey NVARCHAR(100);
SET @ApiKey = 'sk-1234567890abcdef';

-- ✅ Fixed — retrieve at runtime from a secure store
DECLARE @Password NVARCHAR(100);
EXEC dbo.GetSecret @Name = 'AppPassword', @Value = @Password OUTPUT;
```

The rule matches variable and parameter names that contain `password`, `pwd`, `secret`, or `apikey` (case-insensitive) and flags them when a string literal is assigned.

---

### SRD0076 — Identical Expressions on Both Sides

A comparison where both sides are the same expression (e.g., `WHERE col = col`) is almost always a bug. It evaluates to either always `TRUE` or always `FALSE` (depending on the operator and NULLability), making the `WHERE` clause meaningless or the `IF` branch dead code.

```sql
-- ❌ Triggers SRD0076
DECLARE @x INT = 5;

IF @x = @x              -- always TRUE
    PRINT 'Always true';

IF @x <> @x             -- always FALSE (or UNKNOWN if @x IS NULL)
    PRINT 'Never reached';

-- ✅ Fixed — compare to the intended right-hand value
IF @x = @threshold
    PRINT 'Threshold reached';

-- ✅ To test for NOT NULL, use the correct idiom
IF @x IS NOT NULL
    PRINT 'Has a value';
```

---

### SRD0077 — FETCH Variable Count Mismatch

When the number of variables in a `FETCH … INTO` statement does not match the number of columns in the corresponding cursor's `SELECT` list, SQL Server raises a runtime error (`Msg 16924`). This rule surfaces the mismatch at build time.

```sql
-- ❌ Triggers SRD0077 — cursor selects 2 columns, FETCH provides 3 variables
DECLARE @id INT, @name NVARCHAR(100), @extra INT;

DECLARE cur CURSOR FOR
    SELECT [object_id], [name] FROM [sys].[objects];

OPEN cur;
FETCH NEXT FROM cur INTO @id, @name, @extra;   -- runtime error: variable count mismatch
CLOSE cur;
DEALLOCATE cur;

-- ✅ Fixed
DECLARE @id INT, @name NVARCHAR(100);

DECLARE cur CURSOR FOR
    SELECT [object_id], [name] FROM [sys].[objects];

OPEN cur;
FETCH NEXT FROM cur INTO @id, @name;
CLOSE cur;
DEALLOCATE cur;
```

---

### SRP0025 — SELECT * in EXISTS

Using `SELECT *` inside an `EXISTS` subquery sends an unnecessary signal to the reader: "I care about which columns are returned." The optimizer typically handles it, but `SELECT 1` is the established best practice and makes the intent explicit.

```sql
-- ❌ Triggers SRP0025
IF EXISTS (SELECT * FROM [sys].[objects] WHERE [name] = 'MyProc')
BEGIN
    PRINT 'Found';
END;

-- ✅ Fixed
IF EXISTS (SELECT 1 FROM [sys].[objects] WHERE [name] = 'MyProc')
BEGIN
    PRINT 'Found';
END;
```

---

## How to Use the Full Rule Set

SqlServer.Rules ships in several forms. Choose the integration that fits your workflow.

### 1. NuGet Package — Checks at Build Time

Add the rules as a NuGet dependency to any modern SQL Database project based on [MSBuild.Sdk.SqlProj](https://github.com/rr-wfm/MSBuild.Sdk.SqlProj) or [Microsoft.Build.Sql](https://github.com/microsoft/DacFx). Rules run automatically during `dotnet build` and surface violations as warnings in the build output and IDE error list.

```sh
dotnet add package ErikEJ.DacFX.SqlServer.Rules
```

A second package adds the complementary TSQL Smells rule set:

```sh
dotnet add package ErikEJ.DacFX.TSQLSmellSCA
```

Read more about enabling and configuring rules in the [MSBuild.Sdk.SqlProj static code analysis guide](https://github.com/rr-wfm/MSBuild.Sdk.SqlProj?tab=readme-ov-file#static-code-analysis).

### 2. CLI Tool — Analyze Scripts and Databases from the Terminal

The **T-SQL Analyzer CLI** is a .NET global tool that can analyze individual `.sql` files, entire folders, `.dacpac` files, `.zip` archives, and live databases.

```sh
dotnet tool install --global ErikEJ.DacFX.TSQLAnalyzer.Cli
```

**Common usage examples:**

```sh
# Analyze all .sql files in the current folder
tsqlanalyze

# Analyze a single stored procedure
tsqlanalyze -i C:\scripts\usp_CreateOrder.sql

# Analyze a folder
tsqlanalyze -i "C:\database scripts"

# Analyze a dacpac and export results to JSON
tsqlanalyze -i C:\deploy\MyDatabase.dacpac -o results.json

# Analyze a live database
tsqlanalyze -c "Data Source=.\SQLEXPRESS;Initial Catalog=MyDb;Integrated Security=True;Encrypt=false"

# Exclude a specific rule
tsqlanalyze -i C:\scripts -r Rules:-SqlServer.Rules.SRD0004

# Analyze scripts targeting a specific SQL Server version
tsqlanalyze -i C:\scripts\sproc.sql -s SqlAzure
```

### 3. Visual Studio Extension — Inline Warnings in the IDE

The **T-SQL Analyzer** Visual Studio extension brings the same rule set directly into the IDE. Install it from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=ErikEJ.TSqlAnalyzer) and violations appear in the **Error List** window as you work.

### 4. MCP Server — GitHub Copilot Chat Integration

The CLI tool doubles as an [MCP (Model Context Protocol) server](https://modelcontextprotocol.io/), letting GitHub Copilot Chat analyze your SQL scripts on demand inside VS Code or Visual Studio.

| Client | One-click Installation |
|--------|----------------------|
| **VS Code** | [![Install in VS Code](https://img.shields.io/badge/VS_Code-Install_TSQLAnalyzer-0098FF?style=flat-square&logoColor=white)](https://vscode.dev/redirect/mcp/install?name=tsqlanalyzer&config=%7B%22name%22%3A%22tsqlanalyzer%22%2C%22command%22%3A%22dnx%22%2C%22args%22%3A%5B%22ErikEJ.DacFX.TSQLAnalyzer.Cli%22%2C%22--yes%22%2C%22--%22%2C%22-mcp%22%5D%7D) |
| **Visual Studio** | [![Install in Visual Studio](https://img.shields.io/badge/Visual_Studio-Install_TSQLAnalyzer-C16FDE?logo=visualstudio&logoColor=white)](https://vs-open.link/mcp-install?%7B%22name%22%3A%22tsqlanalyzer%22%2C%22type%22%3A%22stdio%22%2C%22command%22%3A%22dnx%22%2C%22args%22%3A%5B%22ErikEJ.DacFX.TSQLAnalyzer.Cli%22%2C%22--yes%22%2C%22--%22%2C%22-mcp%22%5D%7D) |

Once configured, open GitHub Copilot Chat and ask:

> *"Analyze my stored procedure for T-SQL issues."*

Copilot will invoke the MCP server and return a list of rule violations with line numbers and descriptions.

### 5. Suppressing Rules When Needed

Every new rule is *ignorable*. If you have a legitimate reason to keep a particular pattern, suppress the rule in-line without disabling it project-wide:

```sql
-- Suppress a single occurrence on a specific line
SET @label = CASE @status
    WHEN 1 THEN 'Active'
    WHEN 2 THEN 'Inactive'
END; /* IGNORE SRD0071 */

-- Suppress all occurrences in the current object
/* GLOBAL IGNORE SRD0074 */
```

Read more in the [ignoring rules guide](docs/ignoring_rules.md).

---

## Summary

The eight new rules span two common areas of T-SQL quality:

- **Correctness bugs** that are syntactically valid but logically wrong (SRD0071, SRD0072, SRD0073, SRD0076, SRD0077)
- **Security concerns** that are easy to miss in code review (SRD0074, SRD0075)
- **Code clarity** best practice with a performance hint (SRP0025)

Combined with the existing 130+ rules in SqlServer.Rules and the TSQL Smells library, these additions make it harder for subtle T-SQL mistakes to slip through unnoticed. Whether you prefer build-time analysis via NuGet, a one-off CLI scan, IDE integration through the Visual Studio extension, or conversational analysis via GitHub Copilot and the MCP server, there is an integration point that fits your workflow.

For the full rule reference see the [documentation](docs/readme.md).
