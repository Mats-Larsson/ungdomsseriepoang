using ResultCalc;
using ResultCalc.Model;
using ResultCalc.Simulator;

namespace ResultCalcTests
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