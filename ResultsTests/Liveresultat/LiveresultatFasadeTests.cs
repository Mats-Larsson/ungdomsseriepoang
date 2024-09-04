#pragma warning disable CA1001
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Results;
using Results.Liveresultat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultsTests.Liveresultat;

[TestClass]
public class LiveresultatFasadeTests
{
    private Configuration configurationMock = new() { LiveresultatComp = 30823 };
    private Mock<ILogger<LiveresultatFacade>> loggerMock = new();
    private LiveresultatFacade? facade;

    [TestInitialize]
    public void TestInitialize()
    {
        facade = new LiveresultatFacade(configurationMock, loggerMock.Object);
    }

    [TestMethod]
    public async Task GetClassesTest()
    {
        Debug.Assert(facade != null, nameof(facade) + " != null");

        var classes1 = await facade.GetClassesAsync();
        var classes2 = await facade.GetClassesAsync();

        classes1.Should().Be(classes2);
    }

    [TestMethod]
    public async Task GetClassResultAsync()
    {
        var classResultList = await facade!.GetClassResultAsync("Insk").ConfigureAwait(false);

        TimeSpan? startTime = classResultList!.Results![0].StartTime;
    }

    [TestCleanup]
    public void TestCleanup()
    {
        facade?.Dispose();
    }
}
