using ResultCalc;
using ResultCalc.Model;
using ResultCalc.Simulator;

namespace ResultCalcTests
{
    [TestClass]
    public class SimulatorTest
    {
        [TestMethod]
        public async Task GetParticipantResultsAsync()
        {
            using IResultSource resultSource = new SimulatorResultSource(new Configuration { SpeedMultiplier = 1});

            var participantResults = await resultSource.GetParticipantResultsAsync().ConfigureAwait(true);
            Assert.IsNotNull(participantResults);
        }
    }
}