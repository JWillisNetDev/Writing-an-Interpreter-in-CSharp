namespace Interpreter.Ast;

public enum Precedence // Order matters!
{
    None = 0,
    Lowest = 0x10,
    Equals = 0x20,
    LessGreater = 0x30,
    Sum = 0x40,
    Product = 0x50,
    Prefix = 0x60,
    Call = 0x70,
    Index = 0x80,
}