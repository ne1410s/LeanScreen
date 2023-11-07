// <copyright file="CollateModuleTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl.Tests;

using Comanche.Models;
using Comanche.Services;
using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.IO;

/// <summary>
/// Tests for the <see cref="CollateModule"/>.
/// </summary>
public class CollateModuleTests
{
    [Fact]
    public void CollateEvenly_NoWriter_ThrowsException()
    {
        // Arrange
        IOutputWriter writer = null!;

        // Act
        var act = () => CollateModule.CollateEvenly(writer, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(writer));
    }

    [Fact]
    public void CollateEvenly_WithFiles_ReportsProgress()
    {
        // Arrange
        var mockWriter = new Mock<IOutputWriter>();

        // Act
        var act = () => CollateModule.CollateEvenly(mockWriter.Object, "blah");

        // Assert
        act.Should().ThrowExactly<InvalidOperationException>();
        mockWriter.Verify(m => m.WriteLine(It.Is<string>(s => s.StartsWith("Keys: 0, Check: ")), WriteStyle.Default));
    }

    [Fact]
    public void CollateEvenly_ForCryptFile_ProducesSecureResult()
    {
        // Arrange
        const string fileName = "4a3a54004ec9482cb7225c2574b0f889291e8270b1c4d61dbc1ab8d9fef4c9e0.mp4";
        var source = Path.Combine("Samples", fileName);
        const int total = 10;
        const string expectedName = "b5f852c247434f5e677bb11d61b9626de2279fcd109b2cdbbf25573136474ebb.jpg";
        var destInfo = Directory.CreateDirectory(Guid.NewGuid().ToString());

        // Act
        var returnPath = (string)TestHelper.Route(
            $"collate evenly -s {source} -d {destInfo.Name} -t {total} -ks Samples -kr xyz")!;

        // Assert
        returnPath.Should().Match($"*{fileName[..12]}.{expectedName}");
        File.Exists(returnPath).Should().BeTrue();
    }

    [Fact]
    public void CollateEvenly_WithColumns_ProducesExpectedBytes()
    {
        // Arrange
        var source = Path.Combine("Samples", "sample.mp4");
        var destInfo = Directory.CreateDirectory("col_6cols");
        const string expectedMd5Hex = "d941bdcda3112f3466d0059bac7198f9";

        // Act
        var returnPath = (string)TestHelper.Route($"collate evenly -s {source} -d {destInfo.Name} -c 6")!;
        var md5Hex = new FileInfo(returnPath).Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.Should().Be(expectedMd5Hex);
    }

    [Fact]
    public void CollateManyEvenly_NoWriter_ThrowsException()
    {
        // Arrange
        IOutputWriter writer = null!;

        // Act
        var act = () => CollateModule.CollateManyEvenly(writer, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(writer));
    }

    [Fact]
    public void CollateManyEvenly_WithFiles_ReportsProgress()
    {
        // Arrange
        var root = TestHelper.CloneSamples();
        var mockWriter = new Mock<IOutputWriter>();
        const WriteStyle style = WriteStyle.Default;

        // Act
        CollateModule.CollateManyEvenly(mockWriter.Object, root);

        // Assert
        mockWriter.Verify(m => m.WriteLine(It.Is<string>(s => s.StartsWith("Keys: 0, Check: ")), style));
        mockWriter.Verify(m => m.WriteLine("Collation: Start - Files: 4", style), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 25.00%", style), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 50.00%", style), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 75.00%", style), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 100.00%", style), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Collation: End", style), Times.Once());
        var generated = new DirectoryInfo(root).GetFiles("sample.*v*.jpg", SearchOption.AllDirectories);
        generated.Length.Should().Be(3);
        Directory.Delete(root, true);
    }

    [Fact]
    public void CollateManyEvenly_WithMockWriter_WritesToWriter()
    {
        // Arrange
        var root = TestHelper.CloneSamples();
        var mockWriter = new Mock<IOutputWriter>();

        // Act
        CollateModule.CollateManyEvenly(mockWriter.Object, root);
        Directory.Delete(root, true);

        // Assert
        mockWriter.Verify(
            m => m.WriteLine(
                It.Is<string>(s => s.StartsWith("Collation: Start")),
                WriteStyle.Default));
    }
}