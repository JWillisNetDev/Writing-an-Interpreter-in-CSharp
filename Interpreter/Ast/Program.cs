namespace Interpreter.Ast;

public class Program : INode
{
    internal Program()
    {}

    private readonly List<IStatement> _statements = new();

    public IReadOnlyList<IStatement> Statements => _statements.AsReadOnly();
    
    public string TokenLiteral => Statements.Any() ? Statements[0].TokenLiteral : string.Empty;
    
    public override string ToString() => string.Join(string.Empty, Statements.Select(s => s.ToString()));

    internal void AddStatement(IStatement statement) => _statements.Add(statement);
    
    internal void AddStatements(params IStatement[] statements) => _statements.AddRange(statements);
}
