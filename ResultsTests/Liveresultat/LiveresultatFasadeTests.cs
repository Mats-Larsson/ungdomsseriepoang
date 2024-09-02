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
    public void GetClassesTest()
    {
        Debug.Assert(facade != null, nameof(facade) + " != null");

        int classes1 = facade.GetClassesAsync().Result!.Classes.Count;
        var classes2 = facade.GetClassesAsync().Result!.Classes.Count;

        classes1.Should().Be(classes2);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        facade?.Dispose();
    }
}
