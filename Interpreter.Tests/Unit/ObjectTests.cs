namespace Interpreter.Tests.Unit;

public class ObjectTests
{
    [Fact]
    public void Object_TestStringHashKeys_StringsWithSameContentsMakeSameHashKeys()
    {
        var hello1 = new StringObject("Hello, world!");
        var hello2 = new StringObject("Hello, world!");
        Assert.Equal(hello1.GetHashKey(), hello2.GetHashKey());

        var diff1 = new StringObject("My name is johnny");
        var diff2 = new StringObject("My name is johnny");
        Assert.Equal(diff1.GetHashKey(), diff2.GetHashKey());

        Assert.NotEqual(hello1.GetHashKey(), diff1.GetHashKey());
    }
}