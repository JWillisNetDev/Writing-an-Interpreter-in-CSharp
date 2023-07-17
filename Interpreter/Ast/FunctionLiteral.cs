namespace Interpreter.Ast;

// fn(x, y) { return x + y; }
// fn() { return foo + bar; }
// fn() { return fn(x, y) { return x > y; }; }
// let add = fn(x, y) { return x + y; }
// myFunc(x, y, fn(x, y) { return x > y; });
// fn (<params | empty>) <block statement>
public record FunctionLiteral(Token Token, IReadOnlyList<Identifier> Parameters, BlockStatement Body) : IExpression
{
    public FunctionLiteral(Token token, IEnumerable<Identifier> parameters, BlockStatement body) : this(token, parameters.ToList().AsReadOnly(), body)
    {}
    
    public Token Token { get; } = Token ?? throw new ArgumentNullException(nameof(Token));
    public IReadOnlyList<Identifier> Parameters { get; } = Parameters;
    public BlockStatement Body { get; } = Body ?? throw new ArgumentNullException(nameof(Body));

    public string TokenLiteral => Token.Literal;
    public void ExpressionNode()
    {}

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.Append($"{TokenLiteral}(");
        builder.AppendJoin(", ", Parameters.Select(p => p.ToString()));
        builder.Append($") {Body}");
        return builder.ToString();
    }
}