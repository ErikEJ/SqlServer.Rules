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
        private readonly List<VarAssignment> _assignmentList = new List<VarAssignment>();

        private int _iRule;
        private TSqlObject _modelElement;
        private List<SqlRuleProblem> _problems;
        private readonly SelectStatementProcessor _selectStatementProcessor;
        private readonly InsertProcessor _insertProcessor;
        private readonly ExecutableEntityProcessor _executableEntityProcessor;
        private readonly FromProcessor _fromProcessor;
        private readonly WhereProcessor _whereProcessor;
        private readonly OrderByProcessor _orderByProcessor;
        private readonly WhileProcessor _whileProcessor;
        private readonly PredicateSetProcessor _predicateSetProcessor;
        private readonly SetProcessor _setProcessor;
        private readonly FunctionProcessor _functionProcessor;
        private readonly TopProcessor _topProcessor;
        private readonly CreateTableProcessor _createTableProcessor;
        private readonly SelectSetProcessor _selectSetProcessor;
        private readonly SqlDataTypeProcessor _sqlDataTypeProcessor;
        private readonly ViewStatementProcessor _viewStatementProcessor;
        private readonly SetTransactionIsolationLevelProcessor _setTransactionIsolationLevelProcessor;
        private readonly CursorProcessor _cursorProcessor;
        private readonly BeginEndBlockProcessor _beginEndBlockProcessor;
        private readonly SelectFunctionReturnTypeProcessor _selectFunctionReturnTypeProcessor;
        private readonly FunctionStatementBodyProcessor _functionStatementBodyProcessor;
        private readonly ProcedureStatementBodyProcessor _procedureStatementBodyProcessor;
        private readonly IfStatementProcessor _ifStatementProcessor;
        private readonly DeclareVariableProcessor _declareVariableProcessor;
        private readonly TableVariableProcessor _tableVariableProcessor;
        private readonly ReturnStatementProcessor _returnStatementProcessor;
        private readonly ColumnDefinitionProcessor _columnDefinitionProcessor;

        public Smells()
        {
            _selectStatementProcessor = new SelectStatementProcessor(this);
            _insertProcessor = new InsertProcessor(this);
            _executableEntityProcessor = new ExecutableEntityProcessor(this);
            _fromProcessor = new FromProcessor(this);
            _whereProcessor = new WhereProcessor(this);
            _orderByProcessor = new OrderByProcessor(this);
            _whileProcessor = new WhileProcessor(this);
            _predicateSetProcessor = new PredicateSetProcessor(this);
            _setProcessor = new SetProcessor(this);
            _functionProcessor = new FunctionProcessor(this);
            _topProcessor = new TopProcessor(this);
            _createTableProcessor = new CreateTableProcessor(this);
            _selectSetProcessor = new SelectSetProcessor(this);
            _sqlDataTypeProcessor = new SqlDataTypeProcessor(this);
            _viewStatementProcessor = new ViewStatementProcessor(this);
            _setTransactionIsolationLevelProcessor = new SetTransactionIsolationLevelProcessor(this);
            _cursorProcessor = new CursorProcessor(this);
            _beginEndBlockProcessor = new BeginEndBlockProcessor(this);
            _selectFunctionReturnTypeProcessor = new SelectFunctionReturnTypeProcessor(this);
            _functionStatementBodyProcessor = new FunctionStatementBodyProcessor(this);
            _procedureStatementBodyProcessor = new ProcedureStatementBodyProcessor(this);
            _ifStatementProcessor = new IfStatementProcessor(this);
            _declareVariableProcessor = new DeclareVariableProcessor(this);
            _tableVariableProcessor = new TableVariableProcessor(this);
            _returnStatementProcessor = new ReturnStatementProcessor(this);
            _columnDefinitionProcessor = new ColumnDefinitionProcessor(this);
        }

        public InsertProcessor InsertProcessor
        {
            get { return _insertProcessor; }
        }

        public ExecutableEntityProcessor ExecutableEntityProcessor
        {
            get { return _executableEntityProcessor; }
        }

        public FunctionProcessor FunctionProcessor
        {
            get { return _functionProcessor; }
        }

        public SelectSetProcessor SelectSetProcessor
        {
            get { return _selectSetProcessor; }
        }

#pragma warning disable CA1002 // Do not expose generic lists
        public List<VarAssignment> AssignmentList
