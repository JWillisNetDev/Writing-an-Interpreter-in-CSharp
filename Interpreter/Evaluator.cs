using System.Diagnostics.CodeAnalysis;
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

    public static class StandardOut
    {
        private static TextWriter? _writer;

        [MemberNotNull(nameof(_writer))]
        private static void ThrowIfNotAttached()
        {
            if (!IsAttached)
            {
                throw new InvalidOperationException("Attempted to write to standard output when no stream was attached");
            }
        }
        
        internal static void Write(string str)
        {
            ThrowIfNotAttached();
            _writer.Write(str);
            _writer.Flush();
        }

        internal static async Task WriteAsync(string str)
        {
            ThrowIfNotAttached();
            await _writer.WriteAsync(str);
            await _writer.FlushAsync();
        }

        internal static void WriteLine(string str)
        {
            ThrowIfNotAttached();
            _writer.WriteLine(str);
            _writer.Flush();
        }
        
        internal static async Task WriteLineAsync(string str)
        {
            ThrowIfNotAttached();
            await _writer.WriteLineAsync(str);
            await _writer.FlushAsync();
        }
        
        public static void AttachToStream(StreamWriter stream) => _writer = stream;
        
        public static void DetachFromStream() => _writer = null;
        
        [MemberNotNullWhen(true, nameof(_writer))]
        public static bool IsAttached => _writer is not null;
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
            case ArrayLiteral array:
                var elements = EvaluateExpressions(array.Elements, env);
                if (elements.LastOrDefault() is { } last 
                    && IsError(last)) { return last; }
                return new ArrayObject(elements);
            
            case BooleanLiteral boolean:
                return BooleanNativeAsObject(boolean.Value);
            
            case FunctionLiteral func:
                return new FunctionObject(func.Parameters, func.Body, env);
            
            case HashLiteral hash:
                return EvaluateHashLiteral(hash, env);
            
            case IntegerLiteral integer:
                return new IntegerObject(integer.Value);
            
            case StringLiteral str:
                return new StringObject(str.Value);
                
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
            
            case IndexExpression indexExpr:
                left = Evaluate(indexExpr.Left, env);
                if (IsError(left)) { return left; }
                var index = Evaluate(indexExpr.Index, env);
                return IsError(index) ? index : EvaluateIndexExpression(left, index);

            case CallExpression call:
                var function = Evaluate(call.Function, env);
                if (IsError(function)) { return function; }
                
                var args = EvaluateExpressions(call.Arguments, env).ToArray();
                if (IsError(args.LastOrDefault())) { return args.Last(); }

                return BindFunction(function, args);

            // Identifiers
            case Identifier identifier:
                return EvaluateIdentifier(identifier, env);
        }

        return RuntimeConstants.Null;
    }
    
    private static IRuntimeObject EvaluateHashLiteral(HashLiteral hash, Environment env)
    {
        Dictionary<HashKey, HashObject.HashPair> pairs = new();
        foreach (var (keyNode, valueNode) in hash.Pairs)
        {
            var key = Evaluate(keyNode, env);
            if (IsError(key)) { return key; }
            if (key is IHashable hashObj)
            {
                var value = Evaluate(valueNode, env);
                if (IsError(value)) { return value; }
                
                pairs[hashObj.GetHashKey()] = new HashObject.HashPair(key, value);
            }
            else return Error($"unusable as hash key: {key.Type}");
        }
        return new HashObject(pairs);
    }

    private static IRuntimeObject EvaluateIndexExpression(IRuntimeObject left, IRuntimeObject index)
    {
        return (left, index) switch
        {
            (ArrayObject arr, IntegerObject intIndex) => EvaluateArrayIndexExpression(arr, intIndex),
            (HashObject hash, _) => EvaluateHashIndexExpression(hash, index),
            _ => Error($"index operator not supported: {left.Type}"),
        };
    }
    
    private static IRuntimeObject EvaluateHashIndexExpression(HashObject hash, IRuntimeObject index)
    {
        if (index is IHashable hashable)
        {
            return hash.Pairs.TryGetValue(hashable.GetHashKey(), out var pair) ?
                pair.Value :
                RuntimeConstants.Null;
        }
        return Error($"unusable as hash key: {index.Type}");
    }

    private static IRuntimeObject EvaluateArrayIndexExpression(ArrayObject left, IntegerObject index)
    {
        if (index.Value >= 0 && index.Value < left.Elements.Count) { return left.Elements[(int)index.Value]; } // TODO This cast is evil.
        return RuntimeConstants.Null; 
    }

    private static IRuntimeObject BindFunction(IRuntimeObject function, IRuntimeObject[] arguments)
    {
        if (function is FunctionObject functionObject)
        {
            Environment extended = ExtendFunctionEnvironment(functionObject, arguments);
            var evaluated = Evaluate(functionObject.Body, extended);
            return UnwrapReturnValue(evaluated);
        }
        if (function is BuiltinObject builtin) { return builtin.Function(arguments); }
        return Error($"not a function: {function.Type}");
        
        static Environment ExtendFunctionEnvironment(FunctionObject fn, IReadOnlyList<IRuntimeObject> args)
        {
            Environment env = new(fn.Environment);
            for (int i = 0; i < fn.Parameters.Count; i++)
            {
                env.Set(fn.Parameters[i].Value, args[i]);
            }
            
            return env;
        }
        
        static IRuntimeObject UnwrapReturnValue(IRuntimeObject obj)
        {
            if (obj is ReturnValueObject retObj) { return retObj.Value; }
            return obj;
        }
    }
    
    private static List<IRuntimeObject> EvaluateExpressions(IReadOnlyList<IExpression> expressions, Environment env)
    {
        List<IRuntimeObject> evaluated = new();
        foreach (var expression in expressions)
        {
            var eval = Evaluate(expression, env);
            evaluated.Add(eval);
            if (IsError(eval)) { return evaluated; }
        }
        return evaluated;
    }

    private static IRuntimeObject EvaluateIdentifier(Identifier identifier, Environment env)
    {
        if (env.TryGet(identifier.Value, out var runtimeVar))
        {
            return runtimeVar;
        }
        if (Builtins.TryGet(identifier.Value, out var builtin))
        {
            return builtin;
        }
        return Error($"identifier not found: {identifier.Value}");
    }
    
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

    private static RuntimeErrorObject UnknownInfixOperatorError(string infixOperator, IRuntimeObject left, IRuntimeObject right)
        => Error($"unknown operator: {left.Type} {infixOperator} {right.Type}");

    private static bool IsError(IRuntimeObject? obj) => obj is { Type: RuntimeObjectType.Error };
    
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
            if (result.Type is RuntimeObjectType.Return or RuntimeObjectType.Error) { return result; }
        }
        return result;
    }

    private static IRuntimeObject EvaluateInfixExpression(string infixOperator, IRuntimeObject? left, IRuntimeObject? right)
    {
        if (infixOperator == "+" && left is StringObject l) // Departure from Monkey, strings only can concat on strings in typical Monkey
        {
            return EvaluateStringConcat(l, right);
        }
        
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

        IRuntimeObject EvaluateStringConcat(StringObject str, IRuntimeObject r) => r switch
        {
            IntegerObject i => new StringObject(str.Value + i.Value),
            BooleanObject b => new StringObject(str.Value + (b.Value ? "1" : "0")),
            StringObject s => new StringObject(str.Value + s.Value),
            _ => UnknownInfixOperatorError(infixOperator, str, r),
        };
        
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
            _ => UnknownInfixOperatorError(infixOperator, left, right),
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