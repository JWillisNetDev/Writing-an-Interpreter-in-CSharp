using System.Diagnostics.CodeAnalysis;
using Interpreter.Ast;

namespace Interpreter;

public class Parser
{
    private readonly Lexer _lexer;

    // Null warning suppression: Calling NextToken() twice in the constructor will set both values.
    public Token Current { get; private set; } = null!;
    public Token Peek { get; private set; } = null!;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        NextToken();
        NextToken();
    }

    public void NextToken()
    {
        Current = Peek;
        Peek = _lexer.MoveNext();
    }

    public Program ParseProgram()
    {
        throw new NotImplementedException();
    }
}