using Interpreter.Ast;
using Interpreter.Objects;

namespace Interpreter;

public static class Evaluator
{
    public static class Constants
    {
        public static NullObject Null { get; } = new();
        public static BooleanObject True { get; } = new(true);
        public static BooleanObject False { get; } = new(false);
    }

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
            
            case BooleanLiteral boolean:
                return BooleanNativeAsObject(boolean.Value);
            
            case PrefixExpression prefix:
                return EvaluatePrefixExpression(prefix.Operator, Evaluate(prefix.Right));
            
            case InfixExpression infix:
                return EvaluateInfixExpression(infix.Operator, Evaluate(infix.Left), Evaluate(infix.Right));
        }

        return Constants.Null;
    }
    
    private static IRuntimeObject EvaluateInfixExpression(string infixOperator, IRuntimeObject? left, IRuntimeObject? right)
    {
        if (left is IntegerObject lhs
            && right is IntegerObject rhs)
        {
            return EvaluateIntegerInfixExpression(infixOperator, lhs.Value, rhs.Value);
        }

        if (infixOperator == "==")
        {
            return BooleanNativeAsObject(left == right);
        }
        
        if (infixOperator == "!=")
        {
            return BooleanNativeAsObject(left != right);
        }

        return Constants.Null;

        IRuntimeObject EvaluateIntegerInfixExpression(string op, long lhsValue, long rhsValue) => op switch
        {
            "+" => new IntegerObject(lhsValue + rhsValue),
            "-" => new IntegerObject(lhsValue - rhsValue),
            "*" => new IntegerObject(lhsValue * rhsValue),
            "/" => new IntegerObject(lhsValue / rhsValue),
            "<" => BooleanNativeAsObject(lhsValue < rhsValue),
            ">" => BooleanNativeAsObject(lhsValue > rhsValue),
            "==" => BooleanNativeAsObject(lhsValue == rhsValue),
            "!=" => BooleanNativeAsObject(lhsValue != rhsValue),
            _ => Constants.Null,
        };
    }

    private static IRuntimeObject EvaluatePrefixExpression(string prefixOperator, IRuntimeObject? right)
    {
        return prefixOperator switch
        {
            "!" => EvaluateBangOperatorExpression(right),
            "-" => EvaluateMinusOperatorExpression(right),
            _ => Constants.Null,
        };

        IRuntimeObject EvaluateBangOperatorExpression(IRuntimeObject? r) => r switch
        {
            BooleanObject b when b == Constants.True => Constants.False,
            BooleanObject b when b == Constants.False => Constants.True,
            NullObject o => Constants.True,
            _ => Constants.False,
        };

        IRuntimeObject EvaluateMinusOperatorExpression(IRuntimeObject? r) => r switch
        {
            IntegerObject i => new IntegerObject(-i.Value),
            _ => Constants.Null,
        };
    }

    private static IRuntimeObject BooleanNativeAsObject(bool booleanValue) => booleanValue ? Constants.True : Constants.False;

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