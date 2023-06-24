using Interpreter.Ast;

namespace Interpreter;

public class Parser
{
    private readonly Lexer _lexer;
    private readonly List<string> _errors = new();
    private readonly Dictionary<TokenType, Func<IExpression, IExpression>> _infixParsers = new();
    private readonly Dictionary<TokenType, Func<IExpression>> _prefixParsers = new();
    
    public Token Current { get; private set; } = null!; // Null warning suppression: Calling NextToken() twice in the constructor will set both values.
    public Token Next { get; private set; } = null!;
    public IReadOnlyCollection<string> Errors => _errors;

    public IReadOnlyDictionary<TokenType, Func<IExpression, IExpression>> InfixParsers => _infixParsers.AsReadOnly();
    public IReadOnlyDictionary<TokenType, Func<IExpression>> PrefixParsers => _prefixParsers.AsReadOnly();

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        NextToken();
        NextToken();
        
        RegisterPrefix(TokenType.Identifier, () => new Identifier(Current, Current.Literal));
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

    private void NextToken()
    {
        Current = Next;
        Next = _lexer.MoveNext();
    }

    private void RegisterPrefix(TokenType type, Func<IExpression> prefixParser) => _prefixParsers[type] = prefixParser;

    private IStatement? ParseStatement()
    {
        switch (Current.Type)
        {
            case TokenType.Let:
                return ParseLetStatement();
            case TokenType.Return:
                return ParseReturnStatement();
            default:
                return ParseExpressionStatement();
        }
    }

    private LetStatement? ParseLetStatement()
    {
        Token token = Current;

        if (!ExpectPeek(TokenType.Identifier)) { return null; }
        Identifier name = new(Current, Current.Literal);
        
        // TODO Skipping expressions for now.
        if (!ExpectPeek(TokenType.Assign)) { return null; }
        while (CurrentTokenIs(TokenType.Semicolon)) { NextToken(); }

        return new LetStatement(Token: token, Name: name, Value: null); // TODO null
    }

    private ReturnStatement? ParseReturnStatement()
    {
        Token token = Current;
        
        // TODO Skipping expressions for now
        while (!CurrentTokenIs(TokenType.Semicolon)) { NextToken(); }

        return new ReturnStatement(Token: token, ReturnValue: null); // TODO null
    }

    private ExpressionStatement ParseExpressionStatement()
    {
        Token token = Current;
        IExpression expression = ParseExpression(Precedence.Lowest)!; // TODO null
        
        if (NextTokenIs(TokenType.Semicolon)) { NextToken(); }
        
        return new ExpressionStatement(token, expression);
    }

    public IExpression? ParseExpression(Precedence precedence)
    {
        if (_prefixParsers.TryGetValue(Current.Type, out var prefixParser))
        {
            IExpression left = prefixParser();
            return left;
        }
        return null;
    }
    
    private bool ExpectPeek(TokenType type)
    {
        if (NextTokenIs(type))
        {
            NextToken();
            return true;
        }
        PeekError(type);
        return false;
    }
    
    private bool NextTokenIs(TokenType type) => Next.Type == type;
    
    private void PeekError(TokenType type) => _errors.Add($"expected next token to be `{type}`, but got `{Next.Type}` instead.");
}
