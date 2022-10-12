using Flandre.Core.Utils;

namespace Flandre.Core.Tests.UtilsTests;

public class StringParserTests
{
    [Fact]
    public void TestStringParser()
    {
        const string str = "Str   ing Parser_[Tests]";
        var parser = new StringParser(str);
        
        Assert.Equal('S', parser.Current);

        parser.Skip(2);
        Assert.Equal('r', parser.Current);

        parser.Skip(1).SkipSpaces(); // skip 'r' and skip spaces
        Assert.Equal('i', parser.Current);
        
        Assert.Equal("ing", parser.Peek(3));
        
        Assert.Equal("ing P", parser.Peek('a'));
        
        Assert.Equal("ing", parser.Read(' '));
        Assert.Equal(' ', parser.Current);
        
        Assert.Equal("Parser_[Tests]", parser.SkipSpaces().ReadToEnd());
    }

    [Fact]
    public void TestQuotes()
    {
        const string str = "\"alpha\" 'beta' gamma other";
        var parser = new StringParser(str);
        
        Assert.Equal("alpha", parser.ReadQuoted());
        parser.SkipSpaces();
        
        Assert.Equal("beta", parser.ReadQuoted());
        parser.SkipSpaces();
        
        Assert.Equal("gamma", parser.ReadQuoted());
        
        // 'x' is not in the rest of the string, so default to ReadToEnd.
        Assert.Equal(" other", parser.Read('x'));
    }
}