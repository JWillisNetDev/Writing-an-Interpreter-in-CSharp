namespace Interpreter.Objects;

public class RuntimeErrorObject : IRuntimeObject
{
    public RuntimeErrorObject(string? message)
    {
        Message = message;
    }
    
    public string? Message { get; set; }

    public RuntimeObjectType Type => RuntimeObjectType.Error;
    public string Inspect() => $"ERROR: {Message}";
}