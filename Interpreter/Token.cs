namespace Interpreter;

public record class Token(TokenType Type, string Literal)
{
    public TokenType Type { get; } = Type;
    public string Literal { get; } = Literal ?? throw new ArgumentNullException(nameof(Literal));

    public Token(TokenType type, char charLiteral) : this(type, charLiteral.ToString())
    {
    }
}