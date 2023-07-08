using Interpreter.Ast;

namespace Interpreter;

public class Parser
{
    // Private fields
    private readonly Lexer _lexer;
    private readonly List<string> _errors = new();
    private readonly Dictionary<TokenType, Func<IExpression, IExpression>> _infixParsers = new();
    private readonly Dictionary<TokenType, Func<IExpression>> _prefixParsers = new();

    // Public auto-properties
    public Token Current { get; private set; } = null!; // Null warning suppression: Calling NextToken() in the constructor will set this value
    public Token Next { get; private set; } = null!; // Null warning suppression: Calling NextToken() twice in the constructor will set this value
    
    // Public readonly properties
    public IReadOnlyCollection<string> Errors => _errors;
    public IReadOnlyDictionary<TokenType, Func<IExpression, IExpression>> InfixParsers => _infixParsers.AsReadOnly();
    public IReadOnlyDictionary<TokenType, Func<IExpression>> PrefixParsers => _prefixParsers.AsReadOnly();
    
    // Public readonly static properties
    public static IReadOnlyDictionary<TokenType, Precedence> Precedences => new Dictionary<TokenType, Precedence>()
    {
        { TokenType.Equals, Precedence.Equals },
        { TokenType.NotEquals, Precedence.Equals },
        { TokenType.LessThan, Precedence.LessGreater },
        { TokenType.GreaterThan, Precedence.LessGreater },
        { TokenType.Plus, Precedence.Sum },
        { TokenType.Minus, Precedence.Sum },
        { TokenType.Splat, Precedence.Product },
        { TokenType.Slash, Precedence.Product },
    };

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        NextToken();
        NextToken();
        
        // Register prefixers
        RegisterPrefix(TokenType.Identifier, () => new Identifier(Current, Current.Literal));
        RegisterPrefix(TokenType.Int, ParseIntegerLiteralExpression!); // TODO null
        RegisterPrefix(TokenType.Bang, ParsePrefixExpression!); // TODO null
        RegisterPrefix(TokenType.Minus, ParsePrefixExpression!); // TODO null
        RegisterPrefix(TokenType.True, ParseBooleanLiteral);
        RegisterPrefix(TokenType.False, ParseBooleanLiteral);
        RegisterPrefix(TokenType.OpenParen, ParseGroupedExpression!); // TODO null
        
        // Register infixers
        RegisterInfix(TokenType.Plus, ParseInfixExpression);
        RegisterInfix(TokenType.Minus, ParseInfixExpression);
        RegisterInfix(TokenType.Splat, ParseInfixExpression);
        RegisterInfix(TokenType.Slash, ParseInfixExpression);
        RegisterInfix(TokenType.Equals, ParseInfixExpression);
        RegisterInfix(TokenType.NotEquals, ParseInfixExpression);
        RegisterInfix(TokenType.LessThan, ParseInfixExpression);
        RegisterInfix(TokenType.GreaterThan, ParseInfixExpression);
    }
    private IExpression? ParseGroupedExpression()
    {
        NextToken();
        var exp = ParseExpression(Precedence.Lowest);
        return ExpectNext(TokenType.CloseParen) ?
            exp :
            null;
    }

    private IExpression ParseBooleanLiteral() => new BooleanLiteral(Current, CurrentTokenIs(TokenType.True));

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

    private void RegisterInfix(TokenType type, Func<IExpression, IExpression> infixParser) => _infixParsers[type] = infixParser;

    private IStatement? ParseStatement() => Current.Type switch
    {
        TokenType.Let => ParseLetStatement(),
        TokenType.Return => ParseReturnStatement(),
        _ => ParseExpressionStatement(),
    };
    
    // let <identifier> = <expression>
    private LetStatement? ParseLetStatement()
    {
        Token token = Current;

        if (!ExpectNext(TokenType.Identifier)) { return null; }
        Identifier name = new(Current, Current.Literal);
        
        // TODO Skipping expressions for now.
        if (!ExpectNext(TokenType.Assign)) { return null; }
        while (CurrentTokenIs(TokenType.Semicolon)) { NextToken(); }

        return new LetStatement(Token: token, Name: name, Value: null); // TODO null
    }

    // return <expression|identifier>;
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

    private IExpression? ParseExpression(Precedence precedence)
    {
        if (!_prefixParsers.ContainsKey(Current.Type))
        {
            _errors.Add($"Failed to parse prefix operator for token `{Current}` - no parse functionality exists for token.");
            return null;
        }
        Func<IExpression> prefixParser = _prefixParsers[Current.Type];
        IExpression leftExpression = prefixParser();

        while (!NextTokenIs(TokenType.Semicolon) && precedence < NextPrecedence())
        {
            if (!_infixParsers.ContainsKey(Next.Type))
            {
                _errors.Add($"Failed to parse infix operator for token `{Current}` - no parse functionality exists for token.");
                return null;
            }

            Func<IExpression, IExpression> infixParser = _infixParsers[Next.Type];
            NextToken();
            leftExpression = infixParser(leftExpression);
        }

        return leftExpression;
    }

    // [0-9]
    private IExpression? ParseIntegerLiteralExpression()
    {
        if (long.TryParse(Current.Literal, out long value))
        {
            return new IntegerLiteral(Current, value);
        }
        _errors.Add($"Could not parse {Current} as an integer value (64-bit)");
        return null;
    }
    
    // <prefix><expression>
    private IExpression? ParsePrefixExpression()
    {
        var token = Current;
        
        NextToken();
        IExpression? right = ParseExpression(Precedence.Prefix);
        return new PrefixExpression(token, token.Literal, right);
    }
    
    // <expression> <infix-operator> <expression>
    private IExpression ParseInfixExpression(IExpression left)
    {
        var token = Current;
        
        var precedence = CurrentPrecedence();
        NextToken();
        var right = ParseExpression(precedence);
        return new InfixExpression(token, left, right, token.Literal);
    }

    private Precedence GetPrecedence(TokenType tokenType) => Precedences.ContainsKey(tokenType) ?
        Precedences[tokenType] :
        Precedence.Lowest;

    private Precedence CurrentPrecedence() => GetPrecedence(Current.Type);
    
    private Precedence NextPrecedence() => GetPrecedence(Next.Type);
    
    private bool ExpectNext(TokenType type)
    {
        if (NextTokenIs(type))
        {
            NextToken();
            return true;
        }
        ErrorNext(type);
        return false;
    }
    
    private bool NextTokenIs(TokenType type) => Next.Type == type;
    
    private void ErrorNext(TokenType type) => _errors.Add($"expected next token to be `{type}`, but got `{Next.Type}` instead.");
}
