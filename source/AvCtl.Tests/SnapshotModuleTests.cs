// <copyright file="SnapshotModuleTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl.Tests;

using System.Globalization;
using System.Text.RegularExpressions;
using Comanche.Models;
using Comanche.Services;

/// <summary>
/// Tests for the <see cref="SnapshotModule"/>.
/// </summary>
public class SnapshotModuleTests
{
    [Fact]
    public void SnapEvenly_NoWriter_ThrowsException()
    {
        // Arrange
        IOutputWriter writer = null!;

        // Act
        var act = () => SnapshotModule.SnapEvenly(writer, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(writer));
    }

    [Fact]
    public void SnapEvenly_WithWriter_WritesExpected()
    {
        // Arrange
        var mockWriter = new Mock<IOutputWriter>();

        // Act
        var act = () => SnapshotModule.SnapEvenly(mockWriter.Object, "blah");

        // Assert
        act.Should().Throw<InvalidOperationException>();
        mockWriter.Verify(m => m.WriteLine(It.Is<string>(s => s.StartsWith("Keys: 0, Check: ")), WriteStyle.Default));
    }

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
        destInfo.Delete(true);
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
        destInfo.Delete(true);
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
        const string source = "https://download.samplelib.com/mp4/sample-5s.mp4";
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
        var destInfo = Directory.CreateDirectory("pic_crypt");

        // Act
        var returnDest = TestHelper.Route($"snap evenly -s {source} -d {destInfo.Name} -ks Samples -kr xyz -h 30");

        // Assert
        returnDest.Should().Be(destInfo.FullName);
        Directory.GetFiles((string)returnDest!, "*.jpg").Length.Should().BeGreaterThan(0);
        destInfo.Delete(true);
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
        var act = () => TestHelper.Route($"snap evenly -s {source} -d {destInfo.Name}");

        // Assert
        act.Should().NotThrow();
        destInfo.Delete(true);
    }

    [Fact]
    public void SnapSingleFrame_NoWriter_ThrowsException()
    {
        // Arrange
        IOutputWriter writer = null!;

        // Act
        var act = () => SnapshotModule.SnapSingleFrame(writer, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(writer));
    }

    [Fact]
    public void SnapSingleFrame_WithWriter_WritesExpected()
    {
        // Arrange
        var mockWriter = new Mock<IOutputWriter>();

        // Act
        var act = () => SnapshotModule.SnapSingleFrame(mockWriter.Object, "blah");

        // Assert
        act.Should().Throw<InvalidOperationException>();
        mockWriter.Verify(m => m.WriteLine(It.Is<string>(s => s.StartsWith("Keys: 0, Check: ")), WriteStyle.Default));
    }

    [Fact]
    public void SnapSingleFrame_ForFile_ProducesFileApproxFrame()
    {
        // Arrange
        var source = Path.Combine("Samples", "sample.mp4");
        var destInfo = Directory.CreateDirectory($"pic_snap1_{Guid.NewGuid()}");
        const int mediaFrames = 300;
        const double position = 0.5;
        const int expectedFrame = (int)(mediaFrames * position);

        // Act
        var returnPath = TestHelper.Route($"snap frame -s {source} -d {destInfo.Name} -r {position}");
        var frameNo = int.Parse(
            Regex.Match((string)returnPath!, "_f(?<frame>\\d+)").Groups["frame"].Value,
            CultureInfo.InvariantCulture);

        // Assert
        frameNo.Should().BeCloseTo(expectedFrame, 20);
        destInfo.Delete(true);
    }

    [Fact]
    public void SnapSingleFrame_ForCryptFile_ProducesResults()
    {
        // Arrange
        var source = Path.Combine("Samples", "4a3a54004ec9482cb7225c2574b0f889291e8270b1c4d61dbc1ab8d9fef4c9e0.mp4");
        var destInfo = Directory.CreateDirectory($"pic_snap1_{Guid.NewGuid()}");
        const string expectedFileName = "4a3a54004ec9."
            + "0add1090e773790288e7f9e1525390799b47007ef9ea757fba22ba24807cbc36.jpg";

        // Act
        var returnPath = TestHelper.Route($"snap frame -s {source} -d {destInfo} -ks Samples -kr xyz");

        // Assert
        returnPath.Should().Be(Path.Combine(destInfo.FullName, expectedFileName));
        destInfo.Delete(true);
    }

    [Fact]
    public void SnapSingleFrame_ForCryptFileWithHeight_ProducesExpected()
    {
        // Arrange
        var source = Path.Combine("Samples", "4a3a54004ec9482cb7225c2574b0f889291e8270b1c4d61dbc1ab8d9fef4c9e0.mp4");
        var destInfo = Directory.CreateDirectory($"pic_snap1_{Guid.NewGuid()}");
        const string expectedFileName = "4a3a54004ec9."
            + "86b327d4cec76cec1aa6842dce52405bb88d5cb4bb132cfd579b0f32b8ed2d87.jpg";

        // Act
        var returnPath = TestHelper.Route($"snap frame -s {source} -d {destInfo} -ks Samples -kr xyz -h 50");

        // Assert
        returnPath.Should().Be(Path.Combine(destInfo.FullName, expectedFileName));
        destInfo.Delete(true);
    }

    [Fact]
    public void SnapSingleFrame_ForUrlWithDefaultDestination_SavesLocally()
    {
        // Arrange
        const string source = "https://download.samplelib.com/mp4/sample-5s.mp4";
        var expectedDest = Directory.GetCurrentDirectory();
        var expectedPath = Path.Combine(expectedDest, "p0.75_f125.jpg");

        // Act
        var returnPath = TestHelper.Route($"snap frame -s {source} -r 0.75");

        // Assert
        returnPath.Should().Be(expectedPath);
    }
}
