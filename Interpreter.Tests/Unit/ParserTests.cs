using System.Collections.Immutable;
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
    public void ParseProgram_LetStatements_ReturnsProgram()
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
                 in new [] { (0, "x"), (1, "y"), (2, "foobar") })
        {
            var actualStatement = letStatements[key];
            
            // IStatement literal should always be 'let'
            Assert.Equal("let", actualStatement.TokenLiteral);

            // IStatement should be of concrete type LetStatement
            Assert.IsType<LetStatement>(actualStatement);
            var actualLet = (LetStatement)actualStatement;
            
            // LetStatement should be an actual value and have the correct identifier name
            Assert.NotNull(actualLet);
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
        
        Assert.Equal(3, parser.Errors.Count);
    }

    [Fact]
    public void ParseProgram_ReturnStatements_ReturnsProgram()
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
            
            Assert.IsType<ReturnStatement>(statement);
            var actualReturn = (ReturnStatement)statement;
            
            Assert.NotNull(actualReturn);
        }
    }

    [Fact]
    public void ParseProgram_Identifier_ReturnsProgram()
    {
        const string input = "foobar;";
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();
        
        AssertCheckParserErrors(parser);

        Assert.Single(actual.Statements);
        var statement = actual.Statements.Single();
        
        Assert.IsType<ExpressionStatement>(statement);
        var actualStatement = (ExpressionStatement)statement;

        Assert.IsType<Identifier>(actualStatement.Expression);
        Identifier actualIdentifier = (Identifier)actualStatement.Expression;
        Assert.Multiple(() =>
        {
            Assert.Equal("foobar", actualIdentifier.Value);
            Assert.Equal("foobar", actualIdentifier.TokenLiteral);
        });
    }

    [Fact]
    public void ParseProgram_IntegerLiteralExpression_ReturnsProgram()
    {
        const long expected = 5L;
        string input = $"{expected};";
        Lexer lexer = new(input);
        Parser parser = new(lexer);

        Program actual = parser.ParseProgram();

        AssertCheckParserErrors(parser);

        Assert.Single(actual.Statements);
        var statement = actual.Statements.Single();
        
        Assert.IsType<ExpressionStatement>(statement);
        var actualStatement = (ExpressionStatement)statement;

        Assert.IsType<IntegerLiteral>(actualStatement.Expression);
        var integerLiteral = (IntegerLiteral)actualStatement.Expression;
        Assert.NotNull(integerLiteral);
        Assert.Equal(expected, integerLiteral.Value);
        Assert.Equal($"{expected}", integerLiteral.TokenLiteral);
    }

    private void AssertCheckParserErrors(Parser parser)
    {
        foreach (string error in parser.Errors) { _output.WriteLine(error); }
        Assert.Equal(0, parser.Errors.Count);
    }
}