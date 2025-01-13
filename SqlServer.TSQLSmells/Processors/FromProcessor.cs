using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class FromProcessor
    {
        private readonly Smells _smells;

        public FromProcessor(Smells smells)
        {
            _smells = smells;
        }

        private static bool isCteName(SchemaObjectName ObjectName, WithCtesAndXmlNamespaces cte)
        {
            if (cte == null)
            {
                return false;
            }

            foreach (var Expression in cte.CommonTableExpressions)
            {
                if (Expression.ExpressionName.Value == ObjectName.BaseIdentifier.Value)
                {
                    return true;
                }
            }

            return false;
        }

        private void ProcessTableReference(TableReference TableRef, WithCtesAndXmlNamespaces cte)
        {
            var Type = FragmentTypeParser.GetFragmentType(TableRef);
            switch (Type)
            {
                case "NamedTableReference":
                    var NamedTableRef = (NamedTableReference) TableRef;
                    if (NamedTableRef.SchemaObject.BaseIdentifier.Value[0] != '#' &&
                        NamedTableRef.SchemaObject.BaseIdentifier.Value[0] != '@')
                    {
                        if (NamedTableRef.SchemaObject.ServerIdentifier != null)
                        {
                            _smells.SendFeedBack(1, NamedTableRef);
                        }

                        if (NamedTableRef.SchemaObject.SchemaIdentifier == null &&
                            !isCteName(NamedTableRef.SchemaObject, cte))
                        {
                            _smells.SendFeedBack(2, NamedTableRef);
                        }
                    }

                    if (NamedTableRef.TableHints != null)
                    {
                        foreach (var TableHint in NamedTableRef.TableHints)
                        {
                            switch (TableHint.HintKind)
                            {
                                case TableHintKind.NoLock:
                                    _smells.SendFeedBack(3, TableHint);
                                    break;
                                case TableHintKind.ReadPast:
                                    break;
                                case TableHintKind.ForceScan:
                                    _smells.SendFeedBack(44, TableHint);
                                    break;
                                case TableHintKind.Index:
                                    _smells.SendFeedBack(45, TableHint);
                                    break;
                                default:
                                    _smells.SendFeedBack(4, TableHint);
                                    break;
                            }
                        }
                    }

                    break;
                case "QueryDerivedTable":

                    var QueryDerivedRef = (QueryDerivedTable) TableRef;
                    var Alias = QueryDerivedRef.Alias.Value;
                    if (Alias.Length == 1)
                    {
                        _smells.SendFeedBack(11, QueryDerivedRef);
                    }

                    if (FragmentTypeParser.GetFragmentType(QueryDerivedRef.QueryExpression) == "QuerySpecification")
                    {
                        // QuerySpecification QuerySpec = (QuerySpecification)QueryDerivedRef.QueryExpression;
                        //  Process(QuerySpec.FromClause, cte);
                        _smells.ProcessQueryExpression(QueryDerivedRef.QueryExpression, "RG", true, cte);
                    }

                    break;
                case "QualifiedJoin":
                    var QualifiedJoin = (QualifiedJoin) TableRef;
                    ProcessTableReference(QualifiedJoin.FirstTableReference, cte);
                    ProcessTableReference(QualifiedJoin.SecondTableReference, cte);
                    break;
            }
        }

        public void Process(FromClause FromClause, WithCtesAndXmlNamespaces cte)
        {
            foreach (var TableRef in FromClause.TableReferences)
            {
                ProcessTableReference(TableRef, cte);
            }
        }
    }
}