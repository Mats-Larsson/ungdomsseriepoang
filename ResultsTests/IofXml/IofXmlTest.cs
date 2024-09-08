using Microsoft.Extensions.Logging;
using Moq;
using Results;
using Results.IofXml;
using System.Diagnostics.CodeAnalysis;

namespace ResultsTests.IofXml;

[TestClass]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
public class IofXmlTest
{
    private const string IOF_XML_INPUT_FOLDER = @".\IofXml";
    private readonly Mock<Configuration> configurationMock = new();
    private readonly Mock<ILogger<FileListener>> loggerMock = new();

    [TestMethod]
    public Task TestMethod1()
    {
        if (!Directory.Exists(IOF_XML_INPUT_FOLDER)) Directory.CreateDirectory(IOF_XML_INPUT_FOLDER);
        //        File.Delete(IOF_XML_INPUT_FOLDER + "\\*");
        configurationMock.Setup(m => m.IofXmlInputFolder).Returns(IOF_XML_INPUT_FOLDER);

        using var fileListener = new FileListener(configurationMock.Object, loggerMock.Object);
        using var resultSource = new IofXmlResultSource(configurationMock.Object, fileListener);
        
        IofXmlResultSource.LoadResultFile(@".\IofXml\IofResultat.Xml");
        return Task.CompletedTask;
    }
}
