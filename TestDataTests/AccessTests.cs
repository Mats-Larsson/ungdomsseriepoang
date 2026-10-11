using System.Xml.Linq;
using AwesomeAssertions;
using TestData;

namespace TestDataTests;

[TestClass]
public class AccessTests
{
    [TestMethod]
    public void ReadFiles()
    {
        var files = Access.ReadFiles(Access.Final2026ZipPath).ToList();

        files.Should().HaveCount(985);
        files.Should().AllSatisfy(f => { _ = XDocument.Parse(f.Content); });

        files[0].Timestamp.Should().Be(new DateTime(2026, 10, 4, 9, 4, 27, 7));
        files[^1].Timestamp.Should().Be(new DateTime(2026, 10, 4, 12, 6, 56, 100));
        files[0].TimeToNext.Should().Be(TimeSpan.FromMilliseconds(226));
        files[^1].TimeToNext.Should().BeNull();
        files.Take(files.Count - 1).Should().AllSatisfy(f => f.TimeToNext.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero));

        // 09:04:27.007 (first file) -> 12:06:56.100 (last file)
        var totalTime = files.Aggregate(TimeSpan.Zero, (sum, f) => sum + (f.TimeToNext ?? TimeSpan.Zero));
        totalTime.Should().Be(new TimeSpan(0, 3, 2, 29, 93));
    }
}
