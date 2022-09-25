using Flandre.Core.Utils;

namespace Flandre.Core.Tests.UtilsTests;

public class TextUtilsTests
{
    [Fact]
    public void TestRemoveString()
    {
        Assert.Equal("AlphaBeta", "   Alpha Bet a ".RemoveString(" "));
        Assert.Equal("-a beta", "-a --beta".RemoveString("--"));
    }
}