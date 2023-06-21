using System.Collections.Immutable;

namespace Interpreter;

public class Lexer
{
    public static IReadOnlyDictionary<string, TokenType> Keywords { get; } = new Dictionary<string, TokenType>
    {
        { "fn", TokenType.Function },
        { "let", TokenType.Let },
    };
    public static ImmutableArray<char> WhitespaceChars { get; } = new[] { ' ', '\t', '\r', '\n' }.ToImmutableArray();

    public string Input { get; }

    private int _position; // The current position being read
    private int _readPosition; // The next character to be read
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
            return ParseKeyword();
        }

        if (IsDigit(_current))
        {
            return ParseNumber();
        }

        Token token = ParseSymbol();
        
        return token;
    }
    private void SkipWhitespace()
    {
        while (WhitespaceChars.Contains(_current))
        {
            ReadChar();
        }
    }

    private Token ParseKeyword()
    {
        string literal = ReadIdentifier();
        TokenType type = LookupIdentifier(literal);
        return new Token(type, literal);
    }

    private Token ParseSymbol()
    {
        string literal = _current.ToString();
        TokenType type = _current switch
        {
            '=' => TokenType.Assign,
            ';' => TokenType.Semicolon,
            '(' => TokenType.OpenParen,
            ')' => TokenType.CloseParen,
            '{' => TokenType.OpenBrace,
            '}' => TokenType.CloseBrace,
            ',' => TokenType.Comma,
            '+' => TokenType.Plus,
            '\0' => TokenType.EndOfFile,
            _ => TokenType.Illegal
        };
        ReadChar();
        return new Token(type, literal);
    }

    private Token ParseNumber()
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

    private static TokenType LookupIdentifier(string identifier) => Keywords.TryGetValue(identifier, out TokenType type) ? type : TokenType.Identifier;
    
    private static bool IsDigit(char c) => char.IsDigit(c);
    private static bool IsLetterOrUnderscore(char c) => char.IsLetter(c) || c == '_';
}