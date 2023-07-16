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
    public static IReadOnlyDictionary<TokenType, Precedence> Precedences { get; } = new Dictionary<TokenType, Precedence>()
    {
        { TokenType.Equals, Precedence.Equals },
        { TokenType.NotEquals, Precedence.Equals },
        { TokenType.LessThan, Precedence.LessGreater },
        { TokenType.GreaterThan, Precedence.LessGreater },
        { TokenType.Plus, Precedence.Sum },
        { TokenType.Minus, Precedence.Sum },
        { TokenType.Splat, Precedence.Product },
        { TokenType.Slash, Precedence.Product },
        { TokenType.OpenParen, Precedence.Call },
    };

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        NextToken();
        NextToken();
        
        // Register prefixers
        RegisterPrefix(TokenType.Identifier, ParseIdentifier);
        RegisterPrefix(TokenType.Int, ParseIntegerLiteralExpression!); // TODO null
        RegisterPrefix(TokenType.Bang, ParsePrefixExpression!); // TODO null
        RegisterPrefix(TokenType.Minus, ParsePrefixExpression!); // TODO null
        RegisterPrefix(TokenType.True, ParseBooleanLiteral);
        RegisterPrefix(TokenType.False, ParseBooleanLiteral);
        RegisterPrefix(TokenType.OpenParen, ParseGroupedExpression!); // TODO null
        RegisterPrefix(TokenType.If, ParseIfExpression!);
        RegisterPrefix(TokenType.Function, ParseFunctionLiteral!);
        RegisterPrefix(TokenType.String, ParseStringLiteral!);
        RegisterPrefix(TokenType.OpenSquareBracket, ParseArrayLiteral);
        
        // Register infixers
        RegisterInfix(TokenType.Plus, ParseInfixExpression);
        RegisterInfix(TokenType.Minus, ParseInfixExpression);
        RegisterInfix(TokenType.Splat, ParseInfixExpression);
        RegisterInfix(TokenType.Slash, ParseInfixExpression);
        RegisterInfix(TokenType.Equals, ParseInfixExpression);
        RegisterInfix(TokenType.NotEquals, ParseInfixExpression);
        RegisterInfix(TokenType.LessThan, ParseInfixExpression);
        RegisterInfix(TokenType.GreaterThan, ParseInfixExpression);
        RegisterInfix(TokenType.OpenParen, ParseCallExpression);
    }

    public Program ParseProgram()
    {
        Program program = new();

        while (!CurrentTokenIs(TokenType.EndOfFile))
        {
            IStatement? statement = ParseStatement();
            if (statement is not null)
            {
                program.AddStatement(statement);
            }
            NextToken();
        }

        return program;
    }

    private ArrayLiteral ParseArrayLiteral()
    {
        var token = Current;
        var elements = ParseExpressionList(TokenType.CloseSquareBracket);
        return new ArrayLiteral(token, elements!); // TODO nulls
    }
    
    private List<IExpression>? ParseExpressionList(TokenType closingToken)
    {
        List<IExpression> expressions = new();
        if (NextTokenIs(closingToken))
        {
            NextToken();
            return expressions;
        }
        
        NextToken();
        expressions.Add(ParseExpression(Precedence.Lowest)!); // TODO nulls
        while (NextTokenIs(TokenType.Comma))
        {
            NextToken();
            NextToken();
            expressions.Add(ParseExpression(Precedence.Lowest)!); // TODO nulls
        }

        return ExpectNext(closingToken) ? expressions : null; // TODO nulls
    }

    private StringLiteral ParseStringLiteral() => new StringLiteral(Current, Current.Literal);

    private CallExpression ParseCallExpression(IExpression function)
    {
        var token = Current;
        var arguments = ParseExpressionList(TokenType.CloseParen)!; // TODO null
        return new CallExpression(token, function, arguments);
    }
    
    private FunctionLiteral? ParseFunctionLiteral()
    {
        var token = Current;
        if (!ExpectNext(TokenType.OpenParen)) { return null; } // TODO null
        var parameters = ParseFunctionParameters();
        if (parameters is null) { return null; } // TODO null

        if (!ExpectNext(TokenType.OpenBrace)) { return null; } // TODO null
        var body = ParseBlockStatement();

        return new FunctionLiteral(token, parameters, body);
        
        List<Identifier>? ParseFunctionParameters()
        {
            List<Identifier> identifiers = new();
            
            if (NextTokenIs(TokenType.CloseParen))
            {
                NextToken();
                return identifiers;
            }
            
            NextToken();
            identifiers.Add(ParseIdentifier());
            while (NextTokenIs(TokenType.Comma))
            {
                NextToken();
                NextToken();
                identifiers.Add(ParseIdentifier());
            }
            
            return ExpectNext(TokenType.CloseParen) ?
                identifiers :
                null; // TODO null
        }
    }

    private IfExpression? ParseIfExpression()
    {
        var token = Current;
        if (!ExpectNext(TokenType.OpenParen)) { return null; } // TODO null
        NextToken();
        var condition = ParseExpression(Precedence.Lowest)!;
        if (!ExpectNext(TokenType.CloseParen)) { return null; } // TODO null
        
        if (!ExpectNext(TokenType.OpenBrace)) { return null; } // TODO null
        var consequence = ParseBlockStatement();

        BlockStatement? alternative = null;
        if (NextTokenIs(TokenType.Else))
        {
            NextToken();
            if (!ExpectNext(TokenType.OpenBrace)) { return null; } // TODO null
            alternative = ParseBlockStatement();
        }

        return new IfExpression(token, condition, consequence, alternative);
    }
    
    private BlockStatement ParseBlockStatement()
    {
        var token = Current;
        List<IStatement> statements = new();

        NextToken();

        while (!CurrentTokenIs(TokenType.CloseBrace)
               && !CurrentTokenIs(TokenType.EndOfFile))
        {
            var statement = ParseStatement();
            if (statement is not null) { statements.Add(statement); }
            NextToken();
        }
        return new BlockStatement(token, statements);
    }
    
    private IExpression? ParseGroupedExpression()
    {
        NextToken();
        var exp = ParseExpression(Precedence.Lowest);
        return ExpectNext(TokenType.CloseParen) ?
            exp :
            null;
    }
    
    private Identifier ParseIdentifier() => new Identifier(Current, Current.Literal);

    private BooleanLiteral ParseBooleanLiteral() => new BooleanLiteral(Current, CurrentTokenIs(TokenType.True));

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

        if (!ExpectNext(TokenType.Assign)) { return null; }
        NextToken();

        var value = ParseExpression(Precedence.Lowest);

        if (NextTokenIs(TokenType.Semicolon)) { NextToken(); } // Optional semicolon
        
        return new LetStatement(token, name, value); // TODO null
    }

    // return <expression|identifier>;
    private ReturnStatement ParseReturnStatement()
    {
        Token token = Current;

        NextToken();
        var returnValue = ParseExpression(Precedence.Lowest)!; // TODO null
        if (NextTokenIs(TokenType.Semicolon)) { NextToken(); } // Optional semicolon

        return new ReturnStatement(token, returnValue);
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
