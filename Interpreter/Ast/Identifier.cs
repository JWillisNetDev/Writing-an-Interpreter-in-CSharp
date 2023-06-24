namespace Interpreter.Ast;

public record class Identifier(Token Token, string Value) : IExpression
{
    public Token Token { get; } = Token ?? throw new ArgumentNullException(nameof(Token));
    public string Value { get; } = Value ?? throw new ArgumentNullException(nameof(Value));

    public string TokenLiteral => Token.Literal;
    public void ExpressionNode()
    {
    }
}