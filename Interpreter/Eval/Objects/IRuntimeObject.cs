namespace Interpreter.Eval.Objects;

public interface IRuntimeObject
{
    RuntimeObjectType Type { get; }
    string Inspect();
}