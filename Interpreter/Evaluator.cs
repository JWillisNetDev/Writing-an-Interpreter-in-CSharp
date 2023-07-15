using System.Linq.Expressions;
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
        IRuntimeObject value, right, left;
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
                value = Evaluate(statement.ReturnValue);
                return IsError(value) ? value : new ReturnValueObject(value);
            
            // Expressions
            case IntegerLiteral integer:
                return new IntegerObject(integer.Value);
            
            case BooleanLiteral boolean:
                return BooleanNativeAsObject(boolean.Value);
            
            case PrefixExpression prefix:
                right = Evaluate(prefix.Right);
                return IsError(right) ? right : EvaluatePrefixExpression(prefix.Operator, right);
            
            case InfixExpression infix:
                left = Evaluate(infix.Left);
                if (IsError(left)) { return left; }

                right = Evaluate(infix.Right);
                return IsError(right) ? right : EvaluateInfixExpression(infix.Operator, left, right);
            
            case IfExpression ifExpr:
                return EvaluateIfExpression(ifExpr);
        }

        return RuntimeConstants.Null;
    }

    private static IRuntimeObject EvaluateIfExpression(IfExpression ifExpr)
    {
        var condition = Evaluate(ifExpr.Condition);

        if (IsError(condition))
        {
            return condition;
        }
        else if (IsTruthy(condition))
        {
            return Evaluate(ifExpr.Consequence);
        }
        else if (ifExpr.Alternative is not null)
        {
            return Evaluate(ifExpr.Alternative);
        }
        return RuntimeConstants.Null;
    }

    private static RuntimeErrorObject Error(string message) => new(message);

    private static bool IsError(IRuntimeObject? obj) => obj is { Type: RuntimeObjectType.ErrorObject };
    
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
            if (result.Type is RuntimeObjectType.ReturnObject or RuntimeObjectType.ErrorObject) { return result; }
        }
        return result;
    }

    private static IRuntimeObject EvaluateInfixExpression(string infixOperator, IRuntimeObject? left, IRuntimeObject? right)
    {
        if (left?.Type != right?.Type)
        {
            return Error($"type mismatch: {left.Type} {infixOperator} {right.Type}");
        }
        
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

        return Error($"unknown operator: {left?.Type} {infixOperator} {right?.Type}");

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
            _ => Error($"unknown operator: {left.Type} {infixOperator} {right.Type}"),
        };
    }

    private static IRuntimeObject EvaluatePrefixExpression(string prefixOperator, IRuntimeObject? right)
    {
        return prefixOperator switch
        {
            "!" => EvaluateBangOperatorExpression(right),
            "-" => EvaluateMinusOperatorExpression(right),
            _ => Error($"unknown operator: {prefixOperator} {right?.Type}"),
        };
        
        // !<expression>
        IRuntimeObject EvaluateBangOperatorExpression(IRuntimeObject? r) => BooleanNativeAsObject(!IsTruthy(r));
    
        // -<integer expression>
        IRuntimeObject EvaluateMinusOperatorExpression(IRuntimeObject? r) => r switch
        {
            IntegerObject i => new IntegerObject(-i.Value),
            _ => Error($"unknown operator: -{r?.Type}"),
        };
    }

    private static IRuntimeObject BooleanNativeAsObject(bool booleanValue) => booleanValue ? RuntimeConstants.True : RuntimeConstants.False;

    private static IRuntimeObject EvaluateProgram(Program program)
    {
        IRuntimeObject result = RuntimeConstants.Null;
        foreach (var statement in program.Statements)
        {
            result = Evaluate(statement);

            if (result is ReturnValueObject ret) { return ret.Value; }
            if (result is RuntimeErrorObject err) { return err; }
        }
        return result;
    }
}