namespace Interpreter.Objects;

public class IntegerObject: IRuntimeObject
{
    public int Value { get; set; }
    
    public IntegerObject(int value)
    {
        Value = value;
    }
    
    public RuntimeObjectType Type => RuntimeObjectType.IntegerObject;
    public string Inspect() => Value.ToString();
}