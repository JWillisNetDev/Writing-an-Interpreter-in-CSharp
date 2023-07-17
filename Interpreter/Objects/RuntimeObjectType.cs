namespace Interpreter.Objects;

public enum RuntimeObjectType
{
    None = 0,
    Error,
    Null,
    Integer,
    Boolean,
    Return,
    Function,
    String,
    Builtin,
    Array,
    Hash,
}