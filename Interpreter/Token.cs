namespace Interpreter;

public record class Token(TokenType Type, string Literal)
{
    public TokenType Type { get; } = Type;
    public string Literal { get; } = Literal ?? throw new ArgumentNullException(nameof(Literal));
}