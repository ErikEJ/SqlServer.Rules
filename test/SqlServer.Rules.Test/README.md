# Testing SqlServer.Rules

Unit testing the SQL Server rules relies on two things: setup files and a directory of SQL files. The files should be placed in a directory that describes the test case which is in a directory describing the rule's purpose.

For example, a test case for nonSARGablility rules would be placed in the Performance\NonsargableLikePattern directory

An example folder structure would be:

```plaintext
TestScripts
└───_Setup
|   |   table1.sql
|   |   view1.sql
|   |   stored_procedure1.sql
|
└───Performance
    └───NonsargableLikePattern
        |   nonsargable.sql
        |   nonsargable_ignored.sql
        |   sargable.sql
```

**Important:** Any files that are placed in the _Setup directory are loaded and ran for **every** unit test.
