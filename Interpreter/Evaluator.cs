using Interpreter.Ast;
using Interpreter.Objects;

namespace Interpreter;

public static class Evaluator
{
    public static IRuntimeObject? Evaluate(INode node)
    {
        switch (node)
        {
            // Statements
            case Program program:
                return EvaluateStatements(program.Statements);
            
            case ExpressionStatement statement:
                return Evaluate(statement.Expression);
            
            // Expressions
            case IntegerLiteral integer:
                return new IntegerObject(integer.Value);
        }

        return null;
    }
    
    private static IRuntimeObject? EvaluateStatements(IEnumerable<IStatement> programStatements)
    {
        IRuntimeObject? result = null;
        foreach (var statement in programStatements)
        {
            result = Evaluate(statement);
        }
        return result;
    }
}