using System.Collections.Immutable;
using System.Linq.Expressions;
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
        Identifier actualIdentifier = Assert.IsType<Identifier>(actualStatement.Expression);
        Assert.Multiple(() =>
        {
            Assert.Equal("foobar", actualIdentifier.Value);
            Assert.Equal("foobar", actualIdentifier.TokenLiteral);
        });
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
        var integerLiteral = Assert.IsType<IntegerLiteral>(actualStatement.Expression);
        Assert.Equal(expected, integerLiteral.Value);
        Assert.Equal($"{expected}", integerLiteral.TokenLiteral);
    }

    [Theory]
    [InlineData("!5;", "!", 5L)]
    [InlineData("-15;", "-", 15L)]
    public void ParseProgram_PrefixExpressions_ParsesOperatorAndIntLiteral(string input, string expectedOperator, long expectedValue)
    {
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        var statement = Assert.Single(actual.Statements);
        var actualStatement = Assert.IsType<ExpressionStatement>(statement);
        var prefixExpression = Assert.IsType<PrefixExpression>(actualStatement.Expression);
        Assert.Equal(expectedOperator, prefixExpression.Operator);
        AssertCheckIntegerLiteral(prefixExpression.Right, expectedValue);
    }

    [Theory]
    [InlineData("5 + 5;", 5L, 5L, "+")]
    [InlineData("5 - 5;", 5L, 5L, "-")]
    [InlineData("5 * 5;", 5L, 5L, "*")]
    [InlineData("5 / 5;", 5L, 5L, "/")]
    [InlineData("5 > 5;", 5L, 5L, ">")]
    [InlineData("5 < 5;", 5L, 5L, "<")]
    [InlineData("5 == 5;", 5L, 5L, "==")]
    [InlineData("5 != 5;", 5L, 5L, "!=")]
    public void ParseProgram_InfixExpressions_ParsesIntLiteralsAndOperator(string input, long leftValue, long rightValue, string expectedOperator)
    {
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        var statement = Assert.Single(actual.Statements);
        var expressionStatement = Assert.IsType<ExpressionStatement>(statement);
        var infixExpression = Assert.IsType<InfixExpression>(expressionStatement.Expression);
        Assert.Equal(expectedOperator, infixExpression.Operator);
        AssertCheckIntegerLiteral(infixExpression.Left, leftValue);
        AssertCheckIntegerLiteral(infixExpression.Right, rightValue);
    }

    [Theory]
    [InlineData("-a * b", "((-a) * b)")]
    [InlineData("!-a", "(!(-a))")]
    [InlineData("a + b + c", "((a + b) + c)")]
    [InlineData("a + b - c", "((a + b) - c)")]
    [InlineData("a * b * c", "((a * b) * c)")]
    [InlineData("a * b / c", "((a * b) / c)")]
    [InlineData("a + b / c", "(a + (b / c))")]
    [InlineData("a + b * c + d / e - f", "(((a + (b * c)) +  (d / e)) - f)")]
    [InlineData("3 + 4; -5 * 5", "(3 + 4)((-5) * 5)")]
    [InlineData("5 > 4 == 3 < 4", "((5 > 4) == (3 < 4))")]
    [InlineData("5 < 4 != 3 > 4", "((5 < 4) != (3 > 4))")]
    [InlineData("3 + 4 * 5 == 3 * 1 + 4 * 5", "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))")]
    public void ParseProgram_OperationalOrder_OperationsParseInCorrectOrder(string input, string expected)
    {
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);
        
        
    }


    private void AssertCheckParserErrors(Parser parser)
    {
        foreach (string error in parser.Errors) { _output.WriteLine(error); }
        Assert.Equal(0, parser.Errors.Count);
    }

    private void AssertCheckIntegerLiteral(IExpression expression, long expectedValue)
    {
        var integerLiteral = Assert.IsType<IntegerLiteral>(expression);
        Assert.Equal(expectedValue, integerLiteral.Value);
        Assert.Equal($"{expectedValue}", integerLiteral.TokenLiteral);
    }
}