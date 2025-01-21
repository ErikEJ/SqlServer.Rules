using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class FromProcessor
    {
        private readonly Smells smells;

        public FromProcessor(Smells smells)
        {
            this.smells = smells;
        }

        private static bool IsCteName(SchemaObjectName objectName, WithCtesAndXmlNamespaces cte)
        {
            if (cte == null)
            {
                return false;
            }

#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            foreach (var Expression in cte.CommonTableExpressions)
            {
                if (Expression.ExpressionName.Value == objectName.BaseIdentifier.Value)
                {
                    return true;
                }
            }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter

            return false;
        }

        private void ProcessTableReference(TableReference tableRef, WithCtesAndXmlNamespaces cte)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var Type = FragmentTypeParser.GetFragmentType(tableRef);
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            switch (Type)
            {
                case "NamedTableReference":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var NamedTableRef = (NamedTableReference)tableRef;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    if (NamedTableRef.SchemaObject.BaseIdentifier.Value[0] != '#' &&
                        NamedTableRef.SchemaObject.BaseIdentifier.Value[0] != '@')
                    {
                        if (NamedTableRef.SchemaObject.ServerIdentifier != null)
                        {
                            smells.SendFeedBack(1, NamedTableRef);
                        }

                        if (NamedTableRef.SchemaObject.SchemaIdentifier == null &&
                            !IsCteName(NamedTableRef.SchemaObject, cte))
                        {
                            smells.SendFeedBack(2, NamedTableRef);
                        }
                    }

                    if (NamedTableRef.TableHints != null)
                    {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                        foreach (var TableHint in NamedTableRef.TableHints)
                        {
                            switch (TableHint.HintKind)
                            {
                                case TableHintKind.NoLock:
                                    smells.SendFeedBack(3, TableHint);
                                    break;
                                case TableHintKind.ReadPast:
                                    break;
                                case TableHintKind.ForceScan:
                                    smells.SendFeedBack(44, TableHint);
                                    break;
                                case TableHintKind.Index:
                                    smells.SendFeedBack(45, TableHint);
                                    break;
                                default:
                                    smells.SendFeedBack(4, TableHint);
                                    break;
                            }
                        }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    }

                    break;
                case "QueryDerivedTable":

#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var QueryDerivedRef = (QueryDerivedTable)tableRef;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var Alias = QueryDerivedRef.Alias.Value;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    if (Alias.Length == 1)
                    {
                        smells.SendFeedBack(11, QueryDerivedRef);
                    }

                    if (FragmentTypeParser.GetFragmentType(QueryDerivedRef.QueryExpression) == "QuerySpecification")
                    {
                        // QuerySpecification QuerySpec = (QuerySpecification)QueryDerivedRef.QueryExpression;
                        //  Process(QuerySpec.FromClause, cte);
                        smells.ProcessQueryExpression(QueryDerivedRef.QueryExpression, "RG", true, cte);
                    }

                    break;
                case "QualifiedJoin":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var QualifiedJoin = (QualifiedJoin)tableRef;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    ProcessTableReference(QualifiedJoin.FirstTableReference, cte);
                    ProcessTableReference(QualifiedJoin.SecondTableReference, cte);
                    break;
            }
        }

        public void Process(FromClause fromClause, WithCtesAndXmlNamespaces cte)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            foreach (var TableRef in fromClause.TableReferences)
            {
                ProcessTableReference(TableRef, cte);
            }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
        }
    }
}