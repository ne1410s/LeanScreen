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
        const string source = "samples\\sample.mp4";
        const int total = 10;

        // Act
        var returnDest = Route($"snap evenly -s {source} -d {Guid.NewGuid()} -t {total}");

        // Assert
        Directory.GetFiles((string)returnDest!, "*.jpg").Length.Should().Be(total);
    }

    [Fact]
    public void SnapEvenly_ForFileWithCustomDestination_SavesFlexibly()
    {
        // Arrange
        const string source = "samples\\sample.mp4";
        var suppliedDest = Guid.NewGuid().ToString();
        var expectedDest = new DirectoryInfo(suppliedDest).FullName;

        // Act
        var returnDest = Route($"snap evenly -s {source} -d {suppliedDest}");

        // Assert
        returnDest.Should().Be(expectedDest);
        Directory.GetFiles((string)returnDest!, "*.jpg").Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SnapEvenly_ForFileWithDefaultDestination_SavesAdjacently()
    {
        // Arrange
        const string source = "samples\\sample.mp4";
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
        var returnDest = Route($"snap evenly -s \"{source}\"");

        // Assert
        returnDest.Should().Be(expectedDest);
        Directory.GetFiles((string)returnDest!, "*.jpg").Length.Should().BeGreaterThan(0);
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
