using System.Collections.Generic;
using System.Globalization;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class Smells
    {
        private readonly List<VarAssignment> assignmentList = new List<VarAssignment>();

        private readonly SelectStatementProcessor selectStatementProcessor;
        private readonly InsertProcessor insertProcessor;
        private readonly ExecutableEntityProcessor executableEntityProcessor;
        private readonly FromProcessor fromProcessor;
        private readonly WhereProcessor whereProcessor;
        private readonly OrderByProcessor orderByProcessor;
        private readonly WhileProcessor whileProcessor;
        private readonly PredicateSetProcessor predicateSetProcessor;
        private readonly SetProcessor setProcessor;
        private readonly FunctionProcessor functionProcessor;
        private readonly TopProcessor topProcessor;
        private readonly CreateTableProcessor createTableProcessor;
        private readonly SelectSetProcessor selectSetProcessor;
        private readonly SqlDataTypeProcessor sqlDataTypeProcessor;
        private readonly ViewStatementProcessor viewStatementProcessor;
        private readonly SetTransactionIsolationLevelProcessor setTransactionIsolationLevelProcessor;
        private readonly CursorProcessor cursorProcessor;
        private readonly BeginEndBlockProcessor beginEndBlockProcessor;
        private readonly SelectFunctionReturnTypeProcessor selectFunctionReturnTypeProcessor;
        private readonly FunctionStatementBodyProcessor functionStatementBodyProcessor;
        private readonly ProcedureStatementBodyProcessor procedureStatementBodyProcessor;
        private readonly IfStatementProcessor ifStatementProcessor;
        private readonly DeclareVariableProcessor declareVariableProcessor;
        private readonly TableVariableProcessor tableVariableProcessor;
        private readonly ReturnStatementProcessor returnStatementProcessor;
        private readonly ColumnDefinitionProcessor columnDefinitionProcessor;
        private int iRule;
        private TSqlObject modelElement;
        private List<SqlRuleProblem> problems;

        public Smells()
        {
            selectStatementProcessor = new SelectStatementProcessor(this);
            insertProcessor = new InsertProcessor(this);
            executableEntityProcessor = new ExecutableEntityProcessor(this);
            fromProcessor = new FromProcessor(this);
            whereProcessor = new WhereProcessor(this);
            orderByProcessor = new OrderByProcessor(this);
            whileProcessor = new WhileProcessor(this);
            predicateSetProcessor = new PredicateSetProcessor(this);
            setProcessor = new SetProcessor(this);
            functionProcessor = new FunctionProcessor(this);
            topProcessor = new TopProcessor(this);
            createTableProcessor = new CreateTableProcessor(this);
            selectSetProcessor = new SelectSetProcessor(this);
            sqlDataTypeProcessor = new SqlDataTypeProcessor(this);
            viewStatementProcessor = new ViewStatementProcessor(this);
            setTransactionIsolationLevelProcessor = new SetTransactionIsolationLevelProcessor(this);
            cursorProcessor = new CursorProcessor(this);
            beginEndBlockProcessor = new BeginEndBlockProcessor(this);
            selectFunctionReturnTypeProcessor = new SelectFunctionReturnTypeProcessor(this);
            functionStatementBodyProcessor = new FunctionStatementBodyProcessor(this);
            procedureStatementBodyProcessor = new ProcedureStatementBodyProcessor(this);
            ifStatementProcessor = new IfStatementProcessor(this);
            declareVariableProcessor = new DeclareVariableProcessor(this);
            tableVariableProcessor = new TableVariableProcessor(this);
            returnStatementProcessor = new ReturnStatementProcessor(this);
            columnDefinitionProcessor = new ColumnDefinitionProcessor(this);
        }

        public InsertProcessor InsertProcessor
        {
            get { return insertProcessor; }
        }

        public ExecutableEntityProcessor ExecutableEntityProcessor
        {
            get { return executableEntityProcessor; }
        }

        public FunctionProcessor FunctionProcessor
        {
            get { return functionProcessor; }
        }

        public SelectSetProcessor SelectSetProcessor
        {
            get { return selectSetProcessor; }
        }

#pragma warning disable CA1002 // Do not expose generic lists
        public List<VarAssignment> AssignmentList
#pragma warning restore CA1002 // Do not expose generic lists
        {
            get { return assignmentList; }
        }

        public ProcedureStatementBodyProcessor ProcedureStatementBodyProcessor
        {
            get { return procedureStatementBodyProcessor; }
        }

        public void SendFeedBack(int errorNum, TSqlFragment errorFrg)
        {
            if (errorNum != iRule)
            {
                return;
            }

            var rm = Resources.ResourceManager;

            var lookup = "TSQLSmellRuleName" + errorNum.ToString("D2", CultureInfo.InvariantCulture);

            var @out = rm.GetString(lookup, CultureInfo.InvariantCulture);

            problems.Add(new SqlRuleProblem(@out, modelElement, errorFrg));
        }

        public void ProcessQueryExpression(
            QueryExpression queryExpression,
            string parentType,
            bool testTop = false,
            WithCtesAndXmlNamespaces cte = null)
        {
            var expressionType = FragmentTypeParser.GetFragmentType(queryExpression);
            switch (expressionType)
            {
                case "QuerySpecification":
                    // {$Query = $Stmt.QueryExpression;
                    var querySpec = (QuerySpecification)queryExpression;
                    selectStatementProcessor.ProcessSelectElements(querySpec.SelectElements, parentType, cte);
                    if (querySpec.FromClause != null)
                    {
                        fromProcessor.Process(querySpec.FromClause, cte);
                    }

                    if (querySpec.WhereClause != null)
                    {
                        whereProcessor.Process(querySpec.WhereClause);
                    }

                    if (querySpec.OrderByClause != null)
                    {
                        orderByProcessor.Process(querySpec.OrderByClause);
                        if (parentType == "VW")
                        {
                            SendFeedBack(28, querySpec.OrderByClause);
                        }
                    }

                    if (querySpec.TopRowFilter != null)
                    {
                        topProcessor.ProcessTopFilter(querySpec.TopRowFilter);
                    }

                    break;
                case "QueryParenthesisExpression":
                    // {$Query=$Stmt.QueryExpression.QueryExpression;break}
                    var expression = (QueryParenthesisExpression)queryExpression;
                    ProcessQueryExpression(expression.QueryExpression, "RG", testTop, cte);

                    break;
                case "BinaryQueryExpression":
                    var binaryQueryExpression = (BinaryQueryExpression)queryExpression;
                    ProcessQueryExpression(binaryQueryExpression.FirstQueryExpression, parentType, testTop, cte);
                    ProcessQueryExpression(binaryQueryExpression.SecondQueryExpression, parentType, testTop, cte);

                    // BinaryQueryExpression.

                    // {Process-BinaryQueryExpression $Stmt.QueryExpression;break;}
                    break;
            }
        }

        // void ProcessSelectElements(
        public void ProcessTsqlFragment(TSqlFragment fragment)
        {
            var stmtType = FragmentTypeParser.GetFragmentType(fragment);

            // Console.WriteLine(StmtType);
            switch (stmtType)
            {
                case "DeclareCursorStatement":
                    cursorProcessor.ProcessCursorStatement((DeclareCursorStatement)fragment);
                    break;
                case "BeginEndBlockStatement":
                    beginEndBlockProcessor.ProcessBeginEndBlockStatement((BeginEndBlockStatement)fragment);
                    break;
                case "CreateFunctionStatement":
                case "AlterFunctionStatement":
                    functionStatementBodyProcessor.ProcessFunctionStatementBody((FunctionStatementBody)fragment);
                    break;
                case "SelectFunctionReturnType":
                    selectFunctionReturnTypeProcessor.ProcessSelectFunctionReturnType((SelectFunctionReturnType)fragment);
                    return;
                case "SetTransactionIsolationLevelStatement":
                    setTransactionIsolationLevelProcessor.ProcessSetTransactionIolationLevelStatement((SetTransactionIsolationLevelStatement)fragment);
                    break;
                case "WhileStatement":
                    whileProcessor.ProcessWhileStatement((WhileStatement)fragment);
                    break;
                case "InsertStatement":
                    InsertProcessor.Process((InsertStatement)fragment);
                    break;
                case "SelectStatement":
                    selectStatementProcessor.Process((SelectStatement)fragment, "RG", true);
                    break;
                case "SetRowCountStatement":
                    SendFeedBack(42, fragment);
                    break;
                case "IfStatement":
                    ifStatementProcessor.ProcessIfStatement((IfStatement)fragment);
                    break;
                case "PredicateSetStatement":
                    predicateSetProcessor.ProcessPredicateSetStatement((PredicateSetStatement)fragment);
                    break;
                case "ExecuteStatement":
                    ExecutableEntityProcessor.ProcessExecuteStatement((ExecuteStatement)fragment);
                    break;
                case "SetIdentityInsertStatement":
                    SendFeedBack(22, fragment);
                    break;
                case "SetCommandStatement":
                    setProcessor.ProcessSetStatement((SetCommandStatement)fragment);
                    break;

                case "CreateTableStatement":
                    createTableProcessor.ProcessCreateTable((CreateTableStatement)fragment);
                    break;

                case "CreateProcedureStatement":
                case "AlterProcedureStatement":
                    ProcedureStatementBodyProcessor.ProcessProcedureStatementBody((ProcedureStatementBody)fragment);
                    assignmentList.Clear();
                    break;
                case "CreateViewStatement":
                case "AlterViewStatement":
                    viewStatementProcessor.ProcessViewStatementBody((ViewStatementBody)fragment);
                    break;
                case "TSqlBatch":
                    var batch = (TSqlBatch)fragment;
                    foreach (var innerFragment in batch.Statements)
                    {
                        ProcessTsqlFragment(innerFragment);
                    }

                    break;
                case "TSqlScript":
                    var script = (TSqlScript)fragment;
                    foreach (var innerBatch in script.Batches)
                    {
                        ProcessTsqlFragment(innerBatch);
                    }

                    break;
                case "TryCatchStatement":
                    var trycatch = (TryCatchStatement)fragment;

                    foreach (var innerStmt in trycatch.TryStatements.Statements)
                    {
                        ProcessTsqlFragment(innerStmt);
                    }

                    foreach (var innerStmt in trycatch.CatchStatements.Statements)
                    {
                        ProcessTsqlFragment(innerStmt);
                    }

                    break;
                case "BooleanParenthesisExpression":
                    var expression = (BooleanParenthesisExpression)fragment;
                    ProcessTsqlFragment(expression.Expression);
                    break;
                case "BooleanComparisonExpression":
                    var bcExpression = (BooleanComparisonExpression)fragment;
                    ProcessTsqlFragment(bcExpression.FirstExpression);
                    ProcessTsqlFragment(bcExpression.SecondExpression);
                    break;
                case "ScalarSubquery":
                    var scalarSubquery = (ScalarSubquery)fragment;
                    ProcessQueryExpression(scalarSubquery.QueryExpression, "RG");
                    break;
                case "ReturnStatement":
                    returnStatementProcessor.ProcessReturnStatement((ReturnStatement)fragment);
                    break;
                case "IntegerLiteral":
                    break;
                case "DeclareVariableStatement":
                    declareVariableProcessor.ProcessDeclareVariableStatement((DeclareVariableStatement)fragment);
                    break;
                case "DeclareVariableElement":
                    declareVariableProcessor.ProcessDeclareVariableElement((DeclareVariableElement)fragment);
                    break;
                case "PrintStatement":
                    break;
                case "SqlDataTypeReference":
                    sqlDataTypeProcessor.ProcessSqlDataTypeReference((SqlDataTypeReference)fragment);
                    break;
                case "DeclareTableVariableStatement":
                    tableVariableProcessor.ProcessTableVariableStatement((DeclareTableVariableStatement)fragment);
                    break;
                case "TableValuedFunctionReturnType":
                    tableVariableProcessor.ProcessTableValuedFunctionReturnType((TableValuedFunctionReturnType)fragment);
                    break;
                case "DeclareTableVariableBody":
                    tableVariableProcessor.ProcessTableVariableBody((DeclareTableVariableBody)fragment);
                    break;
                case "VariableReference":
                    // ProcessVariableReference((VariableReference)Fragment);
                    break;
                case "ExistsPredicate":
                    tableVariableProcessor.ProcessExistsPredicate((ExistsPredicate)fragment);
                    break;

                case "ColumnDefinition":
                    columnDefinitionProcessor.ProcessColumnDefinition((ColumnDefinition)fragment);
                    break;
            }
        }

#pragma warning disable CA1002 // Do not expose generic lists
        public List<SqlRuleProblem> ProcessObject(TSqlObject sqlObject, int iRule)
#pragma warning restore CA1002 // Do not expose generic lists
        {
            var problems = new List<SqlRuleProblem>();
            this.problems = problems;
            modelElement = sqlObject;
            this.iRule = iRule;

            TSqlFragment frg;
            if (TSqlModelUtils.TryGetFragmentForAnalysis(sqlObject, out frg))
            {
                if (iRule == 23)
                {
                    foreach (var parserToken in frg.ScriptTokenStream)
                    {
                        // if (parserToken.TokenType == TSqlTokenType.SingleLineComment) SendFeedBack(23, parserToken);
                    }
                }

                ProcessTsqlFragment(frg);
            }

            return problems;
        }
    }
}
