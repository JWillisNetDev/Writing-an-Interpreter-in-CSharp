namespace Interpreter.Ast;

public record class LetStatement(Token Token, Identifier Name, IExpression? Value) : IStatement
{
    public Token Token { get; } = Token ?? throw new ArgumentNullException(nameof(Token));
    public Identifier Name { get; } = Name ?? throw new ArgumentNullException(nameof(Name));
    public IExpression? Value { get; } = Value;

    public string TokenLiteral => Token.Literal;
    public void StatementNode()
    {
    }
}
