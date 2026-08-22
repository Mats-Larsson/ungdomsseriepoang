using Moq;
using Results;
using Results.IofXml;
using System.Diagnostics.CodeAnalysis;

namespace ResultsTests.IofXml;

[TestClass]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
public class FileListenerTests
{
    private bool foundNewFile;

    [TestMethod]
    public async Task FileListenerTestAsync()
    {
        var ct = CancellationToken.None;
        Mock<Configuration> configurationMock = new();
        configurationMock.Setup(m => m.IofXmlInputFolder).Returns(".");

        using var fileListener = new FileListener(configurationMock.Object);

        fileListener.NewFile += FileListener_NewFile;

        await WriteFileAsync(@".\Test.xml", 10, TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);

        await Task.Delay(10, ct).WaitAsync(ct);
        Assert.IsTrue(foundNewFile);
    }

    private void FileListener_NewFile(object? sender, NewFileEventArgs e)
    {
        foundNewFile = true;
    }

    [SuppressMessage("ReSharper", "UseAwaitUsing")]
    private static async Task WriteFileAsync(string path, int numLines, TimeSpan lineDelay)
    {
        using StreamWriter writer = File.CreateText(path);
        for (int i = 0; i < numLines; i++)
        {
            _ = writer.WriteLineAsync($"Line {i}").ConfigureAwait(false);
            await Task.Delay(lineDelay).ConfigureAwait(false);
        }
    }
}
