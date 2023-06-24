namespace Interpreter.Ast;

public class Program : INode
{
    public List<IStatement> Statements { get; } = new();

    public string TokenLiteral => Statements.Any() ? Statements[0].TokenLiteral : string.Empty;
}