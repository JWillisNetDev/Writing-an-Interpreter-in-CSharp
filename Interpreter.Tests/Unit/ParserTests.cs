using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Interpreter.Ast;
using Xunit.Abstractions;

namespace Interpreter.Tests.Unit;

public class ParserTests
{
    private readonly ITestOutputHelper _output;

    public ParserTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ParseProgram_LetStatements_ParsesLetStatements()
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

        foreach (IStatement statement in actual.Statements)
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
        var actualExpression = Assert.IsType<ExpressionStatement>(statement);
        AssertCheckLiteralExpression(actualExpression.Expression, expectedValue);
    }

    private void AssertCheckParserErrors(Parser parser)
    {
        foreach (string error in parser.Errors) { _output.WriteLine(error); }
        Assert.Equal(0, parser.Errors.Count);
    }

    private void AssertCheckLiteralExpression<T>(IExpression expression, T expected)
    {
        switch (expected)
        {
            case int i:
                AssertCheckIntegerLiteral(expression, i);
                break;
            case long l:
                AssertCheckIntegerLiteral(expression, l);
                break;
            case string s:
                AssertCheckIdentifier(expression, s);
                break;
            case bool b:
                AssertCheckBooleanLiteral(expression, b);
                break;
            default:
                Assert.Fail($"Encountered unanticipated literal expression `{expression}`");
                break;
        }
    }

    private void AssertCheckIntegerLiteral(IExpression expression, long expectedValue)
    {
        var actualIntLiteral = Assert.IsType<IntegerLiteral>(expression);
        Assert.Equal(expectedValue, actualIntLiteral.Value);
        Assert.Equal(expectedValue.ToString(), actualIntLiteral.TokenLiteral);
    }

    private void AssertCheckIdentifier(IExpression expression, string value)
    {
        var actualIdentifier = Assert.IsType<Identifier>(expression);
        Assert.Equal(value, actualIdentifier.Value);
        Assert.Equal(value, actualIdentifier.TokenLiteral);
    }

    private void AssertCheckBooleanLiteral(IExpression expression, bool expectedValue)
    {
        var actualBoolLiteral = Assert.IsType<BooleanLiteral>(expression);
        Assert.Equal(expectedValue, actualBoolLiteral.Value);
        Assert.Equal(expectedValue.ToString().ToLowerInvariant(), actualBoolLiteral.TokenLiteral);
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
}