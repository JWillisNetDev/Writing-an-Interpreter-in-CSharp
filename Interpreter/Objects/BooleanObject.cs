namespace Interpreter.Objects;

public class BooleanObject : IRuntimeObject, IHashable
{
    public bool Value { get; set; }

    public BooleanObject(bool value)
    {
        Value = value;
    }

    public RuntimeObjectType Type => RuntimeObjectType.Boolean;

    public HashKey GetHashKey() => new(Type, Value ? 1L : 0L);
    
    public string Inspect() => Value.ToString();
}