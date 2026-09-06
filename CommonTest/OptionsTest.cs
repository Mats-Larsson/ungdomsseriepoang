using AwesomeAssertions;
using Common;

namespace CommonTest
{
    [TestClass]
    public class OptionsTest
    {
        [TestMethod]
        public void Options()
        {
            string[] args = ["--version"];
            var options = Common.Options.Parse(args);
            Assert.IsNull(options);

            Console.WriteLine(Common.Options.HelpText);
        }

        [TestMethod]
        public void Pointscalc()
        {
            Parse(["--pointscalc"], true);
            Parse(["--pointscalc X"], true);
            Parse(["--pointscalc", "Final"]).PointsCalc.Should().Be(PointsCalcType.Final);
            Parse(["--pointscalc", "Normal"]).PointsCalc.Should().Be(PointsCalcType.Normal);
        }

        [TestMethod]
        public void IncludeExclude()
        {
            Parse(["--include", "A", "B", "C"]).IncludeClasses.Should().HaveCount(3);
            Parse(["--exclude", "A", "B", "C"]).ExcludeClasses.Should().HaveCount(3);
            Parse(["--exclude", "--listenerport", "80"], true);
        }

        private static Options Parse(string[] args, bool shouldBeNull = false)
        {
            var options = Common.Options.Parse(args);
            if (options != null) return options;

            var helpText = Common.Options.HelpText;
            if (!shouldBeNull) throw new AssertFailedException(helpText);

            Console.WriteLine(Common.Options.HelpText);
            return null!;
        }
    }
}