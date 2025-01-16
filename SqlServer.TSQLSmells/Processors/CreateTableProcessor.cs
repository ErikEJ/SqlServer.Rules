using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class CreateTableProcessor
    {
        private readonly Smells _smells;

        public CreateTableProcessor(Smells smells)
        {
            _smells = smells;
        }

        public void ProcessCreateTable(CreateTableStatement TblStmt)
        {
            var isTemp = TblStmt.SchemaObjectName.BaseIdentifier.Value.StartsWith('#') ||
                          TblStmt.SchemaObjectName.BaseIdentifier.Value.StartsWith('@');

            if (TblStmt.SchemaObjectName.SchemaIdentifier == null &&
                !isTemp)
            {
                _smells.SendFeedBack(27, TblStmt);
            }

            {
                foreach (var colDef in TblStmt.Definition.ColumnDefinitions)
                {
                    _smells.ProcessTsqlFragment(colDef);
                }
            }

            if (isTemp)
            {
                foreach (var constDef in TblStmt.Definition.TableConstraints)
                {
                    if (constDef.ConstraintIdentifier != null) { }

                    switch (FragmentTypeParser.GetFragmentType(constDef))
                    {
                        case "UniqueConstraintDefinition":
                            var unqConst = (UniqueConstraintDefinition)constDef;
                            if (unqConst.IsPrimaryKey)
                            {
                                _smells.SendFeedBack(38, constDef);
                            }

                            break;
                    }
                }

                foreach (var colDef in TblStmt.Definition.ColumnDefinitions)
                {
                    if (colDef.DefaultConstraint?.ConstraintIdentifier != null)
                    {
                        _smells.SendFeedBack(39, colDef);
                    }

                    foreach (var constDef in colDef.Constraints)
                    {

                        if (constDef.ConstraintIdentifier != null) { }

                        switch (FragmentTypeParser.GetFragmentType(constDef))
                        {

                            case "CheckConstraintDefinition":
                                var chkConst = (CheckConstraintDefinition)constDef;
                                if (chkConst.ConstraintIdentifier != null)
                                {
                                    _smells.SendFeedBack(40, chkConst);
                                }

                                break;
                        }
                    }
                }
            }
        }
    }
}