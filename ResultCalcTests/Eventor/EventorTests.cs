using Moq;
using ResultCalc;
using ResultCalc.Eventor;
using ResultCalc.IofXml;

namespace ResultCalcTests.Eventor;

[TestClass]
public class EventorTests
{

    [TestMethod]
    public void Test()
    {
        Configuration configuration = new();
        Mock<IEventorFacade> eventorFacadeMock = new();
        eventorFacadeMock.Setup(x => x.GetIofXmlStream(It.IsAny<int>()))
            .ReturnsAsync((int _) => new FileStream(@".\Eventor\Resultat1.xml", FileMode.Open));
        using EventorResultSource eventorResultSource = new(configuration, eventorFacadeMock.Object, new IofXmlDeserializer());

        var participantResults = eventorResultSource.GetParticipantResults();
        Assert.HasCount(8, participantResults);
    } 
}