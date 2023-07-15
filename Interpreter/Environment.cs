using System.Diagnostics.CodeAnalysis;
using Interpreter.Objects;

namespace Interpreter;

public class Environment
{
    private Dictionary<string, IRuntimeObject> _variableStore = new();

    public IRuntimeObject this[string index]
    {
        get => Get(index);
        set => Set(index, value);
    }

    public IRuntimeObject Get(string index) => _variableStore[index];

    public bool TryGet(string index, [NotNullWhen(true)] out IRuntimeObject? value) => _variableStore.TryGetValue(index, out value);

    public IRuntimeObject Set(string index, IRuntimeObject value) => _variableStore[index] = value;
}