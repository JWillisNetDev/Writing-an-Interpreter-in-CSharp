namespace Interpreter.Objects;

public class NullObject : IRuntimeObject
{
    public RuntimeObjectType Type => RuntimeObjectType.NullObject;
    public string Inspect() => nameof(NullObject);
}