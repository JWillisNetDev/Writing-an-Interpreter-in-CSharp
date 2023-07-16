namespace Interpreter.Objects;

public interface IRuntimeObject
{
    RuntimeObjectType Type { get; }
    string Inspect();
}