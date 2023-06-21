using Xunit.Abstractions;

namespace Interpreter.Tests.Unit;

public class LexerTests
{
    private readonly ITestOutputHelper _output;

    public LexerTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void MoveNextToken_GivenSymbols_SuccessReturnsTokensFromSymbols()
    {
        const string input = "=+(){},;";
        Token[] expected =
        {
            new (TokenType.Assign, "="),
            new (TokenType.Plus, "+"),
            new (TokenType.OpenParen, "("),
            new (TokenType.CloseParen, ")"),
            new (TokenType.OpenBrace, "{"),
            new (TokenType.CloseBrace, "}"),
            new (TokenType.Comma, ","),
            new (TokenType.Semicolon, ";")
        };
        
        Lexer lexer = new(input);
        foreach (Token token in expected)
        {
            var actual = lexer.MoveNext();
            Assert.Equal(token, actual);
        }
    }

    [Fact]
    public void MoveNextToken_GivenMonkeySourceCode_SuccessReturnsTokensFromSourceCode()
    {
        const string input = """
            let five = 5;
            let ten = 10;
            let add = fn(x, y)
            {
                x + y;
            };
            let result = add(five, ten);
            """;
        Token[] expected =
        {
            // let five = 5;
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "five"),
            new(TokenType.Assign, "="),
            new(TokenType.Int, "5"),
            new(TokenType.Semicolon, ";"),

            // let ten = 10;
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "ten"),
            new(TokenType.Assign, "="),
            new(TokenType.Int, "10"),
            new(TokenType.Semicolon, ";"),

            // let add = fn(x, y)
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "add"),
            new(TokenType.Assign, "="),
            new(TokenType.Function, "fn"),
            new(TokenType.OpenParen, "("),
            new(TokenType.Identifier, "x"),
            new(TokenType.Comma, ","),
            new(TokenType.Identifier, "y"),
            new(TokenType.CloseParen, ")"),

            // { x + y; };
            new(TokenType.OpenBrace, "{"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Plus, "+"),
            new(TokenType.Identifier, "y"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.CloseBrace, "}"),
            new(TokenType.Semicolon, ";"),

            // let result = add(five, ten);
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "result"),
            new(TokenType.Assign, "="),
            new(TokenType.Identifier, "add"),
            new(TokenType.OpenParen, "("),
            new(TokenType.Identifier, "five"),
            new(TokenType.Comma, ","),
            new(TokenType.Identifier, "ten"),
            new(TokenType.CloseParen, ")"),
            new(TokenType.Semicolon, ";"),
        };
        
        Lexer lexer = new(input);
        foreach (Token token in expected)
        {
            var actual = lexer.MoveNext();
            Assert.Equal(token, actual);
            _output.WriteLine(actual.ToString());
        }
    }
    
    [Fact]
    public void MoveNextToken_GivenMonkeySourceCodeWithMathSymbols_SuccessReturnsTokensFromSourceCode()
    {
        const string input = """
            let five = 5;
            let ten = 10;
            let add = fn(x, y)
            {
                x + y;
            };
            let result = add(five, ten);
            !-/*5;
            5 < 10 > 5;
            """;
        Token[] expected =
        {
            // let five = 5;
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "five"),
            new(TokenType.Assign, "="),
            new(TokenType.Int, "5"),
            new(TokenType.Semicolon, ";"),

            // let ten = 10;
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "ten"),
            new(TokenType.Assign, "="),
            new(TokenType.Int, "10"),
            new(TokenType.Semicolon, ";"),

            // let add = fn(x, y)
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "add"),
            new(TokenType.Assign, "="),
            new(TokenType.Function, "fn"),
            new(TokenType.OpenParen, "("),
            new(TokenType.Identifier, "x"),
            new(TokenType.Comma, ","),
            new(TokenType.Identifier, "y"),
            new(TokenType.CloseParen, ")"),

            // { x + y; };
            new(TokenType.OpenBrace, "{"),
            new(TokenType.Identifier, "x"),
            new(TokenType.Plus, "+"),
            new(TokenType.Identifier, "y"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.CloseBrace, "}"),
            new(TokenType.Semicolon, ";"),

            // let result = add(five, ten);
            new(TokenType.Let, "let"),
            new(TokenType.Identifier, "result"),
            new(TokenType.Assign, "="),
            new(TokenType.Identifier, "add"),
            new(TokenType.OpenParen, "("),
            new(TokenType.Identifier, "five"),
            new(TokenType.Comma, ","),
            new(TokenType.Identifier, "ten"),
            new(TokenType.CloseParen, ")"),
            new(TokenType.Semicolon, ";"),
            
            // !-/*5;
            // 5 < 10 > 5;
            new(TokenType.Bang, "!"),
            new(TokenType.Minus, "-"),
            new(TokenType.Slash, "/"),
            new(TokenType.Splat, "*"),
            new(TokenType.Int, "5"),
            new(TokenType.Semicolon, ";"),
            new(TokenType.Int, "5"),
            new(TokenType.LessThan, "<"),
            new(TokenType.Int, "10"),
            new(TokenType.GreaterThan, ">"),
            new(TokenType.Int, "5"),
        };
        
        Lexer lexer = new(input);
        foreach (Token token in expected)
        {
            var actual = lexer.MoveNext();
            Assert.Equal(token, actual);
            _output.WriteLine(actual.ToString());
        }
    }
}