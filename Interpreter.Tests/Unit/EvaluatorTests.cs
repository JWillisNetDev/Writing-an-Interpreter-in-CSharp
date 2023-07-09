namespace Interpreter.Tests.Unit;

public class EvaluatorTests
{
    [Theory]
    [InlineData("5", 5L)]
    [InlineData("10", 10L)]
    public void Evaluate_IntegerExpressions_RuntimeIntegerObjects(string input, long expected)
    {
        var actual = TestEval(input);
        Assert.NotNull(actual);
        AssertCheckIntegerObject(actual, expected);
    }

    private static IRuntimeObject? TestEval(string input)
    {
        Lexer lexer = new(input);
        Parser parser = new(lexer);
        var program = parser.ParseProgram();
        return Evaluator.Evaluate(program);
    }

    private static void AssertCheckIntegerObject(IRuntimeObject obj, long expected)
    {
        IntegerObject intObj = Assert.IsType<IntegerObject>(obj);
        Assert.Equal(expected, intObj.Value);
    }
}