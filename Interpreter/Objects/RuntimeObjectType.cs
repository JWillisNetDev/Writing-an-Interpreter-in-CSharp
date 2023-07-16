namespace Interpreter.Objects;

public enum RuntimeObjectType
{
    None = 0,
    ErrorObject,
    NullObject,
    IntegerObject,
    BooleanObject,
    ReturnObject,
    FunctionObject,
    StringObject,
    Builtin,
}