namespace Interpreter.Ast;

public record BlockStatement(Token Token, IEnumerable<IStatement> Statements) : IStatement
{
    public Token Token { get; } = Token ?? throw new ArgumentNullException(nameof(Token));
    public IEnumerable<IStatement> Statements { get; } = Statements;

    public string TokenLiteral => Token.Literal;
    public void StatementNode()
    {}

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.AppendJoin(null, Statements.Select(s => s.ToString()));
        return builder.ToString();
    }
}