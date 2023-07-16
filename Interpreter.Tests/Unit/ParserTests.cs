using System.Net.Security;
using Xunit.Abstractions;

namespace Interpreter.Tests.Unit;

public class ParserTests
{
    private readonly ITestOutputHelper _output;

    public ParserTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData("let x = 5;", "x", 5)]
    [InlineData("let y = true;", "y", true)]
    [InlineData("let foobar = y;", "foobar", "y")]
    public void ParseProgram_LetStatement_ParsesLetStatementIdentifierAndValue<T>(string input, string expectedIdentifier, T expectedValue)
    {
        Lexer lexer = new(input);
        Parser parser = new(lexer);
        var program = parser.ParseProgram();
        AssertCheckParserErrors(parser);

        var statement = Assert.Single(program.Statements);
        
        var letStatement = AssertCheckLetStatement(statement, expectedIdentifier);
        Assert.NotNull(letStatement.Value);
        AssertCheckLiteralExpression(letStatement.Value, expectedValue);
    }
    
    [Fact]
    public void ParseProgram_ManyLetStatements_ParsesLetStatements()
    {
        // Assign
        const string input = """
            let x = 5;
            let y = 10;
            let foobar = 838383; 
            """;
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        // Act
        Program actual = parser.ParseProgram();

        // Assert
        // Should not be null
        Assert.NotNull(actual);

        // Should have 3 let statements
        Assert.Equal(3, actual.Statements.OfType<LetStatement>().Count());
        ImmutableArray<LetStatement> letStatements = actual.Statements.OfType<LetStatement>().ToImmutableArray();

        // Should have identifiers 'x' 'y' and 'foobar'
        foreach ((int key, string expectedName)
                 in new[] { (0, "x"), (1, "y"), (2, "foobar") })
        {
            var actualStatement = letStatements[key];

            // IStatement literal should always be 'let'
            Assert.Equal("let", actualStatement.TokenLiteral);

            // IStatement should be of concrete type LetStatement
            var actualLet = Assert.IsType<LetStatement>(actualStatement);

            // LetStatement should be an actual value and have the correct identifier name
            Assert.Equal(expectedName, actualLet.Name.Value);
            Assert.Equal(expectedName, actualLet.Name.TokenLiteral);

            // Assert if any extraneous errors occurred
            AssertCheckParserErrors(parser);
        }
    }

    [Fact]
    public void ParseProgram_LetStatementsWithErrors_CausesErrorsToOccur()
    {
        const string input = """
            let x 5;
            let = 10;
            let 838383; 
            """;
        Lexer lexer = new Lexer(input);
        Parser parser = new Parser(lexer);

        Program actual = parser.ParseProgram();

        Assert.Equal(4, parser.Errors.Count);
    }

    [Fact]
    public void ParseProgram_ReturnStatements_ParsesReturnStatements()
    {
        const string input = """
            return 5;
            return 10;
            return 993322;
            """;
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        Assert.Equal(3, actual.Statements.Count);

        foreach (var statement in actual.Statements)
        {
            Assert.Equal("return", statement.TokenLiteral);
            var actualReturn = Assert.IsType<ReturnStatement>(statement);
            Assert.NotNull(actualReturn);
        }
    }

    [Fact]
    public void ParseProgram_Identifier_ParsesIdentifier()
    {
        const string input = "foobar;";
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        var statement = Assert.Single(actual.Statements);
        var actualStatement = Assert.IsType<ExpressionStatement>(statement);
        AssertCheckLiteralExpression(actualStatement.Expression, input[..^1]);
    }

    [Fact]
    public void ParseProgram_IntegerLiteralExpression_ParsesLiteralValue()
    {
        const long expected = 5L;
        string input = $"{expected};";
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        var statement = Assert.Single(actual.Statements);
        var actualStatement = Assert.IsType<ExpressionStatement>(statement);
        AssertCheckLiteralExpression(actualStatement.Expression, expected);
    }

    [Theory]
    [InlineData("!5;", "!", 5L)]
    [InlineData("-15;", "-", 15L)]
    [InlineData("!true;", "!", true)]
    [InlineData("!false;", "!", false)]
    public void ParseProgram_PrefixExpressions_ParsesOperatorAndIntLiteral<T>(string input, string expectedOperator, T expectedValue)
    {
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        var statement = Assert.Single(actual.Statements);
        var actualStatement = Assert.IsType<ExpressionStatement>(statement);
        AssertCheckPrefixExpression(actualStatement.Expression, expectedOperator, expectedValue);
    }

    [Theory]
    [InlineData("5 + 5;", 5L, "+", 5L)]
    [InlineData("5 - 5;", 5L, "-", 5L)]
    [InlineData("5 * 5;", 5L, "*", 5L)]
    [InlineData("5 / 5;", 5L, "/", 5L)]
    [InlineData("5 > 5;", 5L, ">", 5L)]
    [InlineData("5 < 5;", 5L, "<", 5L)]
    [InlineData("5 == 5;", 5L, "==", 5L)]
    [InlineData("5 != 5;", 5L, "!=", 5L)]
    [InlineData("true == true;", true, "==", true)]
    [InlineData("true != false;", true, "!=", false)]
    [InlineData("false == false;", false, "==", false)]
    public void ParseProgram_InfixExpressions_ParsesIntLiteralsAndOperator<TLeft, TRight>(string input, TLeft expectedLeft, string expectedOperator, TRight expectedRight)
    {
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        var statement = Assert.Single(actual.Statements);
        var expressionStatement = Assert.IsType<ExpressionStatement>(statement);
        AssertCheckInfixExpression(expressionStatement.Expression, expectedLeft, expectedOperator, expectedRight);
    }

    [Theory]
    [InlineData("-a * b", "((-a) * b)")]
    [InlineData("!-a", "(!(-a))")]
    [InlineData("a + b + c", "((a + b) + c)")]
    [InlineData("a + b - c", "((a + b) - c)")]
    [InlineData("a * b * c", "((a * b) * c)")]
    [InlineData("a * b / c", "((a * b) / c)")]
    [InlineData("a + b / c", "(a + (b / c))")]
    [InlineData("a + b * c + d / e - f", "(((a + (b * c)) + (d / e)) - f)")]
    [InlineData("3 + 4; -5 * 5", "(3 + 4)((-5) * 5)")]
    [InlineData("5 > 4 == 3 < 4", "((5 > 4) == (3 < 4))")]
    [InlineData("5 < 4 != 3 > 4", "((5 < 4) != (3 > 4))")]
    [InlineData("3 + 4 * 5 == 3 * 1 + 4 * 5", "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))")]
    [InlineData("true", "true")]
    [InlineData("false", "false")]
    [InlineData("3 > 5 == false", "((3 > 5) == false)")]
    [InlineData("3 < 5 == false", "((3 < 5) == false)")]
    [InlineData("1 + (2 + 3) + 4", "((1 + (2 + 3)) + 4)")]
    [InlineData("(5 + 5) * 2", "((5 + 5) * 2)")]
    [InlineData("2 / (5 + 5)", "(2 / (5 + 5))")]
    [InlineData("-(5 + 5)", "(-(5 + 5))")]
    [InlineData("!(true == true)", "(!(true == true))")]
    [InlineData("a + add(b * c) + d", "((a + add((b * c))) + d)")]
    [InlineData("add(a, b, 1, 2 * 3, 4 + 5, add(6, 7 * 8))", "add(a, b, 1, (2 * 3), (4 + 5), add(6, (7 * 8)))")]
    [InlineData("add(a + b + c * d / f + g)", "add((((a + b) + ((c * d) / f)) + g))")]
    [InlineData("a * [1, 2, 3, 4][b * c] * d",  "((a * ([1, 2, 3, 4][(b * c)])) * d)")]
    [InlineData("add(a * b[2], b[1], 2 * [1, 2][1])", "add((a * (b[2])), (b[1]), (2 * ([1, 2][1])))")]
    public void ParseProgram_OperationalOrder_OperationsParseInCorrectOrder(string input, string expected)
        // I hate this test and I /HATE/ overriding tostring on records
        // and I have an absolute seething hatred for using strings to provide testing for what can be better expressed using some more practical logical forms
        // TODO nuke
    {
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);
        
        Assert.Equal(expected, actual.ToString());
    }

    [Theory]
    [InlineData("true;", true)]
    [InlineData("false;", false)]
    public void ParseProgram_BooleanLiterals_ParsesBooleanValues(string input, bool expectedValue)
    {
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        var statement = Assert.Single(actual.Statements);
        var expression = Assert.IsType<ExpressionStatement>(statement).Expression;
        AssertCheckLiteralExpression(expression, expectedValue);
    }

    [Fact]
    public void ParseProgram_IfExpressionWithoutElse_ParsesIfExpressionWithoutAlternative()
    {
        const string input = "if (x < y) { x }";
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        var actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        var statement = Assert.Single(actual.Statements);
        var expression = Assert.IsType<ExpressionStatement>(statement).Expression;
        var ifExpression = Assert.IsType<IfExpression>(expression);
        AssertCheckInfixExpression(ifExpression.Condition, "x", "<", "y");

        var consequence = Assert.Single(ifExpression.Consequence.Statements);
        var consequenceExpression = Assert.IsType<ExpressionStatement>(consequence).Expression;
        AssertCheckLiteralExpression(consequenceExpression, "x");

        Assert.Null(ifExpression.Alternative);
    }
    
    [Fact]
    public void ParseProgram_IfExpressionWithElse_ParsesIfExpressionWithAlternative()
    {
        const string input = "if (x < y) { x } else { y }";
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        var actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        var statement = Assert.Single(actual.Statements);
        var expression = Assert.IsType<ExpressionStatement>(statement).Expression;
        var ifExpression = Assert.IsType<IfExpression>(expression);
        AssertCheckInfixExpression(ifExpression.Condition, "x", "<", "y");

        var consequence = Assert.Single(ifExpression.Consequence.Statements);
        var consequenceExpression = Assert.IsType<ExpressionStatement>(consequence).Expression;
        AssertCheckLiteralExpression(consequenceExpression, "x");

        Assert.NotNull(ifExpression.Alternative);
        var alternative = Assert.Single(ifExpression.Alternative.Statements);
        var alternativeExpression = Assert.IsType<ExpressionStatement>(alternative).Expression;
        AssertCheckLiteralExpression(alternativeExpression, "y");
    }

    [Fact]
    public void ParseProgram_FunctionLiteral_ParsesFunctionLiteral()
    {
        const string input = "fn(x, y) { x + y; }";
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        var statement = Assert.Single(actual.Statements);
        var expression = Assert.IsType<ExpressionStatement>(statement).Expression;
        
        var functionLiteral = Assert.IsType<FunctionLiteral>(expression);
        Assert.Equal(2, functionLiteral.Parameters.Count);
        AssertCheckLiteralExpression(functionLiteral.Parameters[0], "x");
        AssertCheckLiteralExpression(functionLiteral.Parameters[1], "y");

        var bodyStatement = Assert.Single(functionLiteral.Body.Statements);
        var bodyExpression = Assert.IsType<ExpressionStatement>(bodyStatement).Expression;
        AssertCheckInfixExpression(bodyExpression, "x", "+", "y");
    }

    [Theory]
    [InlineData("fn() {}", new string[] {})]
    [InlineData("fn(x) {}", new [] { "x" })]
    [InlineData("fn(x, y, z) {}", new [] { "x", "y", "z" })]
    [InlineData("fn(foo, bar) {}", new [] { "foo", "bar" })]
    public void ParseProgram_FunctionLiteralsParameters_ParsesFunctionLiteralExpectedParameters(string input, string[] expectedParams)
    {
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        var statement = Assert.Single(actual.Statements);
        var expression = Assert.IsType<ExpressionStatement>(statement).Expression;
        
        var functionLiteral = Assert.IsType<FunctionLiteral>(expression);
        Assert.Equal(expectedParams.Length, functionLiteral.Parameters.Count);
        Assert.Equivalent(expectedParams, functionLiteral.Parameters.Select(p => p.Value));
    }

    [Fact]
    public void ParseProgram_CallExpressions_ParsesCallExpressionStatementsAndArguments()
    {
        const string input = "add(1, 2 * 3, 4 + 5);";
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        var statement = Assert.Single(actual.Statements);
        var expression = Assert.IsType<ExpressionStatement>(statement).Expression;
        
        var callExpression = Assert.IsType<CallExpression>(expression);
        AssertCheckIdentifier(callExpression.Function, "add");

        Assert.Equal(3, callExpression.Arguments.Count);
        AssertCheckLiteralExpression(callExpression.Arguments[0], 1);
        AssertCheckInfixExpression(callExpression.Arguments[1], 2, "*", 3);
        AssertCheckInfixExpression(callExpression.Arguments[2], 4, "+", 5);
    }

    [Theory]
    [InlineData("rand()", "rand", new string[] { })]
    [InlineData("add(x, y, z)", "add", new [] { "x", "y", "z"})]
    [InlineData("sub(5, 7)", "sub", new [] { "5", "7"})]
    [InlineData("mul(5, 7, d)", "mul", new [] { "5", "7", "d"})]
    public void ParseProgram_CallExpressionsArguments_ParsesCallExpressionArgumentsLiterals(string input, string function, string[] arguments)
    {
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        var statement = Assert.Single(actual.Statements);
        var expression = Assert.IsType<ExpressionStatement>(statement).Expression;
        
        var callExpression = Assert.IsType<CallExpression>(expression);
        AssertCheckIdentifier(callExpression.Function, function);

        Assert.Equal(arguments.Length, callExpression.Arguments.Count);
        Assert.Equivalent(arguments, callExpression.Arguments.Select(a => a.TokenLiteral));
    }

    [Theory]
    [InlineData(@"""Hello, world!""", "Hello, world!")]
    public void ParseProgram_StringLiterals_ParsesStringLiteral(string input, string expected)
    {
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program program = parser.ParseProgram();
        AssertCheckParserErrors(parser);

        var expression = Assert.IsType<ExpressionStatement>(Assert.Single(program.Statements)).Expression;
        var literal = Assert.IsType<StringLiteral>(expression);
        Assert.Equal(expected, literal.Value);
    }

    [Fact]
    public void ParseProgram_ArrayLiterals_ParsesArrayElements()
    {
        const string input = "[1, 2 * 2, 3 + 3]";
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        var expression = Assert.IsType<ExpressionStatement>(Assert.Single(actual.Statements)).Expression;
        var array = Assert.IsType<ArrayLiteral>(expression);
        Assert.Equal(3, array.Elements.Count);
        AssertCheckLiteralExpression(array.Elements[0], 1);
        AssertCheckInfixExpression(array.Elements[1], 2, "*", 2);
        AssertCheckInfixExpression(array.Elements[2], 3, "+", 3);
    }

    [Fact]
    public void ParseProgram_IndexExpressions_ParsesIndexExpressions()
    {
        const string input = "myArray[1 + 1]";
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        var expression = Assert.IsType<ExpressionStatement>(Assert.Single(actual.Statements)).Expression;
        var index = Assert.IsType<IndexExpression>(expression);
        AssertCheckIdentifier(index.Left, "myArray");
        AssertCheckInfixExpression(index.Index, 1, "+", 1);
    }

    private void AssertCheckParserErrors(Parser parser)
    {
        foreach (string error in parser.Errors) { _output.WriteLine(error); }
        Assert.Equal(0, parser.Errors.Count);
    }

    private T AssertCheckLiteralExpression<T>(IExpression expression, T expected) => expected switch
        // This atrocity is okay because it's in a test :thumbsup:
        {
            int i => (T)(object)(int)AssertCheckIntegerLiteral(expression, i),
            long i => (T)(object)AssertCheckIntegerLiteral(expression, i),
            string s => (T)(object)AssertCheckIdentifier(expression, s),
            bool b => (T)(object)AssertCheckBooleanLiteral(expression, b),
            _ => throw new NotImplementedException(),
        };

    private long AssertCheckIntegerLiteral(IExpression expression, long expectedValue)
    {
        var actualIntLiteral = Assert.IsType<IntegerLiteral>(expression);
        Assert.Equal(expectedValue, actualIntLiteral.Value);
        Assert.Equal(expectedValue.ToString(), actualIntLiteral.TokenLiteral);
        return actualIntLiteral.Value;
    }

    private string AssertCheckIdentifier(IExpression expression, string value)
    {
        var actualIdentifier = Assert.IsType<Identifier>(expression);
        Assert.Equal(value, actualIdentifier.Value);
        Assert.Equal(value, actualIdentifier.TokenLiteral);
        return actualIdentifier.Value;
    }

    private bool AssertCheckBooleanLiteral(IExpression expression, bool expectedValue)
    {
        var actualBoolLiteral = Assert.IsType<BooleanLiteral>(expression);
        Assert.Equal(expectedValue, actualBoolLiteral.Value);
        Assert.Equal(expectedValue.ToString().ToLowerInvariant(), actualBoolLiteral.TokenLiteral);
        return actualBoolLiteral.Value;
    }

    private void AssertCheckPrefixExpression<T>(IExpression expression, string expectedOperator, T expectedValue)
    {
        var prefixExpression = Assert.IsType<PrefixExpression>(expression);
        Assert.Equal(expectedOperator, prefixExpression.Operator);
        AssertCheckLiteralExpression(prefixExpression.Right, expectedValue);
    }

    private void AssertCheckInfixExpression<TLeft, TRight>(IExpression expression, TLeft expectedLeft, string expectedOperator, TRight expectedRight)
    {
        var operatorExpression = Assert.IsType<InfixExpression>(expression);
        AssertCheckLiteralExpression(operatorExpression.Left, expectedLeft);
        Assert.Equal(expectedOperator, operatorExpression.Operator);
        AssertCheckLiteralExpression(operatorExpression.Right, expectedRight);
    }

    private LetStatement AssertCheckLetStatement(IStatement statement, string expectedIdentifier)
    {
        var actual = Assert.IsType<LetStatement>(statement);
        AssertCheckIdentifier(actual.Name, expectedIdentifier); // TODO null
        return actual;
    }
}