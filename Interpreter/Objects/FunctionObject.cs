using Interpreter.Ast;

namespace Interpreter.Objects;

public class FunctionObject : IRuntimeObject
{
    private readonly List<Identifier> _parameters;
    
    public IList<Identifier> Parameters => _parameters;
    public BlockStatement Body { get; }
    public Environment Environment { get; }

    public FunctionObject(IEnumerable<Identifier> parameters, BlockStatement body, Environment environment)
    {
        _parameters = new List<Identifier>();
        _parameters.AddRange(parameters);
        Body = body;
        Environment = environment;
    }
    
    public RuntimeObjectType Type => RuntimeObjectType.Function;

    public string Inspect()
    {
        StringBuilder builder = new();
        builder.Append("fn(");
        builder.AppendJoin(", ", Parameters.Select(p => p.ToString()));
        builder.Append($") {{\n{Body}\n}}");
        return builder.ToString();
    }
}