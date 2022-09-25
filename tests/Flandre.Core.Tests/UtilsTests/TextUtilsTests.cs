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
}