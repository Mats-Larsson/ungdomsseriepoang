#pragma warning disable CA1001
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Results;
using Results.Liveresultat;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ResultsTests.Liveresultat;

[TestClass]
[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
public class LiveresultatFasadeTests
{
    private readonly Mock<ILogger<LiveresultatFacade>> loggerMock = new();
    private LiveresultatFacade? facade;

    [TestInitialize]
    public void TestInitialize()
    {
        facade = new LiveresultatFacade(loggerMock.Object);
    }

    [TestCategory("Internet")]
    [TestMethod]
    public async Task GetCompetitionInfoTest()
    {
        Debug.Assert(facade != null, nameof(facade) + " != null");

        var competitionInfo = await facade.GetCompetitionInfoAsync(27215);
        if (competitionInfo == null) throw new AssertFailedException();

        competitionInfo.Id.Should().Be(27215);
    }

    [TestCategory("Internet")]
    [TestMethod]
    public async Task GetClassesTest()
    {
        Debug.Assert(facade != null, nameof(facade) + " != null");

        var classes1 = await facade.GetClassesAsync(30823);
        var classes2 = await facade.GetClassesAsync(30823);

        classes1.Should().Be(classes2);
    }

    [TestCategory("Internet")]
    [TestMethod]
    public async Task GetClassResultAsync()
    {
        var classResultList = await facade!.GetClassResultAsync(30823, "Insk").ConfigureAwait(false);

        TimeSpan? startTime = classResultList!.Results![0].StartTime;
        startTime.Should().NotBeNull();
    }

    [TestMethod]
    public void DeserializeCompetitionInfoTest()
    {
        var json = File.ReadAllText(@"Liveresultat\CompetitionInfo.json");

        var competitionInfo = facade!.DeserializeJson<CompetitionInfo>(json) ?? throw new AssertFailedException();
        competitionInfo.Should().NotBeNull();
        competitionInfo.Id.Should().Be(27215);
    }

    [TestMethod]
    public void DeserializeClassResultListTest()
    {
        var json = File.ReadAllText(@"Liveresultat\H16.json");

        var classResultList = facade!.DeserializeJson<ClassResultList>(json) ?? throw new AssertFailedException();
        classResultList.Status.Should().Be("OK");
        classResultList.Results![0].Time.Should().Be(TimeSpan.FromMilliseconds(81200*10));
    }

    [TestMethod]
    public void DeserializeClassListTest()
    {
        var json = File.ReadAllText(@"Liveresultat\Classes.json");

        var classList = facade!.DeserializeJson<ClassList>(json) ?? throw new AssertFailedException();
        classList.Status.Should().Be("OK");
        classList.Classes.Should().NotBeNull();
        classList.Classes!.Any(c => c.ClassName == "Insk").Should().BeTrue();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        facade?.Dispose();
    }
}
