namespace Interpreter;

public enum TokenType
{
    Illegal, EndOfFile,
    
    // Identifiers and Literals
    Identifier, Int,
    
    // Operators
    Assign, Plus,
    
    // Delimiters
    Comma, Semicolon,
    OpenParen, CloseParen,
    OpenBrace, CloseBrace,
    
    // Keywords
    Function, Let
}