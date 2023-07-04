namespace Interpreter.Ast;

public record IntegerLiteral(Token Token, long Value) : IExpression
{
    public Token Token { get; } = Token ?? throw new ArgumentNullException(nameof(Token));
    public long Value { get; } = Value;

    public string TokenLiteral => Token.Literal;
    public void ExpressionNode()
    {
    }
}