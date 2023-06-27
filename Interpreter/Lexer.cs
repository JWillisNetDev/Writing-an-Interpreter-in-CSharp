using System.Collections.Immutable;

namespace Interpreter;

public class Lexer
{
    public static IReadOnlyDictionary<string, TokenType> Keywords { get; } = new Dictionary<string, TokenType>
    {
        { "fn", TokenType.Function },
        { "let", TokenType.Let },
        { "if", TokenType.If },
        { "else", TokenType.Else },
        { "true", TokenType.True },
        { "false", TokenType.False },
        { "return", TokenType.Return }
    };

    public static ImmutableArray<char> WhitespaceChars { get; } = new[] { ' ', '\t', '\r', '\n' }.ToImmutableArray();

    public string Input { get; }

    private int _position; // The current position being read
    private int _readPosition; // The next position to be read
    private char _current; // Current character being read
    
    public Lexer(string input)
    {
        Input = input;
        ReadChar();
    }

    public Token MoveNext()
    {
        SkipWhitespace();
        if (IsLetterOrUnderscore(_current))
        {
            return ScanKeyword();
        }
        if (IsDigit(_current))
        {
            return ScanNumber();
        }
        
        return ScanOperator();
    }
    private void SkipWhitespace()
    {
        while (WhitespaceChars.Contains(_current))
        {
            ReadChar();
        }
    }

    private Token ScanKeyword()
    {
        string literal = ReadIdentifier();
        TokenType type = LookupIdentifier(literal);
        return new Token(type, literal);
    }

    private Token ScanOperator()
    {
        string literal = _current.ToString();
        Token token;
        switch (_current) 
        {
            case '\0':
                token = new Token(TokenType.EndOfFile, literal);
                break;
            case '=':
                if (PeekChar() == '=')
                {
                    ReadChar();
                    literal += _current;
                    token = new Token(TokenType.Equals, literal);
                }
                else { token = new Token(TokenType.Assign, literal); }
                break;
            case ';':
                token = new Token(TokenType.Semicolon, literal);
                break;
            case '(':
                token = new Token(TokenType.OpenParen ,literal);
                break;
            case ')':
                token = new Token(TokenType.CloseParen ,literal);
                break;
            case '{':
                token = new Token(TokenType.OpenBrace ,literal);
                break;
            case '}':
                token = new Token(TokenType.CloseBrace ,literal);
                break;
            case ',':
                token = new Token(TokenType.Comma ,literal);
                break;
            case '+':
                token = new Token(TokenType.Plus ,literal);
                break;
            case '-':
                token = new Token(TokenType.Minus ,literal);
                break;
            case '/':
                token = new Token(TokenType.Slash ,literal);
                break;
            case '*':
                token = new Token(TokenType.Splat ,literal);
                break;
            case '!':
                if (PeekChar() == '=')
                {
                    ReadChar();
                    literal += _current;
                    token = new Token(TokenType.NotEquals, literal);
                }
                else { token = new Token(TokenType.Bang, literal); }
                break;
            case '>':
                token = new Token(TokenType.GreaterThan ,literal);
                break;
            case '<':
                token = new Token(TokenType.LessThan ,literal);
                break;
            default:
                token = new Token(TokenType.Illegal, _current);
                break;
        }
        ReadChar();

        return token;
    }

    private Token ScanNumber()
    {
        string literal = ReadNumber();
        TokenType type = TokenType.Int;
        return new Token(type, literal);
    }

    private string ReadNumber()
    {
        int startIndex = _position;
        while (IsDigit(_current))
        {
            ReadChar();
        }

        return Input[startIndex.._position];
    }
    
    private string ReadIdentifier()
    {
        int startIndex = _position;
        while (IsLetterOrUnderscore(_current))
        {
            ReadChar();
        }

        return Input[startIndex.._position];
    }
    
    private void ReadChar()
    {
        if (_readPosition >= Input.Length)
        {
            _current = '\0';
        }
        else
        {
            _current = Input[_readPosition];
        }

        _position = _readPosition++;
    }

    private char PeekChar()
    {
        if (_readPosition >= Input.Length)
        {
            return '\0';
        }

        return Input[_readPosition];
    }

    private static TokenType LookupIdentifier(string identifier) => Keywords.TryGetValue(identifier, out TokenType type) ? type : TokenType.Identifier;
    
    private static bool IsDigit(char c) => char.IsDigit(c);
    private static bool IsLetterOrUnderscore(char c) => char.IsLetter(c) || c == '_';
}
