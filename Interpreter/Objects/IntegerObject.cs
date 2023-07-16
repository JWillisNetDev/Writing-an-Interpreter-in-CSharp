namespace Interpreter.Objects;

public class IntegerObject: IRuntimeObject
{
    public long Value { get; set; }
    
    public IntegerObject(long value)
    {
        Value = value;
    }
    
    public RuntimeObjectType Type => RuntimeObjectType.IntegerObject;
    public string Inspect() => Value.ToString();
}