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
        
        // Should have 3 statements
        Assert.Equal(3, actual.Statements.Count);

        // Should have identifiers 'x' 'y' and 'foobar'
        foreach ((int key, string expectedName)
                 in new [] { (0, "x"), (1, "y"), (2, "foobar") })
        {
            var actualStatement = actual.Statements[key];
            
            // IStatement literal should always be 'let'
            Assert.Equal("let", actualStatement.TokenLiteral);

            // IStatement should be of concrete type LetStatement
            Assert.IsType<LetStatement>(actualStatement);
            var actualLet = actualStatement as LetStatement;
            
            // LetStatement should be an actual value and have the correct identifier name.
            Assert.NotNull(actualLet);
            Assert.Equal(expectedName, actualLet.Name.Value);
            Assert.Equal(expectedName, actualLet.Name.TokenLiteral);
        }
    }
}