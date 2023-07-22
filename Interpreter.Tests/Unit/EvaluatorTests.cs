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
    [InlineData("5 + true;", $"type mismatch: Integer + Boolean")]
    [InlineData("5 + true; 5;", "type mismatch: Integer + Boolean")]
    [InlineData("-true", "unknown operator: -Boolean")]
    [InlineData("true + false;", "unknown operator: Boolean + Boolean")]
    [InlineData("5; true + false; 5", "unknown operator: Boolean + Boolean")]
    [InlineData("if (10 > 1) { true + false; }", "unknown operator: Boolean + Boolean")]
    [InlineData("""
        if (10 > 1) {
            if (10 > 1) { return true + false; }
            return 1;
        }
        """, "unknown operator: Boolean + Boolean")]
    [InlineData("foobar", "identifier not found: foobar")]
    [InlineData(@"{""name"": ""Monkey""}[fn(x){x}];", "unusable as hash key: Function")]
    public void Evaluate_ErrorHandling_CreatesExpectedErrorMessages(string input, string expected)
    {
        var actual = TestEval(input, errorCheck: false);
        Assert.NotNull(actual);
        AssertCheckErrorMessage(actual, expected);
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

    [Fact]
    public void Evaluate_FunctionLiteral_CreatesFunctionObject()
    {
        const string input = "fn(x) { x + 2; }";

        var evaluated = TestEval(input);
        var functionObject = Assert.IsType<FunctionObject>(evaluated);
        var parameter = Assert.Single(functionObject.Parameters);
        Assert.Equal("x", parameter.ToString());
        Assert.Equal("(x + 2)", functionObject.Body.ToString());
    }

    [Theory]
    [InlineData("let identity = fn(x) { x; }; identity(5);", 5L)]
    [InlineData("let identity = fn(x) { return x; }; identity(5);", 5L)]
    [InlineData("let double = fn(x) { x * 2; }; double(5)", 10L)]
    [InlineData("let add = fn(x, y) { x + y; }; add(5, 5);", 10L)]
    [InlineData("let add = fn(x, y) { x + y; }; add(5 + 5, add(5, 5));", 20L)]
    [InlineData("fn(x) { x; }(5)", 5L)]
    public void Evaluate_FunctionLiterals_CreatesFunctionObjectsWithIntegerLiteralValues(string input, long expected)
    {
        
        var actual = TestEval(input);
        Assert.NotNull(actual);
        AssertCheckIntegerObject(actual, expected);
    }

    [Fact]
    public void Evaluate_FunctionLiterals_CreatesProperClosures()
    {
        const string input = """
            let newAdder = fn(x) {
                fn(y) { x + y };
            };
            let addTwo = newAdder(2);
            addTwo(2);
            """;
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        AssertCheckIntegerObject(evaluated, 4L);
    }

    [Theory]
    [InlineData(@"""Hello World!""", "Hello World!")]
    [InlineData(@"let a = ""This is a test!"";", "This is a test!")]
    public void Evaluate_StringLiteral_CreatesStringRuntimeObjects(string input, string expected)
    {
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        AssertCheckStringObject(evaluated, expected);
    }

    [Theory]
    [InlineData(@"""Hello, "" + ""world!""", "Hello, world!")]
    [InlineData(@"""this"" + "" and "" + ""that""", "this and that")]
    [InlineData(@"""one"" + "", two"" + "", three""", "one, two, three")]
    public void Evaluate_StringLiteralConcatenation_ConcatenatesStrings(string input, string expected)
    {
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        AssertCheckStringObject(evaluated, expected);
    }

    [Theory]
    [InlineData(@"""Test"" + 123", "Test123")]
    [InlineData(@"""1"" + 2 + ""3""", "123")]
    [InlineData(@"""1"" + 2 + 3", "123")]
    public void Evaluate_StringLiteralConcatenationWithIntegers_ConcatenatesStringsAndIntegers(string input, string expected)
    {
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        AssertCheckStringObject(evaluated, expected);
    }

    [Theory]
    [InlineData(@"len("""")", 0L)]
    [InlineData(@"len(""four"")", 4L)]
    [InlineData(@"len(""hello world"")", 11L)]
    public void Evaluate_BuiltinFunctionLenStrings_StringLengths(string input, long expectedLength)
    {
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        AssertCheckIntegerObject(evaluated, expectedLength);
    }

    [Theory]
    [InlineData(@"len(1)", "argument to `len` not supported, got Integer")]
    [InlineData(@"len(""one"", ""two"")", "wrong number of arguments. got=2, wanted=1")]
    public void Evaluate_BuiltinFunctionLenErrors_Errors(string input, string expectedError)
    {
        var evaluated = TestEval(input, errorCheck: false);
        Assert.NotNull(evaluated);
        AssertCheckErrorMessage(evaluated, expectedError);
    }
    
    [Theory]
    [InlineData(@"len([])", 0L)]
    [InlineData(@"len([1, 2, 3, 4])", 4L)]
    [InlineData(@"len([""Hello,"", "" world!""])", 2L)]
    public void Evaluate_BuiltinFunctionLenArrays_ArrayLengths(string input, long expectedLength)
    {
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        AssertCheckIntegerObject(evaluated, expectedLength);
    }

    [Fact]
    public void Evaluate_ArrayLiterals_EvaluatesArrayLiteral()
    {
        const string input = "[1, 2 * 2, 3 + 3]";
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        var array = Assert.IsType<ArrayObject>(evaluated);
        Assert.Equal(3, array.Elements.Count);
        AssertCheckIntegerObject(array.Elements[0], 1);
        AssertCheckIntegerObject(array.Elements[1], 4);
        AssertCheckIntegerObject(array.Elements[2], 6);
    }

    [Theory]
    [InlineData("[1, 2, 3][0]", 1L)]
    [InlineData("[1, 2, 3][1]", 2L)]
    [InlineData("[1, 2, 3][2]", 3L)]
    [InlineData("let i = 0; [1][i];", 1L)]
    [InlineData("[1, 2, 3][1 + 1];", 3L)]
    [InlineData("let myArray = [1, 2, 3]; myArray[2];", 3L)]
    [InlineData("let myArray = [1, 2, 3]; myArray[0] + myArray[1] + myArray[2];", 6L)]
    [InlineData("let myArray = [1, 2, 3]; let i = myArray[0]; myArray[i];", 2L)]
    [InlineData("[1, 2, 3][3]", null)]
    [InlineData("[1, 2, 3][-1]", null)]
    public void Evaluate_ArrayIndexExpressions_EvaluatesCorrectIndex<T>(string input, T expected)
    {
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        AssertCheckObject(evaluated, expected);
    }

    [Theory]
    [InlineData("first([1, 2, 3])", 1L)]
    [InlineData("first([2, 3, 1])", 2L)]
    [InlineData(@"first([""test"", 3, 1])", "test")]
    [InlineData("first([true, 3, 1])", true)]
    [InlineData("first([false, true, false])", false)]
    [InlineData("first([])", null)]
    public void Evaluate_BuiltinFunctionFirst_GetsFirstElement<T>(string input, T expected)
    {
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        AssertCheckObject(evaluated, expected);
    }

    [Theory]
    [InlineData("last([1, 2, 3])", 3L)]
    [InlineData("last([2, 3, 1])", 1L)]
    [InlineData(@"last([""test"", 3, ""hello, world""])", "hello, world")]
    [InlineData("last([true, 12931, true])", true)]
    [InlineData("last([false, true, false])", false)]
    [InlineData("last([])", null)]
    public void Evaluate_BuiltinFunctionLast_GetsLastElement<T>(string input, T expected)
    {
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        AssertCheckObject(evaluated, expected);
    }
    
    [Theory]
    [InlineData("rest([1, 2, 3])", new object[] { 2L, 3L })]
    [InlineData("rest([2, 3, 1])", new object[] { 3L, 1L })]
    [InlineData(@"rest([""test"", 3, ""hello, world""])", new object[] { 3L, "hello, world" })]
    [InlineData("rest([true, 12931, true])", new object[] { 12931L, true })]
    [InlineData("rest([false, true, false])", new object[] { true, false })]
    [InlineData("rest([])", null)]
    public void Evaluate_BuiltinFunctionRest_GetsTheRest(string input, object[]? expectedCollection)
    {
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        if (expectedCollection is not null) { AssertCheckObjectCollection(evaluated, expectedCollection); }
        else { AssertCheckNullObject(evaluated); }
    }
    
    [Theory]
    [InlineData("push([1, 2], 3)", new object[] { 1L, 2L, 3L })]
    [InlineData(@"push([1, 2], ""3"")", new object[] { 1L, 2L, "3" })]
    public void Evaluate_BuiltinFunctionPush_MakesNewArrayWithValue(string input, object[]? expectedCollection)
    {
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        if (expectedCollection is not null) { AssertCheckObjectCollection(evaluated, expectedCollection); }
        else { AssertCheckNullObject(evaluated); }
    }

    [Fact]
    public void Evaluate_BuiltinFunctionPuts_ReturnsRuntimeNull()
    {
        using Stream stream = new MemoryStream();
        using StreamWriter writer = new(stream);
        Evaluator.StandardOut.AttachToStream(writer);

        const string input = @"puts(""hello, world!"")";
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        AssertCheckNullObject(evaluated);
    }
    
    [Theory]
    [InlineData(@"puts(""hello, world!"");", "hello, world!")]
    [InlineData(@"let foo = ""output!""; puts(foo);", "output!")]
    [InlineData(@"puts(""hello"", "" "", ""world!"")", "hello", " ", "world!")]
    [InlineData(@"puts(1337)", "1337")]
    [InlineData(@"puts(1337, ""1338"")", "1337", "1338")]
    [InlineData(@"puts("""")", "")]
    public void Evaluate_BuiltinFunctionPuts_PutsExpectedToStream(string input, params string[] expectedLines)
    {
        using Stream outStream = new MemoryStream();
        using TextReader reader = new StreamReader(outStream);
        using StreamWriter writer = new(outStream);
        Evaluator.StandardOut.AttachToStream(writer);
        
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        outStream.Position = 0L;
        string? line = reader.ReadLine();
        Assert.NotNull(line);
        for (int i = 0; line is not null; i++)
        {
            Assert.Equal(expectedLines[i], line);
            line = reader.ReadLine();
        }
    }
    
    [Fact]
    public void Evaluate_HashLiterals_CreatesExpectedHashMap()
    {
        const string input = """
            let two = "two";
            {
                "one": 10 - 9,
                two: 1 + 1,
                "thr" + "ee": 6 / 2,
                4: 4,
                true: 5,
                false: 6
            }
            """;

        var evaluated = TestEval(input);
        var hash = Assert.IsType<HashObject>(evaluated);

        Dictionary<HashKey, long> expected = new()
        {
            [HashKeyFromString("one")] = 1L,
            [HashKeyFromString("two")] = 2L,
            [HashKeyFromString("three")] = 3L,
            [HashKeyFromLong(4L)] = 4L,
            [HashKeyFromBool(true)] = 5L,
            [HashKeyFromBool(false)] = 6L,
        };
        
        Assert.Equal(expected.Count, hash.Pairs.Count);
        foreach (KeyValuePair<HashKey, long> kvp in expected)
        {
            var actual = Assert.Contains(kvp.Key, hash.Pairs);
            AssertCheckIntegerObject(actual.Value, kvp.Value);
        }
        
        static HashKey HashKeyFromString(string str) => new StringObject(str).GetHashKey();
        static HashKey HashKeyFromLong(long l) => new IntegerObject(l).GetHashKey();
        static HashKey HashKeyFromBool(bool b) => b ? Evaluator.RuntimeConstants.True.GetHashKey() : Evaluator.RuntimeConstants.False.GetHashKey();
    }

    [Theory]
    [InlineData(@"{""foo"": 5}[""foo""]", 5L)]
    [InlineData(@"{""foo"": 5}[""bar""]", null)]
    [InlineData(@"let key = ""foo""; {""foo"": 5}[key]", 5L)]
    [InlineData(@"{}[""foo""]", null)]
    [InlineData(@"{5: 5}[5]", 5L)]
    [InlineData(@"{true: 5}[true]", 5L)]
    [InlineData(@"{false: 5}[false]", 5L)]
    public void Evaluate_HashLiteralIndexOperator_GetsExpectedValueFromHash<T>(string input, T? expectedValue)
    {
        var evaluated = TestEval(input);
        Assert.NotNull(evaluated);
        AssertCheckObject(evaluated, expectedValue);
    }

    private static IRuntimeObject? TestEval(string input, bool errorCheck = true)
    {
        Lexer lexer = new(input);
        Parser parser = new(lexer); 
        Environment env = new();
        var program = parser.ParseProgram();
        var evaluated = Evaluator.Evaluate(program, env);
        if (errorCheck && evaluated is RuntimeErrorObject error)
        {
            Assert.Fail($"!!EVALUATOR ERROR!!\nMessage: {error.Message ?? "--empty error message--"}");
        }
        return evaluated;
    }

    private static void AssertCheckObjectCollection(IRuntimeObject obj, object[] expected)
    {
        var array = Assert.IsType<ArrayObject>(obj);
        Assert.Equal(expected.Length, array.Elements.Count);
        for (int i = 0; i < expected.Length; i++)
        {
            var expectedElement = expected[i];
            var actual = array.Elements[i];
            AssertCheckObject(actual, expectedElement);
        }
    }

    private static void AssertCheckObject<T>(IRuntimeObject obj, T expected)
    {
        switch (expected)
        {
            case long l:
                AssertCheckIntegerObject(obj, l);
                break;
            case string str:
                AssertCheckStringObject(obj, str);
                break;
            case bool b:
                AssertCheckBooleanObject(obj, b);
                break;
            case null:
                AssertCheckNullObject(obj);
                break;
            default:
                throw new NotImplementedException($"Not implemented: {typeof(T)}");
        }
    }

    private static StringObject AssertCheckStringObject(IRuntimeObject obj, string expectedValue)
    {
        var strObj = Assert.IsType<StringObject>(obj);
        Assert.Equal(expectedValue, strObj.Value);
        return strObj;
    }
    
    private static void AssertCheckError(IRuntimeObject obj)
    {
        if (obj is RuntimeErrorObject error)
        {
            Assert.Fail(error.Message ?? "No error message.");
        }
    }
    
    private void AssertCheckErrorMessage(IRuntimeObject actual, string expected)
    {
        var message = Assert.IsType<RuntimeErrorObject>(actual).Message;
        Assert.Equal(expected, message);
    }

    private static NullObject AssertCheckNullObject(IRuntimeObject evaluated)
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