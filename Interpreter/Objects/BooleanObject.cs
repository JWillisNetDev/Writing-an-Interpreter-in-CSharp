namespace Interpreter.Objects;

public class BooleanObject : IRuntimeObject, IHashable
{
    public bool Value { get; set; }

    public BooleanObject(bool value)
    {
        Value = value;
    }

    public RuntimeObjectType Type => RuntimeObjectType.Boolean;

    public HashKey GetHash() => new(Type, Convert.ToUInt64(Value));
    
    public string Inspect() => Value.ToString();
}