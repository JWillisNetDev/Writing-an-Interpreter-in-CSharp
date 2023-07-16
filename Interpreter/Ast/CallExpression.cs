namespace Interpreter.Ast;

// fn(x, y) { x + y; }(2, 3)
// <expression>(<comma separated expressions>)
public record CallExpression(Token Token, IExpression Function, IReadOnlyList<IExpression> Arguments) : IExpression
{
    public CallExpression(Token token, IExpression function, IEnumerable<IExpression> arguments) : this(token, function, arguments.ToList().AsReadOnly())
    {}
    
    public Token Token { get; } = Token ?? throw new ArgumentNullException(nameof(Token));
    public IExpression Function { get; } = Function ?? throw new ArgumentNullException(nameof(Function));
    public IReadOnlyList<IExpression> Arguments { get; } = Arguments;

    public string TokenLiteral => Token.Literal;
    public void ExpressionNode()
    {
    }

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.Append($"{Function}(");
        builder.AppendJoin(", ", Arguments.Select(a => a.ToString()));
        builder.Append(')');
        return builder.ToString();
    }
}