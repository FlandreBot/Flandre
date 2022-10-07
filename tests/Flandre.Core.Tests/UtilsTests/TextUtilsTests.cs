using Flandre.Core.Utils;

namespace Flandre.Core.Tests.UtilsTests;

public class TextUtilsTests
{
    [Theory]
    [InlineData("   Alp ha Bet a  ", "AlphaBeta", " ")]
    [InlineData("-a --beta", "-a beta", "--")]
    public void TestRemoveString(string source, string result, string removal)
    {
        Assert.Equal(result, source.RemoveString(removal));
    }

    [Theory]
    [InlineData("string", "ring", "st")]
    [InlineData("string", "string", "")]
    public void TestTrimStart(string source, string result, string trim)
    {
        Assert.Equal(result, source.TrimStart(trim));
    }
}