namespace Interpreter.Objects;

public class BooleanObject : IRuntimeObject
{
    public bool Value { get; set; }

    public BooleanObject(bool value)
    {
        Value = value;
    }

    public RuntimeObjectType Type => RuntimeObjectType.BooleanObject;
    public string Inspect() => Value.ToString();
}