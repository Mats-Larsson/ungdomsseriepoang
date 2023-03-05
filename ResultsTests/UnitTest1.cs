using Results;
using Results.Model;
using Results.Ola;

namespace ResultsTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            using IResultSource resultSource = new OlaResultSource(new Configuration());

            var participantResults = resultSource.GetParticipantResults();
        }

        [TestMethod]
        public void TestPatrolActivated()
        {
            var opr = new OlaParticipantResult[]
            { 
                Create(ParticipantStatus.Activated, 1, 2),
                Create(ParticipantStatus.Activated, 2, 1),
                Create(ParticipantStatus.Activated, 3),
            };
            var actual = OlaResultSource.GetExtraParticipants(opr).ToArray();
            Assert.AreEqual(1, actual.Length);
            Assert.IsTrue(new[] { 1, 2 }.Contains(actual[0]));

        }

        [TestMethod]
        public void TestPatrolActivatedSingleLinked()
        {
            var opr = new OlaParticipantResult[]
            {
                Create(ParticipantStatus.Activated, 1, 2),
                Create(ParticipantStatus.Activated, 2),
                Create(ParticipantStatus.Activated, 3),
            };
            var actual = OlaResultSource.GetExtraParticipants(opr).ToArray();
            Assert.AreEqual(1, actual.Length);
            Assert.IsTrue(new[] { 1, 2 }.Contains(actual[0]));

        }

        [TestMethod]
        public void TestPatrolNotActivated()
        {
            var opr = new OlaParticipantResult[]
            {
                Create(ParticipantStatus.NotActivated, 1, 2),
                Create(ParticipantStatus.NotActivated, 2, 1),
                Create(ParticipantStatus.Activated, 3),
            };
            var actual = OlaResultSource.GetExtraParticipants(opr).ToArray();
            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void TestPatrolWithActivatedAndNotActivated()
        {
            var opr = new OlaParticipantResult[]
            {
                Create(ParticipantStatus.Activated, 1, 2),
                Create(ParticipantStatus.NotActivated, 2, 1),
                Create(ParticipantStatus.Activated, 3),
            };
            var actual = OlaResultSource.GetExtraParticipants(opr).ToArray();
            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void TestMTestPatrolWithActivatedActivatedActivated()
        {
            var opr = new OlaParticipantResult[]
            {
                Create(ParticipantStatus.Activated, 2, 1),
                Create(ParticipantStatus.Activated, 1, 2),
                Create(ParticipantStatus.Activated, 3, 2),
                Create(ParticipantStatus.Activated, 4),
            };
            var actual = OlaResultSource.GetExtraParticipants(opr).ToArray();
            Assert.AreEqual(2, actual.Length, $"[{string.Join(", ", actual)}]");
        }

        private static OlaParticipantResult Create(ParticipantStatus status, int id, int? togetherWithId = null)
        {
            return new("", "", "", null, null, status, id, togetherWithId);
        }
    }
}