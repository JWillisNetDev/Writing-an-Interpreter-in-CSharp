namespace Interpreter.Tests.Unit;

public class ObjectTests
{
    [Fact]
    public void Object_TestStringHashKeys_StringsWithSameContentsMakeSameHashKeys()
    {
        var hello1 = new StringObject("Hello, world!");
        var hello2 = new StringObject("Hello, world!");
        Assert.Equal(hello1.GetHash(), hello2.GetHash());

        var diff1 = new StringObject("My name is johnny");
        var diff2 = new StringObject("My name is johnny");
        Assert.Equal(diff1.GetHash(), diff2.GetHash());

        Assert.NotEqual(hello1.GetHash(), diff1.GetHash());
    }
}