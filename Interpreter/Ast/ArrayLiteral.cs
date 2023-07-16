namespace Interpreter.Ast;

public record ArrayLiteral(Token Token, IReadOnlyList<IExpression> Elements) : IExpression
{
    public ArrayLiteral(Token token, IEnumerable<IExpression> elements) : this(token, elements.ToList().AsReadOnly())
    {}
    
    public Token Token { get; } = Token ?? throw new ArgumentNullException(nameof(Token));
    public IReadOnlyList<IExpression> Elements { get; } = Elements ?? throw new ArgumentNullException(nameof(Elements));
    
    public string TokenLiteral => Token.Literal;
    
    public void ExpressionNode()
    {}

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.Append('[');
        builder.AppendJoin(", ", Elements.Select(e => e.ToString()));
        builder.Append(']');
        return builder.ToString();
    }
}