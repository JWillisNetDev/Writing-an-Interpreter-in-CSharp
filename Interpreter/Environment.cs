using System.Diagnostics.CodeAnalysis;
using Interpreter.Objects;

namespace Interpreter;

public class Environment
{
    private Dictionary<string, IRuntimeObject> _variableStore = new();
    public Environment? Outer { get; }

    public Environment(Environment? outer = null)
    {
        Outer = outer;
    }
    
    public IRuntimeObject this[string index]
    {
        get => Get(index);
        set => Set(index, value);
    }

    public IRuntimeObject Get(string index)
    {
        if (!_variableStore.ContainsKey(index) && Outer is not null) { return Outer.Get(index); }
        return _variableStore[index];
    }

    public bool TryGet(string index, [NotNullWhen(true)] out IRuntimeObject? value)
    {
        if (_variableStore.TryGetValue(index, out value)) { return true; }
        return Outer is not null && Outer.TryGet(index, out value);
    }

    public IRuntimeObject Set(string index, IRuntimeObject value) => _variableStore[index] = value;
}