namespace Interpreter.Objects;

public class HashObject : IRuntimeObject
{
    
    
    public RuntimeObjectType Type => RuntimeObjectType.Hash;
    public string Inspect() => throw new NotImplementedException();
}