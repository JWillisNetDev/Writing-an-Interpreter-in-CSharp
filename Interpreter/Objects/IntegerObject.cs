namespace Interpreter.Objects;

public class IntegerObject: IRuntimeObject, IHashable
{
    public long Value { get; set; }
    
    public IntegerObject(long value)
    {
        Value = value;
    }
    
    public RuntimeObjectType Type => RuntimeObjectType.Integer;
    
    public string Inspect() => Value.ToString();
    
    public HashKey GetHash() => new(Type, Convert.ToUInt64(Value));
}