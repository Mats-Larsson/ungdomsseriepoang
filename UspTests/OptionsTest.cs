using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UspTests
{
    [TestClass]
    public class OptionsTest
    {
        [TestMethod]
        public void Options()
        {
            string[] args = { "--version" };
            var options = Usp.Options.Parse(args);
            Assert.IsNull(options);

            Console.WriteLine(Usp.Options.HelpText);
        }
    }
}