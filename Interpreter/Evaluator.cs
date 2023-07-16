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

    public static IRuntimeObject Evaluate(INode node, Environment env)
    {
        IRuntimeObject value, right, left;
        switch (node)
        {
            // Statements
            case Program program:
                return EvaluateProgram(program, env);
            
            case ExpressionStatement statement:
                return Evaluate(statement.Expression, env);
            
            case BlockStatement statement:
                return EvaluateBlockStatement(statement, env);
            
            case ReturnStatement statement:
                value = Evaluate(statement.ReturnValue, env);
                return IsError(value) ? value : new ReturnValueObject(value);
            
            case LetStatement statement:
                value = Evaluate(statement.Value!, env);
                if (IsError(value)) { return value; }
                env.Set(statement.Name.Value, value);
                return value;

            // Expressions
            case IntegerLiteral integer:
                return new IntegerObject(integer.Value);
            
            case BooleanLiteral boolean:
                return BooleanNativeAsObject(boolean.Value);
            
            case FunctionLiteral func:
                return new FunctionObject(func.Parameters, func.Body, env);
                
            case PrefixExpression prefix:
                right = Evaluate(prefix.Right, env);
                return IsError(right) ? right : EvaluatePrefixExpression(prefix.Operator, right);
            
            case InfixExpression infix:
                left = Evaluate(infix.Left, env);
                if (IsError(left)) { return left; }

                right = Evaluate(infix.Right, env);
                return IsError(right) ? right : EvaluateInfixExpression(infix.Operator, left, right);
            
            case IfExpression ifExpr:
                return EvaluateIfExpression(ifExpr, env);
            
            case CallExpression call:
                var function = Evaluate(call.Function, env);
                if (IsError(function)) { return function; }
                
                var args = EvaluateExpressions(call.Arguments, env).ToList();
                if (IsError(args.LastOrDefault())) { return args.Last(); }

                return BindFunction(function, args);

            // Identifiers
            case Identifier identifier:
                return EvaluateIdentifier(identifier, env);
        }

        return RuntimeConstants.Null;
    }
    private static IRuntimeObject BindFunction(IRuntimeObject function, IReadOnlyList<IRuntimeObject> arguments)
    {
        if (function is FunctionObject functionObject)
        {
            Environment extended = ExtendFunctionEnvironment(functionObject, arguments);
            var evaluated = Evaluate(functionObject.Body, extended);
            return UnwrapReturnValue(evaluated);
        }
        return Error($"not a function: {function.Type}");
        
        Environment ExtendFunctionEnvironment(FunctionObject fn, IReadOnlyList<IRuntimeObject> args)
        {
            Environment env = new(fn.Environment);
            for (int i = 0; i < fn.Parameters.Count; i++)
            {
                env.Set(fn.Parameters[i].Value, args[i]);
            }
            
            return env;
        }
        
        IRuntimeObject UnwrapReturnValue(IRuntimeObject obj)
        {
            if (obj is ReturnValueObject retObj) { return retObj.Value; }
            return obj;
        }
    }
    private static IEnumerable<IRuntimeObject> EvaluateExpressions(ImmutableArray<IExpression> expressions, Environment env)
    {
        foreach (var expression in expressions)
        {
            IRuntimeObject evaluated = Evaluate(expression, env);
            yield return evaluated;
            if (IsError(evaluated)) { break; }
        }
    }
    
    private static IRuntimeObject EvaluateIdentifier(Identifier identifier, Environment env)
        => env.TryGet(identifier.Value, out var obj) ? obj : Error($"identifier not found: {identifier.Value}");

    private static IRuntimeObject EvaluateIfExpression(IfExpression ifExpr, Environment env)
    {
        var condition = Evaluate(ifExpr.Condition, env);

        if (IsError(condition))
        {
            return condition;
        }
        else if (IsTruthy(condition))
        {
            return Evaluate(ifExpr.Consequence, env);
        }
        else if (ifExpr.Alternative is not null)
        {
            return Evaluate(ifExpr.Alternative, env);
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
    
    private static IRuntimeObject EvaluateBlockStatement(BlockStatement block, Environment env)
    {
        IRuntimeObject result = RuntimeConstants.Null;
        foreach (var statement in block.Statements)
        {
            result = Evaluate(statement, env);
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

    private static IRuntimeObject EvaluateProgram(Program program, Environment env)
    {
        IRuntimeObject result = RuntimeConstants.Null;
        foreach (var statement in program.Statements)
        {
            result = Evaluate(statement, env);

            if (result is ReturnValueObject ret) { return ret.Value; }
            if (result is RuntimeErrorObject err) { return err; }
        }
        return result;
    }
}