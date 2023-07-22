namespace Interpreter.Objects;

public class HashObject : IRuntimeObject
{
    public record HashPair(IRuntimeObject Key, IRuntimeObject Value);

    private readonly Dictionary<HashKey, HashPair> _pairs = new();
    public IReadOnlyDictionary<HashKey, HashPair> Pairs => _pairs;

    public RuntimeObjectType Type => RuntimeObjectType.Hash;

    public string Inspect()
    {
        StringBuilder builder = new();
        builder.Append('{');
        builder.AppendJoin(",\n", Pairs.Select(kvp => $"\t[#{kvp.Key.Value:x}] {kvp.Value.Key.Inspect()}: {kvp.Value.Value.Inspect()}"));
        builder.Append('}');
        return builder.ToString();
    }
}