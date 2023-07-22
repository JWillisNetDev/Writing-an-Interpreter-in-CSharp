namespace Interpreter;

public enum TokenType
{
    Illegal, EndOfFile,
    
    // Identifiers and Literals
    Identifier, Int, String,
    
    // Operators
    Assign, Plus,
    Minus, Slash,
    Splat, Bang,
    GreaterThan,
    LessThan,
    
    // Delimiters
    OpenParen, CloseParen,
    OpenBrace, CloseBrace,
    OpenSquareBracket, CloseSquareBracket,
    Comma, Semicolon,
    Colon,
    
    // Keywords
    Function, Let,
    True, False,
    If, Else,
    Return,
    
    // Comparators
    Equals, NotEquals,
}