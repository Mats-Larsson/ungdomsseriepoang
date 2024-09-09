using FluentAssertions;
using Results;

namespace ResultsTests;

[TestClass]
public class ClassFilterTests
{
    private readonly ClassFilter filter;

    public ClassFilterTests()
    {
        Configuration configuration = new()
        { 
            IncludeClasses = new HashSet<string>(["H11"]),
            ExcludeClasses = new HashSet<string>(["U4X"])
        };
        filter = new ClassFilter(configuration);
    }

    [TestMethod]
    public void IsIncluded()
    {
        filter.IsIncluded("H10").Should().BeTrue();
        filter.IsIncluded("D16").Should().BeTrue();
        filter.IsIncluded("U2").Should().BeTrue();
        filter.IsIncluded("U2H").Should().BeTrue();
        filter.IsIncluded("U2D").Should().BeTrue();
        filter.IsIncluded("Insk").Should().BeTrue();
        filter.IsIncluded("insk").Should().BeTrue();
        filter.IsIncluded("INSK").Should().BeTrue();
        filter.IsIncluded("H11").Should().BeTrue();
        filter.IsIncluded("U4H").Should().BeTrue();

        filter.IsIncluded("U5").Should().BeFalse();
        filter.IsIncluded("U4X").Should().BeFalse();

        filter.IsIncluded("").Should().BeFalse();
    }
}
