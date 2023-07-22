using System.Diagnostics.CodeAnalysis;
using Interpreter.Objects;

namespace Interpreter;

public static class Builtins
{
    private static readonly Dictionary<string, BuiltinObject> _builtins = new()
    {
        ["len"] = new BuiltinObject(Len),
        ["first"] = new BuiltinObject(First),
        ["last"] = new BuiltinObject(Last),
        ["rest"] = new BuiltinObject(Rest),
        ["push"] = new BuiltinObject(Push),
        ["puts"] = new BuiltinObject(Puts)
    };
    public static BuiltinObject Get(string name) => _builtins[name];

    public static bool TryGet(string name, [NotNullWhen(true)] out BuiltinObject? value) => _builtins.TryGetValue(name, out value);

    private static IRuntimeObject First(IRuntimeObject[] args) => args switch
    {
        [ArrayObject { Elements.Count: > 0 } arrObj] => arrObj.Elements[0],
        not null when IsArgumentCountMismatch(args, 1, out var err) => err,
        [{ Type: not RuntimeObjectType.Array } obj] => new RuntimeErrorObject($"argument to `first` expected={RuntimeObjectType.Array}, got={obj.Type}"),
        _ => Evaluator.RuntimeConstants.Null,
    };

    private static IRuntimeObject Last(IRuntimeObject[] args) => args switch
    {
        [ArrayObject { Elements.Count: > 0 } arrObj] => arrObj.Elements[^1],
        not null when IsArgumentCountMismatch(args, 1, out var err) => err,
        [{ Type: not RuntimeObjectType.Array } obj] => new RuntimeErrorObject($"argument to `first` expected={RuntimeObjectType.Array}, got={obj.Type}"),
        _ => Evaluator.RuntimeConstants.Null,
    };

    private static IRuntimeObject Len(IRuntimeObject[] args) => args switch
    {
        [ArrayObject arrObj] => new IntegerObject(arrObj.Elements.Count),
        [StringObject strObj] => new IntegerObject(strObj.Value.Length),
        not null when IsArgumentCountMismatch(args, 1, out var err) => err,
        [{ } arg] => new RuntimeErrorObject($"argument to `len` not supported, got {arg.Type}"),
        _ => Evaluator.RuntimeConstants.Null,
    };

    private static IRuntimeObject Push(IRuntimeObject[] args) => args switch
    {
        [ArrayObject array, { } toPush] => new ArrayObject(array.Elements.Append(toPush)),
        not null when IsArgumentCountMismatch(args, 2, out var err) => err,
        [{ Type: not RuntimeObjectType.Array } target, ..] => new RuntimeErrorObject($"argument to `first` expected={RuntimeObjectType.Array}, got={target.Type}"),
        _ => throw new InvalidOperationException(),
    };

    private static IRuntimeObject Puts(IRuntimeObject[] objects)
    {
        foreach (var @object in objects)
        {
            Evaluator.StandardOut.WriteLine(@object.Inspect());
        }
        return Evaluator.RuntimeConstants.Null;
    }

    private static IRuntimeObject Rest(IRuntimeObject[] args) => args switch
    {
        [ArrayObject { Elements.Count: > 0 } arrObj] => new ArrayObject(arrObj.Elements.Skip(1)),
        not null when IsArgumentCountMismatch(args, 1, out var err) => err,
        [{ Type: not RuntimeObjectType.Array } obj] => new RuntimeErrorObject($"argument to `first` expected={RuntimeObjectType.Array}, got={obj.Type}"),
        _ => Evaluator.RuntimeConstants.Null,
    };

    private static bool IsArgumentCountMismatch(Span<IRuntimeObject> args, int expected, [NotNullWhen(true)] out RuntimeErrorObject? error)
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