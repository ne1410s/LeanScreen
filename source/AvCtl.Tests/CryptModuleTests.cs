// <copyright file="CryptModuleTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl.Tests;

using Av;
using Av.Models;
using Comanche.Models;
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
        const WriteStyle style = WriteStyle.Default;

        // Act
        CryptModule.EncryptMedia(mockWriter.Object, root);

        // Assert
        mockWriter.Verify(m => m.Write(It.Is<string>(s => s.StartsWith("Keys: 0, Check: ")), style, true));
        mockWriter.Verify(m => m.Write("Encryption: Start - Files: 4", style, true), Times.Once());
        mockWriter.Verify(m => m.Write("Done: 25.00%", style, true), Times.Once());
        mockWriter.Verify(m => m.Write("Done: 50.00%", style, true), Times.Once());
        mockWriter.Verify(m => m.Write("Done: 75.00%", style, true), Times.Once());
        mockWriter.Verify(m => m.Write("Done: 100.00%", style, true), Times.Once());
        mockWriter.Verify(m => m.Write("Encryption: End", style, true), Times.Once());
        mockWriter.Verify(
            m => m.Write(It.Is<string>(s => s.StartsWith(" - Not secured: ")), style, true),
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
            m => m.Write(
                It.Is<string>(s => s.StartsWith("Encryption: Start")),
                WriteStyle.Default,
                true));
    }

    [Fact]
    public void EncryptMedia_WithDepth_MovesFile()
    {
        // Arrange
        const int groupLength = 3;
        const string fileName = "4349535bdc7d5452054e1fc4485a566f6ed54337a90f9d37499031eee427a809.mkv";
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
    public void EncryptFile_WithWriter_WritesExpected()
    {
        // Arrange
        var mockWriter = new Mock<IOutputWriter>();

        // Act
        var act = () => CryptModule.EncryptFile(mockWriter.Object, "blah");

        // Assert
        act.Should().Throw<FileNotFoundException>();
        mockWriter.Verify(m => m.Write(It.Is<string>(s => s.StartsWith("Keys: 0, Check: ")), WriteStyle.Default, true));
    }

    [Fact]
    public void EncryptFile_WithSource_ReturnsExpected()
    {
        // Arrange
        var root = TestHelper.CloneSamples();
        var mockWriter = new Mock<IOutputWriter>();
        var inputPath = Path.Combine(root, "sample.avi");
        const string expectedName = "471d7c1d2426b48c6115cb1c364fdfcaa65afcc8dd9cd4301121d28794e328ec.avi";
        var expected = new FileInfo(Path.Combine(root, expectedName)).FullName;

        // Act
        var returnPath = CryptModule.EncryptFile(mockWriter.Object, inputPath);
        Directory.Delete(root, true);

        // Assert
        returnPath.Should().Be(expected);
    }
}
