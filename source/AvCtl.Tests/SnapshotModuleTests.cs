using System.Reflection;
using System.Text.RegularExpressions;
using Comanche;
using Comanche.Services;

namespace AvCtl.Tests;

public class SnapshotModuleTests
{
    [Fact]
    public void SnapEvenly_ForFileWithCustomTotal_ProducesSameTotal()
    {
        // Arrange
        var source = Path.Combine("Samples", "sample.mp4");
        var destInfo = Directory.CreateDirectory($"pic_count10_{Guid.NewGuid()}");
        const int total = 10;

        // Act
        var returnDest = Route($"snap evenly -s {source} -d {destInfo.Name} -t {total}");

        // Assert
        Directory.GetFiles((string)returnDest!, "*.jpg").Length.Should().Be(total);
    }

    [Fact]
    public void SnapEvenly_ForFileWithDefaultDestination_SavesAdjacently()
    {
        // Arrange
        var source = Path.Combine("Samples", "sample.mp4");
        var expectedDest = new FileInfo(source).DirectoryName;

        // Act
        var returnDest = Route($"snap evenly -s {source}");

        // Assert
        returnDest.Should().Be(expectedDest);
        Directory.GetFiles((string)returnDest!, "*.jpg").Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SnapEvenly_ForUrlWithDefaultDestination_SavesLocally()
    {
        // Arrange
        const string source = "http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4";
        var expectedDest = Directory.GetCurrentDirectory();

        // Act
        var returnDest = Route($"snap evenly -s {source}");

        // Assert
        returnDest.Should().Be(expectedDest);
        Directory.GetFiles((string)returnDest!, "*.jpg").Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SnapEvenly_ForCryptFile_ProducesResults()
    {
        // Arrange
        var source = Path.Combine("Samples", "4a3a54004ec9482cb7225c2574b0f889291e8270b1c4d61dbc1ab8d9fef4c9e0.mp4");
        const string keyCsv = "9,0,2,1,0";
        var destInfo = Directory.CreateDirectory("pic_crypt");

        // Act
        var returnDest = Route($"snap evenly -s {source} -d {destInfo.Name} -k {keyCsv}");

        // Assert
        returnDest.Should().Be(destInfo.FullName);
        Directory.GetFiles((string)returnDest!, "*.jpg").Length.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("sample.avi")]
    [InlineData("sample.flv")]
    [InlineData("sample.mkv")]
    [InlineData("sample.mp4")]
    public void SnapEvenly_VaryingFile_ProducesExpected(string sampleFile)
    {
        // Arrange
        var source = Path.Combine("Samples", sampleFile);
        var folder = "pic_" + new FileInfo(source).Extension.TrimStart('.');
        var destInfo = Directory.CreateDirectory(folder);

        // Act
        Route($"snap evenly -s {source} -d {destInfo.Name}");

        // Assert
        // e.g. check a list of hashes perhaps?
    }

    private static object? Route(string consoleInput, IOutputWriter? writer = null)
    {
        Environment.ExitCode = 0;
        return Session.Route(
            Regex.Split(consoleInput, "\\s+"),
            Assembly.GetAssembly(typeof(SnapshotModule)),
            writer);
    }
}
