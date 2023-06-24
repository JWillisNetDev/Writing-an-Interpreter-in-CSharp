using Interpreter.Ast;

namespace Interpreter;

public class Parser
{
    private readonly Lexer _lexer;

    // Null warning suppression: Calling NextToken() twice in the constructor will set both values.
    public Token Current { get; private set; } = null!;
    public Token Peek { get; private set; } = null!;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        NextToken();
        NextToken();
    }

    public void NextToken()
    {
        Current = Peek;
        Peek = _lexer.MoveNext();
    }

    public Program ParseProgram()
    {
        Program program = new();

        while (Current.Type != TokenType.EndOfFile)
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

    private LetStatement? ParseLetStatement()
    {
        Token token = Current;

        if (!ExpectPeek(TokenType.Identifier)) { return null; }
        Identifier name = new(Current, Current.Literal);

        if (!ExpectPeek(TokenType.Assign)) { return null; }
        while (CurrentTokenIs(TokenType.Semicolon)) { NextToken(); }

        return new(Token: token, Name: name, Value: null); // TODO null
    }

    private bool CurrentTokenIs(TokenType type) => Current.Type == type;
    private bool PeekTokenIs(TokenType type) => Peek.Type == type;
    private bool ExpectPeek(TokenType type)
    {
        if (PeekTokenIs(type))
        {
            NextToken();
            return true;
        }
        return false;
    }
}