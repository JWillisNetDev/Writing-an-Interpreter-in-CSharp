namespace Interpreter;

public class Lexer
{
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
        Token token = _current switch
        {
            '=' => new(TokenType.Assign, _current.ToString()),
            ';' => new(TokenType.Semicolon, _current.ToString()),
            '(' => new(TokenType.OpenParen, _current.ToString()),
            ')' => new(TokenType.CloseParen, _current.ToString()),
            '{' => new(TokenType.OpenBrace, _current.ToString()),
            '}' => new(TokenType.CloseBrace, _current.ToString()),
            ',' => new(TokenType.Comma, _current.ToString()),
            '+' => new(TokenType.Plus, _current.ToString()),
            '\0' => new(TokenType.EndOfFile, _current.ToString()),
            
            { } when IsLetterOrUnderscore(_current) => new(TokenType.Identifier, ReadIdentifier()),
            
            _ => new(TokenType.Illegal, _current.ToString())
        };
        ReadChar();
        
        return token;
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

    private static bool IsLetterOrUnderscore(char c) => char.IsLetter(c) || c == '_';
}