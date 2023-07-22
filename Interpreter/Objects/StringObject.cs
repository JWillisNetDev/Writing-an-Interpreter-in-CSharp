namespace Interpreter.Objects;

public class StringObject : IRuntimeObject, IHashable
{
    public string Value { get; set; }

    public StringObject(string value)
    {
        Value = value;
    }

    public RuntimeObjectType Type => RuntimeObjectType.String;

    public string Inspect() => Value;
    
    public HashKey GetHashKey() => new(Type, Convert.ToInt64(Value.GetHashCode()));
}