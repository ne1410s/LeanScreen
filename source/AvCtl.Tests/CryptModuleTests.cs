// <copyright file="CryptModuleTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl.Tests;

using Av;
using Av.Models;
using Comanche.Services;

/// <summary>
/// Tests for the <see cref="CryptModule"/>.
/// </summary>
public class CryptModuleTests
{
    [Fact]
    public void EncryptMedia_NoWriter_ThrowsException()
    {
        // Arrange
        IOutputWriter writer = null!;

        // Act
        var act = () => CryptModule.EncryptMedia(writer, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(writer));
    }

    [Fact]
    public void EncryptMedia_WithFiles_ReportsProgress()
    {
        // Arrange
        var root = TestHelper.CloneSamples();
        var mockWriter = new Mock<IOutputWriter>();

        // Act
        CryptModule.EncryptMedia(mockWriter.Object, root);

        // Assert
        mockWriter.Verify(m => m.WriteLine(It.Is<string>(s => s.StartsWith("Keys: 0, Check: ")), false));
        mockWriter.Verify(m => m.WriteLine("Encryption: Start - Files: 4", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 25.00%", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 50.00%", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 75.00%", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 100.00%", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Encryption: End", false), Times.Once());
        mockWriter.Verify(
            m => m.WriteLine(It.Is<string>(s => s.StartsWith(" - Not secured: ")), false),
            Times.Exactly(3));

        var remains = new DirectoryInfo(root).EnumerateMedia(MediaTypes.AnyMedia, false);
        remains.Should().BeEmpty();
        Directory.Delete(root, true);
    }

    [Fact]
    public void EncryptMedia_WithNoWriter_WritesToConsole()
    {
        // Arrange
        var root = TestHelper.CloneSamples();
        var mockWriter = new Mock<IOutputWriter>();

        // Act
        CryptModule.EncryptMedia(mockWriter.Object, root);
        Directory.Delete(root, true);

        // Assert
        mockWriter.Verify(
            m => m.WriteLine(
                It.Is<string>(s => s.StartsWith("Encryption: Start")),
                false));
    }

    [Fact]
    public void EncryptMedia_WithDepth_MovesFile()
    {
        // Arrange
        const int groupLength = 3;
        const string fileName = "44e339204806870505a2a448115b2e554080cee37ddfb46949e47f1c586b011f.mkv";
        var root = TestHelper.CloneSamples();
        var expectedLocation = Path.Combine(root, fileName[..groupLength], fileName);
        var expectedRemoval = Path.Combine(root, fileName);
        var consoleWriter = new Mock<IOutputWriter>();

        // Act
        CryptModule.EncryptMedia(consoleWriter.Object, root, groupLabelLength: groupLength);
        var creationCheck = File.Exists(expectedLocation);
        var removalCheck = !File.Exists(expectedRemoval);
        Directory.Delete(root, true);

        // Assert
        creationCheck.Should().BeTrue();
        removalCheck.Should().BeTrue();
    }

    [Fact]
    public void EncryptFile_NoWriter_ThrowsException()
    {
        // Arrange
        IOutputWriter writer = null!;

        // Act
        var act = () => CryptModule.EncryptFile(writer, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(writer));
    }

    [Fact]
    public void EncryptFile_WithSource_ReturnsExpected()
    {
        // Arrange
        var root = TestHelper.CloneSamples();
        var mockWriter = new Mock<IOutputWriter>();
        var inputPath = Path.Combine(root, "sample.avi");
        const string expectedName = "0f5bed56f862512644ec87b7db6afc7299e2195c5bf9b27bcc631adb16785ed9.avi";
        var expected = new FileInfo(Path.Combine(root, expectedName)).FullName;

        // Act
        var returnPath = CryptModule.EncryptFile(mockWriter.Object, inputPath);
        Directory.Delete(root, true);

        // Assert
        returnPath.Should().Be(expected);
    }
}
