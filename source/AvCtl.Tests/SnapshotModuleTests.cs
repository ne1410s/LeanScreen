// <copyright file="SnapshotModuleTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.RegularExpressions;

namespace AvCtl.Tests;

/// <summary>
/// Tests for the <see cref="SnapshotModule"/>.
/// </summary>
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
        var returnDest = TestHelper.Route($"snap evenly -s {source} -d {destInfo.Name} -t {total}");

        // Assert
        Directory.GetFiles((string)returnDest!, "*.jpg").Length.Should().Be(total);
    }

    [Fact]
    public void SnapEvenly_WithFile_FileNamingConventionsApplied()
    {
        // Arrange
        var source = Path.Combine("Samples", "sample.mp4");
        var destInfo = Directory.CreateDirectory($"pic_names_{Guid.NewGuid()}");
        const int total = 9;
        var expectedPrefixes = Enumerable.Range(1, total).Select(i => $"n{i}_f");

        // Act
        var returnDest = TestHelper.Route($"snap evenly -s {source} -d {destInfo.Name} -t {total}");
        var actualPrefixes = Directory.GetFiles((string)returnDest!, "*.jpg")
            .Select(name => new FileInfo(name).Name[..4]);

        // Assert
        actualPrefixes.Should().BeEquivalentTo(expectedPrefixes);
    }

    [Fact]
    public void SnapEvenly_ForFileWithDefaultDestination_SavesAdjacently()
    {
        // Arrange
        var source = Path.Combine("Samples", "sample.mp4");
        var expectedDest = new FileInfo(source).DirectoryName;

        // Act
        var returnDest = TestHelper.Route($"snap evenly -s {source}");

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
        var returnDest = TestHelper.Route($"snap evenly -s {source}");

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
        var returnDest = TestHelper.Route($"snap evenly -s {source} -d {destInfo.Name} -k {keyCsv}");

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
        TestHelper.Route($"snap evenly -s {source} -d {destInfo.Name}");

        // Assert
        // e.g. check a list of hashes perhaps?
    }

    [Fact]
    public void SnapSingle_ForFile_ProducesFileApproxFrame()
    {
        // Arrange
        var source = Path.Combine("Samples", "sample.mp4");
        var destInfo = Directory.CreateDirectory($"pic_snap1_{Guid.NewGuid()}");
        const int mediaFrames = 973;
        const double position = 0.5;
        const int expectedFrame = (int)(mediaFrames * position);

        // Act
        var returnDest = TestHelper.Route($"snap single -s {source} -d {destInfo.Name} -r {position}");
        var filePath = Directory.GetFiles((string)returnDest!, "*.jpg").Single();
        var fileName = new FileInfo(filePath).Name;
        var frameNo = int.Parse(
            Regex.Match(fileName, "_f(?<frame>\\d+)").Groups["frame"].Value,
            CultureInfo.InvariantCulture);

        // Assert
        frameNo.Should().BeCloseTo(expectedFrame, 20);
    }

    [Fact]
    public void SnapSingle_ForCryptFile_ProducesResults()
    {
        // Arrange
        var source = Path.Combine("Samples", "4a3a54004ec9482cb7225c2574b0f889291e8270b1c4d61dbc1ab8d9fef4c9e0.mp4");
        var expectedDest = new FileInfo(source).DirectoryName;
        const string keyCsv = "9,0,2,1,0";

        // Act
        var returnDest = TestHelper.Route($"snap single -s {source} -k {keyCsv}");

        // Assert
        returnDest.Should().Be(expectedDest);
        Directory.GetFiles((string)returnDest!, "p0.3*.jpg").Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SnapSingle_ForUrlWithDefaultDestination_SavesLocally()
    {
        // Arrange
        const string source = "http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4";
        var expectedDest = Directory.GetCurrentDirectory();

        // Act
        var returnDest = TestHelper.Route($"snap single -s {source} -r 0.75");

        // Assert
        returnDest.Should().Be(expectedDest);
        Directory.GetFiles((string)returnDest!, "p0.75*.jpg").Length.Should().BeGreaterThan(0);
    }
}
