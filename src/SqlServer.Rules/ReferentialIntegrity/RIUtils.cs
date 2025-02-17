using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Index = Microsoft.SqlServer.Dac.Model.Index;

namespace SqlServer.Rules.ReferentialIntegrity
{
    public static class RIUtils
    {
        public static bool CheckForFkIndex(this TSqlObject table, IList<ObjectIdentifier> columnNames)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            if (columnNames == null)
            {
                throw new ArgumentNullException(nameof(columnNames));
            }

            if (table.ObjectType != Table.TypeClass)
            {
                throw new ArgumentException("The parameter is not of type Table", nameof(table));
            }

            // convert the column names to a list of string
            var fkColumnNames = columnNames.Select(x => x.Parts.Last()).ToList();

            // get all the indexes for this table
            var indexes = table.GetReferencing(DacQueryScopes.All)
                .Where(x => x.ObjectType == Index.TypeClass
                    || x.ObjectType == PrimaryKeyConstraint.TypeClass
                    || x.ObjectType == UniqueConstraint.TypeClass).ToList();

            if (indexes.Count == 0)
            {
                return false;
            }

            // pull all the column names out of the indexes
            var indexInfo = new Dictionary<string, IList<string>>();
            foreach (var index in indexes)
            {
                var columns = index.GetReferenced(DacQueryScopes.All)
                    .Where(x => x.ObjectType == Column.TypeClass);
                indexInfo.Add(
                    index.Name.GetName(),
                    new List<string>(columns.Select(c => c.Name.Parts.Last())));
            }

            // find any index that contains all the columns from the foreign key
            return indexInfo.Any(ii =>
            {
                // intersect works, but the index must match the column names in
                // the correct order, and the proper ordinal in the index hence the for...
                // i.Value.Intersect(fkColumnNames).Count() == fkColumnNames.Count())
                for (var i = 0; i < fkColumnNames.Count; i++)
                {
                    if (!fkColumnNames[i].StringEquals(ii.Value?.ElementAtOrDefault(i)))
                    {
                        return false;
                    }
                }

                return true;
            });
        }

        public static IDictionary<string, ForeignKeyInfo> GetTableFKInfos(this TSqlObject table)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            if (table.ObjectType != Table.TypeClass)
            {
                throw new ArgumentException("The parameter is not of type Table", nameof(table));
            }

            var fks = new Dictionary<string, ForeignKeyInfo>();
            var tableFks = table.GetReferencing(DacQueryScopes.All).Where(x => x.ObjectType == ForeignKeyConstraint.TypeClass);
            foreach (var fk in tableFks)
            {
                var name = fk.Name.GetName();
                if (!fks.ContainsKey(name))
                {
                    fks.Add(name, fk.GetFKInfo());
                }
            }

