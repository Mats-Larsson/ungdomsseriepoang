﻿using System.Collections.Specialized;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Results;
using Results.Liveresultat;
using Results.Liveresultat.Model;
using Configuration = Results.Configuration;

namespace ResultsTests.Liveresultat;

[TestClass]
#pragma warning disable CA1001
public class LiveresultatResultSourceTests
{
    private readonly Configuration configuration = new() { LiveresultatId = 12345 };
    private readonly Mock<ILogger<LiveresultatFacade>> liveresultatFacadeLoggerMock = new();
    private LiveresultatFacade? liveresultatFacade;
    private readonly Mock<ILogger<LiveresultatResultSource>> liveresultatResultSourceLoggerMock = new();
    private readonly ClassFilter classFilter = new(new Configuration());
    private LiveresultatResultSource? liveresultatResultSource;

    [TestInitialize]
    public void Init()
    {
        liveresultatFacade = new LiveresultatFacadeMock(liveresultatFacadeLoggerMock.Object);
        liveresultatResultSource = new LiveresultatResultSource(configuration, liveresultatFacade, liveresultatResultSourceLoggerMock.Object, classFilter);
    }

    [TestMethod]
    public void CurrentTimeOfDay()
    {
        liveresultatResultSource!.CurrentTimeOfDay.Should().BePositive();
    }

    [TestMethod]
    public void SupportsPreliminary()
    {
        liveresultatResultSource!.SupportsPreliminary.Should().BeFalse();
    }
    
    [TestMethod]
    public void GetParticipantResults()
    {
        liveresultatResultSource!.GetParticipantResults().Count.Should().Be(945);
    }



    [TestCleanup]
    public void Cleanup()
    {
        liveresultatResultSource?.Dispose();
    }
}

class LiveresultatFacadeMock : LiveresultatFacade
{
    public LiveresultatFacadeMock(ILogger<LiveresultatFacade> logger) : base(logger)
    {
    }

    protected override Task<T?> GetDataAsync<T>(int competitionId, string method, string? hash,
        NameValueCollection? parameters = null) where T : class
    {
        DeserializationBase result = typeof(T).Name switch
        {
            "LastPassingList" => new LastPassingList { Passings = [new Passing { PassTimeRaw = "10:11:12" }] },
            "CompetitionInfo" => GetFromFile<T>(@"Liveresultat\CompetitionInfo.json"),
            "ClassList" => GetFromFile<T>(@"Liveresultat\Classes.json"),
            "ClassResultList" => GetFromFile<T>(@"Liveresultat\H16.json"),
            _ => null!
        };
        return Task.FromResult((T?)Convert.ChangeType(result, typeof(T)));
    }

    private T GetFromFile<T>(string fileName)
    {
        return this.DeserializeJson<T>(File.ReadAllText(fileName))!;
    }
}