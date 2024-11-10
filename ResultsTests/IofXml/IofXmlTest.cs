using Microsoft.Extensions.Logging;
using Moq;
using Results;
using Results.IofXml;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;

namespace ResultsTests.IofXml;

[TestClass]
[DoNotParallelize]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
public sealed class IofXmlTest : IDisposable
{
    private readonly string iofXmlInputFolder = Path.GetFullPath(@".\IofXml");
    private readonly string listenFolder = Path.GetFullPath(@".\listen");
    private readonly Mock<Configuration> configurationMock = new();
    private readonly Mock<ILogger<FileListener>> fileListenerLoggerMock = new();
    private readonly Mock<ILogger<IofXmlResultSource>> resultSourceLoggerMock = new();
    private FileListener? fileListener;
    private IofXmlResultSource? resultSource;

    public IofXmlTest()
    {
        configurationMock.Setup(m => m.IofXmlInputFolder).Returns(listenFolder);
    }

    [TestInitialize]
    public void Init()
    {
        if (!Directory.Exists(listenFolder)) Directory.CreateDirectory(listenFolder);
        Directory.EnumerateFiles(listenFolder, "*")
            .ToList()
            .ForEach(s =>
            {
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        File.Delete(s);
                        break;
                    }
                    catch
                    {
                        Task.Delay(10).Wait();
                    }
                }
            });

        fileListener = new FileListener(configurationMock.Object, fileListenerLoggerMock.Object);
        resultSource = new IofXmlResultSource(configurationMock.Object, fileListener, resultSourceLoggerMock.Object);
        resultSource.GetParticipantResults().Should().BeEmpty();
    }

    [TestCleanup]
    public void Cleanup()
    {
        Dispose();
    }

    [TestMethod]
    public void TestReadEmptyFile()
    {
        Assert.IsNotNull(fileListener);
        Assert.IsNotNull(resultSource);

        ApplyFile("Resultat0.xml");
        AssertHasResult(0);
    }

    [TestMethod]
    public void TestRead4ResultFiles()
    {
        Assert.IsNotNull(fileListener);
        Assert.IsNotNull(resultSource);

        ApplyFile("Resultat0.xml");
        AssertHasResult(0);

        ApplyFile("Resultat1.xml");
        AssertHasResult(8);
        
        ApplyFile("Resultat2.xml");
        AssertHasResult(230);
        
        ApplyFile("Resultat3.xml");
        AssertHasResult(231);
    }

    [TestMethod]
    public void TestReadBadFile()
    {
        Assert.IsNotNull(fileListener);
        Assert.IsNotNull(resultSource);

        ApplyFile("Bad.xml");
        AssertHasResult(0);

        ApplyFile("Resultat2.xml");
        AssertHasResult(230);
        
        ApplyFile("Bad.xml");
        AssertHasResult(230);
    }

    private void AssertHasResult(int numResults)
    {
        for (int i = 0; i < 50; i++)
        {
            if (resultSource!.GetParticipantResults().Any()) break;
            Task.Delay(10).Wait();
        }

        resultSource!.GetParticipantResults().Should().HaveCount(numResults);
    }

    private void ApplyFile(string file)
    {
        var sourcePath = $"{iofXmlInputFolder}\\{file}";
        var destPath = $"{listenFolder}\\{file}";
        File.Copy(sourcePath, destPath);
        while (File.Exists(destPath))
        {
            Task.Delay(10).Wait();
        }
    }

    public void Dispose()
    {
        fileListener?.Dispose();
        fileListener = null;
        resultSource?.Dispose();
        resultSource = null;
    }
}