            return fks;
        }

        public static ForeignKeyInfo GetFKInfo(this TSqlObject fk)
        {
            if (fk == null)
            {
                throw new ArgumentNullException(nameof(fk));
            }

            if (fk.ObjectType != ForeignKeyConstraint.TypeClass)
            {
                throw new ArgumentException("The parameter is not of type ForeignKeyConstraint", nameof(fk));
            }

            var fkColumns = fk.GetReferencedRelationshipInstances(ForeignKeyConstraint.Columns, DacQueryScopes.All)
                .Select(x => x.ObjectName).ToList();
            var fkForeignColumns = fk.GetReferencedRelationshipInstances(ForeignKeyConstraint.ForeignColumns, DacQueryScopes.All)
                .Select(x => x.ObjectName).ToList();

            return new ForeignKeyInfo
            {
                Name = fk.Name.GetName(),
                TableName = new ObjectIdentifier(GetTableOrAliasName(fkColumns.FirstOrDefault())),
                ToTableName = new ObjectIdentifier(GetTableOrAliasName(fkForeignColumns.FirstOrDefault())),
                ColumnNames = fkColumns,
                ToColumnNames = fkForeignColumns,
            };
        }

        public static IList<JoinInfo> GetFromClauseJoinTables(this FromClause from)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            var joins = new List<JoinInfo>();

            if (from.TableReferences.Count == 0 || from.TableReferences.First().GetType() != typeof(QualifiedJoin))
            {
                return joins;
            }

            var joinVisitor = new JoinVisitor();
            from.Accept(joinVisitor);

            // build the list of pure tables along with the list of boolean comparisons
            foreach (var join in joinVisitor.QualifiedJoins)
            {
                var joinInfo = new JoinInfo { };

                var boolVisitor = new BooleanComparisonVisitor();
                join.SearchCondition.Accept(boolVisitor);
                joinInfo.Compares = new List<BooleanComparisonExpression>(boolVisitor.Statements);

                if (join.FirstTableReference.GetType() == typeof(NamedTableReference))
                {
                    joinInfo.Table1 = join.FirstTableReference as NamedTableReference;
                }

                if (join.SecondTableReference.GetType() == typeof(NamedTableReference))
                {
                    joinInfo.Table2 = join.SecondTableReference as NamedTableReference;
                }

                joins.Add(joinInfo);
            }

            // table2 should always have a table..... maybe. unless the table is actually a sub-select. Then we will ignore it
            foreach (var join in joins.Where(j => j.Table2 != null))
            {
                var table1 = join.Table1;
                var table2 = join.Table2;
                var table2Alias = table2.Alias?.Value;
                var table2Name = new ObjectIdentifier(table2.SchemaObject.Identifiers.Select(x => x.Value));

                // we need to figure out which side of the comparison goes to which table..... PITA. yes.....
                foreach (var compare in join.Compares
                    .Where(x => x.FirstExpression is ColumnReferenceExpression && x.SecondExpression is ColumnReferenceExpression))
                {
                    // we use a loop as we need to check both the first expression and the second expression to see which table the columns belong to
                    for (var i = 0; i < 2; i++)
                    {
                        var col = (i == 0 ? compare.FirstExpression : compare.SecondExpression) as ColumnReferenceExpression;

                        if (col == null)
                        {
                            continue;
                        }

                        var colTblName = GetTableOrAliasName(col.MultiPartIdentifier.Identifiers);
                        if (table2Alias.StringEquals(colTblName.First()) || table2Name.CompareTo(colTblName) >= 5)
                        {
                            join.Table2JoinColumns.Add(col);
                            continue;
                        }

                        // use table1 if it was supplied in the compare. else scan the joins to find the matching table to the column
                        var tbl = table1 ?? joins.Select(x =>
                        {
                            if (CheckName(x.Table2, col))
                            {
                                return x.Table2;
                            }

                            if (CheckName(x.Table1, col))
                            {
                                return x.Table1;
                            }

                            return null;
                        }).FirstOrDefault(x => x != null);

                        if (tbl != null)
                        {
                            var tblAlias = tbl.Alias?.Value;
                            var tblName = new ObjectIdentifier(tbl.SchemaObject.Identifiers.Select(x => x.Value));

                            if (join.Table1 == null)
                            {
                                join.Table1 = tbl;
                            }

                            if (tblAlias.StringEquals(colTblName.First()) || tblName.CompareTo(colTblName) >= 5)
                            {
                                join.Table1JoinColumns.Add(col);
                            }
                        }
                    }
                }
            }

            return joins;
        }

        private static IList<string> GetTableOrAliasName(ObjectIdentifier identifier)
        {
            var parts = identifier.Parts;
            if (parts.Count == 1)
            {
                return parts;
            }

            // take the first parts minus one from the length. as they could use dbo.Table.Column or Table.Column, or t1.Column
            return parts.Take(parts.Count - 1).ToList();
        }

        private static List<string> GetTableOrAliasName(IEnumerable<Identifier> identifiers)
        {
#pragma warning disable CA1851 // Possible multiple enumerations of 'IEnumerable' collection
            if (identifiers.Count() == 1)
            {
                return identifiers.Select(x => x.Value).ToList();
            }

            // take the first parts minus one from the length. as they could use dbo.Table.Column or Table.Column, or t1.Column
            return identifiers.Take(identifiers.Count() - 1).Select(x => x.Value).ToList();
#pragma warning restore CA1851 // Possible multiple enumerations of 'IEnumerable' collection
        }

        private static bool CheckName(NamedTableReference tbl, ColumnReferenceExpression col)
        {
            if (tbl == null)
            {
                return false;
            }

            var colNameParts = col.MultiPartIdentifier.Identifiers;
            var colTableName = new ObjectIdentifier(colNameParts.Take(colNameParts.Count - 1).Select(x => x.Value));

            var alias = tbl.Alias?.Value;
            var tblName = GetTableOrAliasName(tbl.SchemaObject.Identifiers);

            return alias.StringEquals(colTableName.Parts.First()) || colTableName.CompareTo(tblName) >= 5;
        }
    }
}
