using A2G.ServiceStats.Discovery;

namespace A2G.ServiceStats.Tests.Discovery;

public sealed class ProcessNameMatcherTests
{
    [Fact]
    public void NormalizeInput_RemovesExeSuffix()
    {
        Assert.Equal("Orders.Api", ProcessNameMatcher.NormalizeInput("Orders.Api.exe"));
    }

    [Fact]
    public void Matches_RequiresExactMatchWithoutSubstringMatching()
    {
        Assert.True(ProcessNameMatcher.Matches("Orders.Api", "Orders.Api"));
        Assert.False(ProcessNameMatcher.Matches("Orders", "Orders.Api"));
    }
}