#pragma warning restore CA1002 // Do not expose generic lists
        {
            get { return _assignmentList; }
        }

        public ProcedureStatementBodyProcessor ProcedureStatementBodyProcessor
        {
            get { return _procedureStatementBodyProcessor; }
        }

        public void SendFeedBack(int errorNum, TSqlFragment errorFrg)
        {
            if (errorNum != _iRule)
            {
                return;
            }

            var rm = Resources.ResourceManager;

            var lookup = "TSQLSmell_RuleName" + errorNum.ToString("D2", CultureInfo.InvariantCulture);
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var Out = rm.GetString(lookup, CultureInfo.InvariantCulture);
#pragma warning restore SA1312 // Variable names should begin with lower-case letter

            _problems.Add(new SqlRuleProblem(Out, _modelElement, errorFrg));
        }

        public void ProcessQueryExpression(QueryExpression queryExpression, string parentType, bool testTop = false,
            WithCtesAndXmlNamespaces cte = null)
        {
            var expressionType = FragmentTypeParser.GetFragmentType(queryExpression);
            switch (expressionType)
            {
                case "QuerySpecification":
                    // {$Query = $Stmt.QueryExpression;
                    var querySpec = (QuerySpecification)queryExpression;
                    _selectStatementProcessor.ProcessSelectElements(querySpec.SelectElements, parentType, cte);
                    if (querySpec.FromClause != null)
                    {
                        _fromProcessor.Process(querySpec.FromClause, cte);
                    }

                    if (querySpec.WhereClause != null)
                    {
                        _whereProcessor.Process(querySpec.WhereClause);
                    }

                    if (querySpec.OrderByClause != null)
                    {
                        _orderByProcessor.Process(querySpec.OrderByClause);
                        if (parentType == "VW")
                        {
                            SendFeedBack(28, querySpec.OrderByClause);
                        }
                    }

                    if (querySpec.TopRowFilter != null)
                    {
                        _topProcessor.ProcessTopFilter(querySpec.TopRowFilter);
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
                    _cursorProcessor.ProcessCursorStatement((DeclareCursorStatement)fragment);
                    break;
                case "BeginEndBlockStatement":
                    _beginEndBlockProcessor.ProcessBeginEndBlockStatement((BeginEndBlockStatement)fragment);
                    break;
                case "CreateFunctionStatement":
                case "AlterFunctionStatement":
                    _functionStatementBodyProcessor.ProcessFunctionStatementBody((FunctionStatementBody)fragment);
                    break;
                case "SelectFunctionReturnType":
                    _selectFunctionReturnTypeProcessor.ProcessSelectFunctionReturnType((SelectFunctionReturnType)fragment);
                    return;
                case "SetTransactionIsolationLevelStatement":
                    _setTransactionIsolationLevelProcessor.ProcessSetTransactionIolationLevelStatement((SetTransactionIsolationLevelStatement)fragment);
                    break;
                case "WhileStatement":
                    _whileProcessor.ProcessWhileStatement((WhileStatement)fragment);
                    break;
                case "InsertStatement":
                    InsertProcessor.Process((InsertStatement)fragment);
                    break;
                case "SelectStatement":
                    _selectStatementProcessor.Process((SelectStatement)fragment, "RG", true);
                    break;
                case "SetRowCountStatement":
                    SendFeedBack(42, fragment);
                    break;
                case "IfStatement":
                    _ifStatementProcessor.ProcessIfStatement((IfStatement)fragment);
                    break;
                case "PredicateSetStatement":
                    _predicateSetProcessor.ProcessPredicateSetStatement((PredicateSetStatement)fragment);
                    break;
                case "ExecuteStatement":
                    ExecutableEntityProcessor.ProcessExecuteStatement((ExecuteStatement)fragment);
                    break;
                case "SetIdentityInsertStatement":
                    SendFeedBack(22, fragment);
                    break;
                case "SetCommandStatement":
                    _setProcessor.ProcessSetStatement((SetCommandStatement)fragment);
                    break;

                case "CreateTableStatement":
                    _createTableProcessor.ProcessCreateTable((CreateTableStatement)fragment);
                    break;

                case "CreateProcedureStatement":
                case "AlterProcedureStatement":
                    ProcedureStatementBodyProcessor.ProcessProcedureStatementBody((ProcedureStatementBody)fragment);
                    _assignmentList.Clear();
                    break;
                case "CreateViewStatement":
                case "AlterViewStatement":
                    _viewStatementProcessor.ProcessViewStatementBody((ViewStatementBody)fragment);
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
                    _returnStatementProcessor.ProcessReturnStatement((ReturnStatement)fragment);
                    break;
                case "IntegerLiteral":
                    break;
                case "DeclareVariableStatement":
                    _declareVariableProcessor.ProcessDeclareVariableStatement((DeclareVariableStatement)fragment);
                    break;
                case "DeclareVariableElement":
                    _declareVariableProcessor.ProcessDeclareVariableElement((DeclareVariableElement)fragment);
                    break;
                case "PrintStatement":
                    break;
                case "SqlDataTypeReference":
                    _sqlDataTypeProcessor.ProcessSqlDataTypeReference((SqlDataTypeReference)fragment);
                    break;
                case "DeclareTableVariableStatement":
                    _tableVariableProcessor.ProcessTableVariableStatement((DeclareTableVariableStatement)fragment);
                    break;
                case "TableValuedFunctionReturnType":
                    _tableVariableProcessor.ProcessTableValuedFunctionReturnType((TableValuedFunctionReturnType)fragment);
                    break;
                case "DeclareTableVariableBody":
                    _tableVariableProcessor.ProcessTableVariableBody((DeclareTableVariableBody)fragment);
                    break;
                case "VariableReference":
                    // ProcessVariableReference((VariableReference)Fragment);
                    break;
                case "ExistsPredicate":
                    _tableVariableProcessor.ProcessExistsPredicate((ExistsPredicate)fragment);
                    break;

                case "ColumnDefinition":
                    _columnDefinitionProcessor.ProcessColumnDefinition((ColumnDefinition)fragment);
                    break;
            }
        }

#pragma warning disable CA1002 // Do not expose generic lists
        public List<SqlRuleProblem> ProcessObject(TSqlObject sqlObject, int iRule)
#pragma warning restore CA1002 // Do not expose generic lists
        {
            var problems = new List<SqlRuleProblem>();
            _problems = problems;
            _modelElement = sqlObject;
            _iRule = iRule;

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
