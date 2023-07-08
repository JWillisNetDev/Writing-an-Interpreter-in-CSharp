namespace Interpreter.Ast;

// if (<condition>) <consequence> (else <alternative>)
public record IfExpression(Token Token, IExpression Condition, BlockStatement Consequence, BlockStatement? Alternative = null) : IExpression
{
    public Token Token { get; } = Token ?? throw new ArgumentNullException(nameof(Token));
    public IExpression Condition { get; } = Condition ?? throw new ArgumentNullException(nameof(Condition));
    public BlockStatement Consequence { get; } = Consequence ?? throw new ArgumentNullException(nameof(Consequence));
    public BlockStatement? Alternative { get; } = Alternative;

    public string TokenLiteral => Token.Literal;
    public void  ExpressionNode()
    {}

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.Append($"if {Condition} {Consequence}");
        if (Alternative is not null) { builder.Append($"else {Alternative}"); }
        return builder.ToString();
    }
}