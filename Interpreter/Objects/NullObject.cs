namespace Interpreter.Objects;

public class NullObject : IRuntimeObject
{
    public RuntimeObjectType Type => RuntimeObjectType.Null;
    public string Inspect() => nameof(NullObject);
}