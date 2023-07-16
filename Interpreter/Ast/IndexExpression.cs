namespace Interpreter.Ast;

public record IndexExpression(Token Token, IExpression Left, IExpression Index) : IExpression
{
    public Token Token { get; } = Token ?? throw new ArgumentNullException(nameof(Token));
    public IExpression Left { get; } = Left ?? throw new ArgumentNullException(nameof(Left));
    public IExpression Index { get; } = Index ?? throw new ArgumentNullException(nameof(Index));

    public string TokenLiteral => Token.Literal;
    
    public void ExpressionNode()
    {}

    public override string ToString() => $"({Left}[{Index}])";
}