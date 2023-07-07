namespace Interpreter.Ast;

public record class ExpressionStatement(Token Token, IExpression Expression) : IStatement
{
    public Token Token { get; } = Token ?? throw new ArgumentNullException(nameof(Token));
    public IExpression Expression { get; } = Expression;

    public string TokenLiteral => Token.Literal;
    public void StatementNode()
    {
    }

    public override string ToString() => $"{Expression?.ToString() ?? string.Empty}";
} 