namespace Interpreter.Tests.Unit;

public class EvaluatorTests
{
    [Theory]
    [InlineData("5", 5L)]
    [InlineData("10", 10L)]
    public void Evaluate_IntegerLiterals_RuntimeIntegerObjects(string input, long expected)
    {
        var actual = TestEval(input);
        Assert.NotNull(actual);
        Assert.NotSame(actual, Evaluator.RuntimeConstants.Null);
        AssertCheckError(actual);
        AssertCheckIntegerObject(actual, expected);
    }

    [Theory]
    [InlineData("-5", -5L)]
    [InlineData("-10", -10L)]
    public void Evaluate_MinusPrefixOperatorWithIntegerLiteral_NegatesIntegers(string input, long expected)
    {
        var actual = TestEval(input);
        Assert.NotNull(actual);
        Assert.NotSame(actual, Evaluator.RuntimeConstants.Null);
        AssertCheckError(actual);
        AssertCheckIntegerObject(actual, expected);
    }

    [Theory]
    [InlineData("5 + 5 + 5 + 5 - 10", 10L)]
    [InlineData("2 * 2 * 2 * 2 * 2", 32L)]
    [InlineData("-50 + 100 + -50", 0L)]
    [InlineData("5 * 2 + 10", 20L)]
    [InlineData("5 + 2 * 10", 25L)]
    [InlineData("20 + 2 * -10", 0L)]
    [InlineData("50 / 2 * 2 + 10", 60L)]
    [InlineData("2 * (5 + 10)", 30L)]
    [InlineData("3 * 3 * 3 + 10", 37L)]
    [InlineData("3 * (3 * 3) + 10", 37L)]
    [InlineData("(5 + 10 * 2 + 15 / 3) * 2 + -10", 50L)]
    public void Evaluate_InfixOperatorsWithIntegerExpressions_EvaluatesExpressions(string input, long expected)
    {
        var actual = TestEval(input);
        Assert.NotNull(actual);
        Assert.NotSame(actual, Evaluator.RuntimeConstants.Null);
        AssertCheckError(actual);
        AssertCheckIntegerObject(actual, expected);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void Evaluate_BooleanExpressions_RuntimeBooleanObjects(string input, bool expected)
    {
        var actual = TestEval(input);
        Assert.NotNull(actual);
        Assert.NotSame(actual, Evaluator.RuntimeConstants.Null);
        AssertCheckError(actual);
        AssertCheckBooleanObject(actual, expected);
    }

    [Theory]
    [InlineData("!true", false)]
    [InlineData("!false", true)]
    [InlineData("!5", false)]
    [InlineData("!!true", true)]
    [InlineData("!!false", false)]
    [InlineData("!!5", true)]
    public void Evaluate_NotOperator_LogicallyNegatesIntsAndBools(string input, bool expected)
    {
        var actual = TestEval(input);
        Assert.NotNull(actual);
        Assert.NotSame(actual, Evaluator.RuntimeConstants.Null);
        AssertCheckError(actual);
        AssertCheckBooleanObject(actual, expected);
    }

    [Theory]
    [InlineData("1 < 2", true)]
    [InlineData("1 > 2", false)]
    [InlineData("1 < 1", false)]
    [InlineData("1 > 1", false)]
    [InlineData("1 == 1", true)]
    [InlineData("1 != 1", false)]
    [InlineData("1 == 2", false)]
    [InlineData("1 != 2", true)]

    public void Evaluate_IntegerExpressionsWithInfix_LogicallyEvaluatesExpressionsToBooleans(string input, bool expected)
    {
        var actual = TestEval(input);
        Assert.NotNull(actual);
        Assert.NotSame(actual, Evaluator.RuntimeConstants.Null);
        AssertCheckError(actual);
        AssertCheckBooleanObject(actual, expected);
    }
    
    [Theory]
    [InlineData("true == true", true)]
    [InlineData("false == true", false)]
    [InlineData("false == false", true)]
    [InlineData("false != true", true)]
    [InlineData("(1 < 2) == true", true)]
    [InlineData("(1 < 2) == false", false)]
    [InlineData("(1 > 2) == false", true)]
    [InlineData("(1 > 2) == true", false)]
    public void Evaluate_BooleanExpressionsWithInfix_LogicallyEvaluatesExpressionsToBooleans(string input, bool expected)
    {
        var actual = TestEval(input);
        Assert.NotNull(actual);
        Assert.NotSame(actual, Evaluator.RuntimeConstants.Null);
        AssertCheckError(actual);
        AssertCheckBooleanObject(actual, expected);
    }

    [Theory]
    [InlineData("if (true) { 10 }", 10L)]
    [InlineData("if (false) { 10 }", null)]
    [InlineData("if (1) { 10 }", 10L)]
    [InlineData("if (1 < 2) { 10 }", 10L)]
    [InlineData("if (1 > 2) { 10 }", null)]
    public void Evaluate_IfExpressionsWithoutElse_ReturnsCorrectConsequences(string input, long? expected)
    {
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        AssertCheckError(evaluated);
        if (expected.HasValue)
        {
            Assert.NotSame(Evaluator.RuntimeConstants.Null, evaluated);
            AssertCheckIntegerObject(evaluated, expected.Value);
        }
        else
        {
            AssertCheckNullObject(evaluated);
        }
    }

    [Theory]
    [InlineData("if (1 > 2) { 10 } else { 20 }", 20L)]
    [InlineData("if (1 < 2) { 10 } else { 20 }", 10L)]
    [InlineData("if (1 > 2) { 10 } else { ; }", null)]
    public void Evaluate_IfExpressionsWithElse_ReturnsCorrectConsequences(string input, long? expected)
    {
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        AssertCheckError(evaluated);
        if (expected.HasValue)
        {
            Assert.NotSame(Evaluator.RuntimeConstants.Null, evaluated);
            AssertCheckIntegerObject(evaluated, expected.Value);
        }
        else
        {
            AssertCheckNullObject(evaluated);
        }
    }

    [Theory]
    [InlineData("return 10;", 10L)]
    [InlineData("return 10; 9;", 10L)]
    [InlineData("return 2 * 5; 9;", 10L)]
    [InlineData("9; return 2 * 5; 9;", 10L)]
    public void Evaluate_ReturnStatements_ReturnsEvaluatedInteger(string input, long expected)
    {
        var actual = TestEval(input);
        Assert.NotNull(actual);
        Assert.NotSame(Evaluator.RuntimeConstants.Null, actual);
        AssertCheckError(actual);
        AssertCheckIntegerObject(actual, expected);
    }

    [Fact]
    public void Evaluate_ReturnStatementsNestedSequence_ReturnsCorrectReturnValueInSequence()
    {
        const long expected = 10L;
        string input = $$"""
            if (10 > 1) {
                if (10 > 1) {
                    return {{expected}};
                }
                return 1;
            }
            """;
        var actual = TestEval(input);
        Assert.NotNull(actual);
        Assert.NotSame(Evaluator.RuntimeConstants.Null, actual);
        AssertCheckError(actual);
        AssertCheckIntegerObject(actual, expected);
    }

    [Theory]
    [InlineData("5 + true;", $"type mismatch: IntegerObject + BooleanObject")]
    [InlineData("5 + true; 5;", "type mismatch: IntegerObject + BooleanObject")]
    [InlineData("-true", "unknown operator: -BooleanObject")]
    [InlineData("true + false;", "unknown operator: BooleanObject + BooleanObject")]
    [InlineData("5; true + false; 5", "unknown operator: BooleanObject + BooleanObject")]
    [InlineData("if (10 > 1) { true + false; }", "unknown operator: BooleanObject + BooleanObject")]
    [InlineData("""
        if (10 > 1) {
            if (10 > 1) { return true + false; }
            return 1;
        }
        """, "unknown operator: BooleanObject + BooleanObject")]
    [InlineData("foobar", "identifier not found: foobar")]
    public void Evaluate_ErrorHandling_CreatesExpectedErrorMessages(string input, string expected)
    {
        var actual = TestEval(input);
        var errorObject = Assert.IsType<RuntimeErrorObject>(actual);
        Assert.NotNull(errorObject.Message);
        Assert.NotEmpty(errorObject.Message);
        Assert.Equal(expected, errorObject.Message);
    }

    [Theory]
    [InlineData("let a = 5; a;", 5L)]
    [InlineData("let a = 5 * 5; a;", 25L)]
    [InlineData("let a = 5; let b = a; b;", 5L)]
    [InlineData("let a = 5; let b = a; let c = a + b + 5 c;", 15L)]
    public void Evaluate_LetStatements_CreatesExpectedBindingsAndEvaluatesVariables(string input, long expected)
    {
        var actual = TestEval(input);
        Assert.NotNull(actual);
        AssertCheckIntegerObject(actual, expected);
    }
    
    private static IRuntimeObject? TestEval(string input)
    {
        Lexer lexer = new(input);
        Parser parser = new(lexer);
        Environment env = new();
        var program = parser.ParseProgram();
        var evaluated = Evaluator.Evaluate(program, env);
        return evaluated;
    }

    private static void AssertCheckError(IRuntimeObject obj)
    {
        if (obj is RuntimeErrorObject error)
        {
            Assert.Fail(error.Message ?? "No error message.");
        }
    }
    
    private NullObject AssertCheckNullObject(IRuntimeObject evaluated)
    {
        var nullObj = Assert.IsType<NullObject>(evaluated);
        Assert.Same(Evaluator.RuntimeConstants.Null, evaluated);
        return nullObj;
    }
    
    private static BooleanObject AssertCheckBooleanObject(IRuntimeObject actual, bool expected)
    {
        var boolObj = Assert.IsType<BooleanObject>(actual);
        Assert.Equal(expected, boolObj.Value);
        return boolObj;
    }

    private static IntegerObject AssertCheckIntegerObject(IRuntimeObject obj, long expected)
    {
        var intObj = Assert.IsType<IntegerObject>(obj);
        Assert.Equal(expected, intObj.Value);
        return intObj;
    }
}