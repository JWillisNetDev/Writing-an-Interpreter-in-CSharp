using System.Diagnostics.CodeAnalysis;

namespace Interpreter.Objects;

public static class Builtins
{
    private static readonly Dictionary<string, BuiltinObject> _builtins = new()
    {
        ["len"] = new BuiltinObject(Len),
    };

    public static BuiltinObject Get(string name) => _builtins[name];

    public static bool TryGet(string name, [NotNullWhen(true)] out BuiltinObject? value) => _builtins.TryGetValue(name, out value);

    private static IRuntimeObject Len(IRuntimeObject[] args)
    {
        if (args is [StringObject obj]) { return new IntegerObject(obj.Value.Length); }
        if (GetIsArgumentCountMismatch(args, 1, out var err)) { return err; }
        return new RuntimeErrorObject($"argument to `len` not supported, got {args[0].Type}");
    }

    private static bool GetIsArgumentCountMismatch(Span<IRuntimeObject> args, int expected, [NotNullWhen(true)] out RuntimeErrorObject? error)
    {
        if (args.Length != expected)
        {
            error = new RuntimeErrorObject($"wrong number of arguments. got={args.Length}, wanted={expected}");
            return true;
        }
        error = null;
        return false;
    }
}