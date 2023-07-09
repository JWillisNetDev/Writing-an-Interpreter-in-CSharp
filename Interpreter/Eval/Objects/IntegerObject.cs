namespace Interpreter.Eval.Objects;

public record IntegerObject(int Value) : IRuntimeObject
{
    public int Value { get; } = Value;
    
    public RuntimeObjectType Type => RuntimeObjectType.IntegerObject;
    public string Inspect() => Value.ToString();
}