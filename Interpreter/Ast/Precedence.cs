namespace Interpreter.Ast;

public enum Precedence // Order matters!
{
    None = 0,
    Lowest = 100,
    Equals = 200,
    LessGreater = 300,
    Sum = 400,
    Product = 500,
    Prefix = 600,
    Call = 700,
}