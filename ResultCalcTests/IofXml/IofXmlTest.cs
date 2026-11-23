using Microsoft.Extensions.Logging;
using Moq;
using ResultCalc;
using ResultCalc.IofXml;
using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;

namespace ResultCalcTests.IofXml;

[TestClass]
[DoNotParallelize]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
public sealed class IofXmlTest : IDisposable
{
    private readonly string iofXmlInputFolder = Path.GetFullPath(@".\IofXml");
    private readonly string listenFolder = Path.GetFullPath(@".\listen");
    private readonly Mock<Configuration> configurationMock = new();
    private readonly Mock<ILogger<IofXmlResultSource>> resultSourceLoggerMock = new();
    private FileListener? fileListener;
    private IofXmlResultSource? resultSource;

    public IofXmlTest()
    {
        configurationMock.Setup(m => m.IofXmlInputFolder).Returns(listenFolder);
    }

    [TestInitialize]
    public async Task Init()
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

        fileListener = new FileListener(configurationMock.Object);
        var iofXmlDeserializer = new IofXmlDeserializer();
        resultSource = new IofXmlResultSource(configurationMock.Object, fileListener, resultSourceLoggerMock.Object, iofXmlDeserializer);
        (await resultSource.GetParticipantResultsAsync().ConfigureAwait(true)).Should().BeEmpty();
    }

    [TestCleanup]
    public void Cleanup()
    {
        Dispose();
    }

    [TestMethod]
    public async Task TestReadEmptyFile()
    {
        Assert.IsNotNull(fileListener);
        Assert.IsNotNull(resultSource);

        ApplyFile("Resultat0.xml");
        await AssertHasResultAsync(0).ConfigureAwait(true);
    }

    [TestMethod]
    public async Task TestRead4ResultFiles()
    {
        Assert.IsNotNull(fileListener);
        Assert.IsNotNull(resultSource);

        ApplyFile("Resultat0.xml");
        await AssertHasResultAsync(0).ConfigureAwait(true);

        ApplyFile("Resultat1.xml");
        await AssertHasResultAsync(8).ConfigureAwait(true);
        
        ApplyFile("Resultat2.xml");
        await AssertHasResultAsync(230).ConfigureAwait(true);
        
        ApplyFile("Resultat3.xml");
        await AssertHasResultAsync(231).ConfigureAwait(true);
    }

    [TestMethod]
    public async Task TestReadBadFile()
    {
        Assert.IsNotNull(fileListener);
        Assert.IsNotNull(resultSource);

        ApplyFile("Bad.xml");
        await AssertHasResultAsync(0).ConfigureAwait(true);

        ApplyFile("Resultat2.xml");
        await AssertHasResultAsync(230).ConfigureAwait(true);
        
        ApplyFile("Bad.xml");
        await AssertHasResultAsync(230).ConfigureAwait(true);
    }

    private async Task AssertHasResultAsync(int numResults)
    {
        // The file is renamed before the result is stored, so wait for the expected count rather than any result
        for (int i = 0; i < 500; i++)
        {
            if ((await resultSource!.GetParticipantResultsAsync().ConfigureAwait(true)).Count == numResults) break;
            await Task.Delay(10).ConfigureAwait(true);
        }

        (await resultSource!.GetParticipantResultsAsync().ConfigureAwait(true)).Should().HaveCount(numResults);
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