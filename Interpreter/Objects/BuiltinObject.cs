namespace Interpreter.Objects;

public class BuiltinObject : IRuntimeObject
{
    public delegate IRuntimeObject BuiltinFunction(params IRuntimeObject[] objects);

    public BuiltinFunction Function { get; }
    
    public BuiltinObject(BuiltinFunction function)
    {
        Function = function;
    }

    public RuntimeObjectType Type => RuntimeObjectType.Builtin;
    public string Inspect() => "builtin function";
}