namespace Interpreter.Ast;

public enum Precedence
{
    None = 0,
    Lowest,
    Equals,
    LessGreater,
    Sum,
    Product,
    Prefix,
    Call,
}