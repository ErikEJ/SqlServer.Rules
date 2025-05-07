using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class CreateTableProcessor
    {
        private readonly Smells smells;

        public CreateTableProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessCreateTable(CreateTableStatement tblStmt)
        {
#if NETSTANDARD
            var isTemp = tblStmt.SchemaObjectName.BaseIdentifier.Value.StartsWith('#') ||
                          tblStmt.SchemaObjectName.BaseIdentifier.Value.StartsWith('@');
#else
            var isTemp = tblStmt.SchemaObjectName.BaseIdentifier.Value.StartsWith("#", System.StringComparison.Ordinal) ||
                          tblStmt.SchemaObjectName.BaseIdentifier.Value.StartsWith("@", System.StringComparison.Ordinal);
#endif

            if (tblStmt.SchemaObjectName.SchemaIdentifier == null &&
                !isTemp)
            {
                smells.SendFeedBack(27, tblStmt);
            }

            {
                foreach (var colDef in tblStmt.Definition.ColumnDefinitions)
                {
                    smells.ProcessTsqlFragment(colDef);
                }
            }

            if (isTemp)
            {
                foreach (var constDef in tblStmt.Definition.TableConstraints)
                {
                    if (constDef.ConstraintIdentifier != null)
                    {
                    }

                    switch (FragmentTypeParser.GetFragmentType(constDef))
                    {
                        case "UniqueConstraintDefinition":
                            var unqConst = (UniqueConstraintDefinition)constDef;
                            if (unqConst.IsPrimaryKey && unqConst.ConstraintIdentifier != null)
                            {
                                smells.SendFeedBack(38, constDef);
                            }

                            break;
                    }
                }

                foreach (var colDef in tblStmt.Definition.ColumnDefinitions)
                {
                    if (colDef.DefaultConstraint?.ConstraintIdentifier != null)
                    {
                        smells.SendFeedBack(39, colDef);
                    }

                    foreach (var constDef in colDef.Constraints)
                    {
                        if (constDef.ConstraintIdentifier != null)
                        {
                        }

                        switch (FragmentTypeParser.GetFragmentType(constDef))
                        {
                            case "CheckConstraintDefinition":
                                var chkConst = (CheckConstraintDefinition)constDef;
                                if (chkConst.ConstraintIdentifier != null)
                                {
                                    smells.SendFeedBack(40, chkConst);
                                }

                                break;
                        }
                    }
                }
            }
        }
    }
}