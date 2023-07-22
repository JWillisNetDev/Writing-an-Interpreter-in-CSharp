namespace Interpreter.Ast;

public record HashLiteral(Token Token, IReadOnlyDictionary<IExpression, IExpression> Pairs) : IExpression
{
    public Token Token { get; } = Token ?? throw new ArgumentNullException(nameof(Token));
    public IReadOnlyDictionary<IExpression, IExpression> Pairs { get; } = Pairs ?? throw new ArgumentNullException(nameof(Pairs));


    public string TokenLiteral => Token.Literal;
    
    public void ExpressionNode()
    {}

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.Append('{');
        builder.AppendJoin(", ", Pairs.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        builder.Append('}');
        return builder.ToString();
    }
}