using Results;
using Results.Model;
using Results.Simulator;

namespace ResultsTests
{
    [TestClass]
    public class SimulatorTest
    {
        [TestMethod]
        public void GetParticipantResults()
        {
            using IResultSource resultSource = new SimulatorResultSource(new Configuration { SpeedMultiplier = 1});

            var participantResults = resultSource.GetParticipantResults();
            Assert.IsNotNull(participantResults);
        }
    }
}