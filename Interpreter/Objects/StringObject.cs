namespace Interpreter.Objects;

public class StringObject : IRuntimeObject
{
    public string Value { get; set; }

    public StringObject(string value)
    {
        Value = value;
    }

    public RuntimeObjectType Type => RuntimeObjectType.StringObject;
    public string Inspect() => Value;
}