namespace Interpreter.Objects;

public class ArrayObject : IRuntimeObject
{
    private readonly List<IRuntimeObject> _elements;
    
    public IReadOnlyList<IRuntimeObject> Elements => _elements;

    public ArrayObject(IEnumerable<IRuntimeObject> objects)
    {
        _elements = new List<IRuntimeObject>();
        _elements.AddRange(objects);
    }
    
    public RuntimeObjectType Type => RuntimeObjectType.Array;

    public string Inspect()
    {
        StringBuilder builder = new();
        builder.Append('[');
        builder.AppendJoin(", ", Elements.Select(e => e.Inspect()));
        builder.Append(']');
        return builder.ToString();
    }
}