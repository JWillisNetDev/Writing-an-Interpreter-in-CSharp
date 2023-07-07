namespace Interpreter.Ast;

public record ReturnStatement(Token Token, IExpression? ReturnValue) : IStatement
{
    public Token Token { get; } = Token ?? throw new ArgumentNullException(nameof(Token));
    public IExpression? ReturnValue { get; } = ReturnValue; // TODO Implement expressions
    
    public string TokenLiteral => Token.Literal;
    public void StatementNode()
    {
    }
    
    public override string ToString() => $"{TokenLiteral} {ReturnValue?.ToString() ?? string.Empty};";
}