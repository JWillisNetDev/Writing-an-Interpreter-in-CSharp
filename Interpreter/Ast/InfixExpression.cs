namespace Interpreter.Ast;

public record InfixExpression(Token Token, IExpression Left, IExpression Right, string Operator) : IExpression
{
    public Token Token { get; } = Token ?? throw new ArgumentNullException(nameof(Token));
    public IExpression Left { get; } = Left ?? throw new ArgumentNullException(nameof(Left));
    public IExpression Right { get; } = Right ?? throw new ArgumentNullException(nameof(Right));
    public string Operator { get; } = Operator ?? throw new ArgumentNullException(nameof(Operator));

    public string TokenLiteral => Token.Literal;
    public void ExpressionNode()
    {
    }

    public override string ToString() => $"({Left} {Operator} {Right})";
}