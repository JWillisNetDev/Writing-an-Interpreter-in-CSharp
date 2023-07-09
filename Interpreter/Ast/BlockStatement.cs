namespace Interpreter.Ast;

public record BlockStatement(Token Token, IReadOnlyList<IStatement> Statements) : IStatement
{
    public BlockStatement(Token token, IEnumerable<IStatement> statements) : this(token, statements.ToList().AsReadOnly())
    {}
    
    public Token Token { get; } = Token ?? throw new ArgumentNullException(nameof(Token));
    public IReadOnlyList<IStatement> Statements { get; } = Statements ?? throw new ArgumentNullException(nameof(Statements));

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