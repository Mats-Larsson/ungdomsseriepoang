﻿using Microsoft.Extensions.Logging;
using Moq;
using Results;
using Results.IofXml;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultsTests.IofXml;

[TestClass]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
public class FileListenerTests
{
    private bool foundNewFile;

    [TestMethod]
    public async Task FileListenerTestAsync()
    {
        Mock<Configuration> configurationMock = new();
        Mock<ILogger<FileListener>> loggerMock = new();
        configurationMock.Setup(m => m.IofXmlInputFolder).Returns(".");

        using var fileListener = new FileListener(configurationMock.Object, loggerMock.Object);

        fileListener.NewFile += FileListener_NewFile;

        await WriteFileAsync(@".\Test.xml", 10, TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);

        Task.Delay(10).Wait();
        Assert.IsTrue(foundNewFile);
    }

    private void FileListener_NewFile(object? sender, NewFileEventArgs e)
    {
        foundNewFile = true;
    }

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