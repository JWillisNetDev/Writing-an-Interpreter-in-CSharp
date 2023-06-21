namespace Interpreter;

public enum TokenType
{
    Illegal, EndOfFile,
    
    // Identifiers and Literals
    Identifier, Int,
    
    // Operators
    Assign, Plus,
    Minus, Slash,
    Splat, Bang,
    GreaterThan,
    LessThan,
    
    // Delimiters
    OpenParen, CloseParen,
    OpenBrace, CloseBrace,
    Comma, Semicolon,
    
    // Keywords
    Function, Let,
}