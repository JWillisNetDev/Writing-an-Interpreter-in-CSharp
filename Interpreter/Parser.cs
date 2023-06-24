using Interpreter.Ast;

namespace Interpreter;

public class Parser
{
    private readonly Lexer _lexer;

    private readonly List<string> _errors = new();
    
    public Token Current { get; private set; } = null!; // Null warning suppression: Calling NextToken() twice in the constructor will set both values.
    public Token Peek { get; private set; } = null!;
    public IReadOnlyCollection<string> Errors => _errors;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        NextToken();
        NextToken();
    }

    public Program ParseProgram()
    {
        Program program = new();

        while (!CurrentTokenIs(TokenType.EndOfFile))
        {
            IStatement? statement = ParseStatement();
            if (statement is not null)
            {
                program.Statements.Add(statement);
            }
            NextToken();
        }

        return program;
    }

    private bool CurrentTokenIs(TokenType type) => Current.Type == type;
    
    private bool ExpectPeek(TokenType type)
    {
        if (PeekTokenIs(type))
        {
            NextToken();
            return true;
        }
        return false;
    }

    private void NextToken()
    {
        Current = Peek;
        Peek = _lexer.MoveNext();
    }

    private LetStatement? ParseLetStatement()
    {
        Token token = Current;

        if (!ExpectPeek(TokenType.Identifier)) { return null; }
        Identifier name = new(Current, Current.Literal);

        if (!ExpectPeek(TokenType.Assign)) { return null; }
        while (CurrentTokenIs(TokenType.Semicolon)) { NextToken(); }

        return new(Token: token, Name: name, Value: null); // TODO null
    }

    private IStatement? ParseStatement()
    {
        switch (Current.Type)
        {
            case TokenType.Let:
                return ParseLetStatement();
            default:
                return null;
        }
    }
    
    private void PeekError(TokenType type) => _errors.Add($"expected next token to be `{type}`, but got `{Peek.Type}` instead.");
    
    private bool PeekTokenIs(TokenType type) => Peek.Type == type;
}
