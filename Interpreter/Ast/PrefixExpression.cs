namespace Interpreter.Ast;

public record PrefixExpression(Token Token, string Operator, IExpression Right) : IExpression
{
    public Token Token { get; } = Token ?? throw new ArgumentNullException(nameof(Token));
    public string Operator { get; } = Operator ?? throw new ArgumentNullException(nameof(Operator));
    public IExpression Right { get; } = Right ?? throw new ArgumentNullException(nameof(Right));
    
    public string TokenLiteral => Token.Literal;
    public void ExpressionNode()
    {
    }

    public override string ToString() => $"({Operator}{Right})";
}