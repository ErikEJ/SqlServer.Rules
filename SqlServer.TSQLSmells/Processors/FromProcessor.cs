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

            foreach (var expression in cte.CommonTableExpressions)
            {
                if (expression.ExpressionName.Value == objectName.BaseIdentifier.Value)
                {
                    return true;
                }
            }

            return false;
        }

        private void ProcessTableReference(TableReference tableRef, WithCtesAndXmlNamespaces cte)
        {
            var type = FragmentTypeParser.GetFragmentType(tableRef);

            switch (type)
            {
                case "NamedTableReference":

                    var namedTableRef = (NamedTableReference)tableRef;

                    if (namedTableRef.SchemaObject.BaseIdentifier.Value[0] != '#' &&
                        namedTableRef.SchemaObject.BaseIdentifier.Value[0] != '@')
                    {
                        if (namedTableRef.SchemaObject.ServerIdentifier != null)
                        {
                            smells.SendFeedBack(1, namedTableRef);
                        }

                        if (namedTableRef.SchemaObject.SchemaIdentifier == null &&
                            !IsCteName(namedTableRef.SchemaObject, cte))
                        {
                            smells.SendFeedBack(2, namedTableRef);
                        }
                    }

                    if (namedTableRef.TableHints != null)
                    {
                        foreach (var tableHint in namedTableRef.TableHints)
                        {
                            switch (tableHint.HintKind)
                            {
                                case TableHintKind.NoLock:
                                    smells.SendFeedBack(3, tableHint);
                                    break;
                                case TableHintKind.ReadPast:
                                    break;
                                case TableHintKind.ForceScan:
                                    smells.SendFeedBack(44, tableHint);
                                    break;
                                case TableHintKind.Index:
                                    smells.SendFeedBack(45, tableHint);
                                    break;
                                default:
                                    smells.SendFeedBack(4, tableHint);
                                    break;
                            }
                        }
                    }

                    break;
                case "QueryDerivedTable":

                    var queryDerivedRef = (QueryDerivedTable)tableRef;

                    var alias = queryDerivedRef.Alias.Value;

                    if (alias.Length == 1)
                    {
                        smells.SendFeedBack(11, queryDerivedRef);
                    }

                    if (FragmentTypeParser.GetFragmentType(queryDerivedRef.QueryExpression) == "QuerySpecification")
                    {
                        // QuerySpecification QuerySpec = (QuerySpecification)QueryDerivedRef.QueryExpression;
                        //  Process(QuerySpec.FromClause, cte);
                        smells.ProcessQueryExpression(queryDerivedRef.QueryExpression, "RG", true, cte);
                    }

                    break;
                case "QualifiedJoin":

                    var qualifiedJoin = (QualifiedJoin)tableRef;

                    ProcessTableReference(qualifiedJoin.FirstTableReference, cte);
                    ProcessTableReference(qualifiedJoin.SecondTableReference, cte);
                    break;
            }
        }

        public void Process(FromClause fromClause, WithCtesAndXmlNamespaces cte)
        {
            foreach (var tableRef in fromClause.TableReferences)
            {
                ProcessTableReference(tableRef, cte);
            }
        }
    }
}