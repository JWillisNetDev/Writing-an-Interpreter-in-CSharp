namespace Interpreter.Objects;

public class ReturnValueObject : IRuntimeObject
{
    public ReturnValueObject(IRuntimeObject value)
    {
        Value = value;
    }
    
    public IRuntimeObject Value { get; set; }

    public RuntimeObjectType Type => RuntimeObjectType.Return;
    public string Inspect() => Value.Inspect();
}