using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Usp;

namespace UspTests
{
    [TestClass]
    public class OptionsTest
    {
        [TestMethod]
        public void Options()
        {
            string[] args = ["--version"];
            var options = Usp.Options.Parse(args);
            Assert.IsNull(options);

            Console.WriteLine(Usp.Options.HelpText);
        }
        [TestMethod]
        public void Pointscalc()
        {
            Parse(["--pointscalc"], true);
            Parse(["--pointscalc X"], true);
            Parse(["--pointscalc", "Final"]).PointsCalc.Should().Be(PointsCalcType.Final);
            Parse(["--pointscalc", "Normal"]).PointsCalc.Should().Be(PointsCalcType.Normal);
        }

        private static Options Parse(string[] args, bool shouldBeNull = false)
        {
            Options? options = Usp.Options.Parse(args);
            if (options == null)
            {
                if (!shouldBeNull) throw new AssertFailedException(Usp.Options.HelpText);
                CommandLine.Text.HelpText? helpText = Usp.Options.HelpText;

                Console.WriteLine(Usp.Options.HelpText);
                return default!;
            }
            return options;
        }
    }
}