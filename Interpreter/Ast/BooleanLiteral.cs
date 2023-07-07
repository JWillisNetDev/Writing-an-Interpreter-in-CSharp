namespace Interpreter.Ast;

public record BooleanLiteral(Token Token, bool Value) : IExpression
{
    public Token Token { get; } = Token ?? throw new ArgumentNullException(nameof(Token));
    public bool Value { get; } = Value;

    public string TokenLiteral => Token.Literal;
    public void ExpressionNode()
    {
    }

    public override string ToString() => TokenLiteral;
}