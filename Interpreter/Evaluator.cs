using Interpreter.Ast;
using Interpreter.Objects;

namespace Interpreter;

public static class Evaluator
{
    public static class RuntimeConstants
    {
        public static NullObject Null { get; } = new();
        public static BooleanObject True { get; } = new(true);
        public static BooleanObject False { get; } = new(false);
    }

    public static IRuntimeObject Evaluate(INode node)
    {
        switch (node)
        {
            // Statements
            case Program program:
                return EvaluateProgram(program);
            
            case ExpressionStatement statement:
                return Evaluate(statement.Expression);
            
            case BlockStatement statement:
                return EvaluateBlockStatement(statement);
            
            case ReturnStatement statement:
                return new ReturnValueObject(Evaluate(statement.ReturnValue));
            
            // Expressions
            case IntegerLiteral integer:
                return new IntegerObject(integer.Value);
            
            case BooleanLiteral boolean:
                return BooleanNativeAsObject(boolean.Value);
            
            case PrefixExpression prefix:
                return EvaluatePrefixExpression(prefix.Operator, Evaluate(prefix.Right));
            
            case InfixExpression infix:
                return EvaluateInfixExpression(infix.Operator, Evaluate(infix.Left), Evaluate(infix.Right));
            
            case IfExpression ifExpr:
                return EvaluateIfExpression(ifExpr);
        }

        return RuntimeConstants.Null;
    }

    private static IRuntimeObject EvaluateIfExpression(IfExpression ifExpr)
    {
        var condition = Evaluate(ifExpr.Condition);
        
        if (IsTruthy(condition))
        {
            return Evaluate(ifExpr.Consequence);
        }
        else if (ifExpr.Alternative is not null)
        {
            return Evaluate(ifExpr.Alternative);
        }
        return RuntimeConstants.Null;
    }

    private static bool IsTruthy(IRuntimeObject? obj) => obj switch
    {
        not null when obj == RuntimeConstants.Null => false,
        BooleanObject b => b.Value,
        null => false,
        _ => true,
    };
    
    private static IRuntimeObject EvaluateBlockStatement(BlockStatement block)
    {
        IRuntimeObject result = RuntimeConstants.Null;
        foreach (var statement in block.Statements)
        {
            result = Evaluate(statement);
            if (result is ReturnValueObject)
            {
                return result;
            }
        }
        return result;
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

        return RuntimeConstants.Null;

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
            _ => RuntimeConstants.Null,
        };
    }

    private static IRuntimeObject EvaluatePrefixExpression(string prefixOperator, IRuntimeObject? right)
    {
        return prefixOperator switch
        {
            "!" => EvaluateBangOperatorExpression(right),
            "-" => EvaluateMinusOperatorExpression(right),
            _ => RuntimeConstants.Null,
        };

        IRuntimeObject EvaluateBangOperatorExpression(IRuntimeObject? r) => r switch
        {
            BooleanObject b when b == RuntimeConstants.True => RuntimeConstants.False,
            BooleanObject b when b == RuntimeConstants.False => RuntimeConstants.True,
            NullObject o => RuntimeConstants.True,
            _ => RuntimeConstants.False,
        };

        IRuntimeObject EvaluateMinusOperatorExpression(IRuntimeObject? r) => r switch
        {
            IntegerObject i => new IntegerObject(-i.Value),
            _ => RuntimeConstants.Null,
        };
    }

    private static IRuntimeObject BooleanNativeAsObject(bool booleanValue) => booleanValue ? RuntimeConstants.True : RuntimeConstants.False;

    private static IRuntimeObject EvaluateProgram(Program program)
    {
        IRuntimeObject result = RuntimeConstants.Null;
        foreach (var statement in program.Statements)
        {
            result = Evaluate(statement);

            if (result is ReturnValueObject retObj)
            {
                return retObj.Value;
            }
        }
        return result;
    }
}