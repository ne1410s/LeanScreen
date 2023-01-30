// <copyright file="CollateModuleTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl.Tests;

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
    public void CollateEvenly_ForCryptFile_ProducesSecureResult()
    {
        // Arrange
        const string fileName = "4a3a54004ec9482cb7225c2574b0f889291e8270b1c4d61dbc1ab8d9fef4c9e0.mp4";
        var source = Path.Combine("Samples", fileName);
        const int total = 10;
        const string keyCsv = "9,0,2,1,0";
        const string expectedName = "b5f852c247434f5e677bb11d61b9626de2279fcd109b2cdbbf25573136474ebb.jpg";
        var destInfo = Directory.CreateDirectory(Guid.NewGuid().ToString());

        // Act
        var returnPath = (string)TestHelper.Route(
            $"collate evenly -s {source} -d {destInfo.Name} -t {total} -k {keyCsv}")!;

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
        const string expectedMd5Hex = "9d4483d67c4af8ffba8f9dba26144059";

        // Act
        var returnPath = (string)TestHelper.Route($"collate evenly -s {source} -d {destInfo.Name} -c 6")!;
        var md5Hex = new FileInfo(returnPath).Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.Should().Be(expectedMd5Hex);
    }

    [Fact]
    public void CollateManyEvenly_WithFiles_ReportsProgress()
    {
        // Arrange
        var root = TestHelper.CloneSamples();
        var mockWriter = new Mock<IOutputWriter>();

        // Act
        CollateModule.CollateManyEvenly(root, writer: mockWriter.Object);

        // Assert
        mockWriter.Verify(m => m.WriteLine("Collation: Start - Files: 4", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 25.00%", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 50.00%", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 75.00%", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 100.00%", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Collation: End", false), Times.Once());
        var generated = new DirectoryInfo(root).GetFiles("sample.*v*.jpg", SearchOption.AllDirectories);
        generated.Length.Should().Be(3);
    }

    [Fact]
    public void CollateManyEvenly_WithNoWriter_WritesToConsole()
    {
        // Arrange
        var root = TestHelper.CloneSamples();
        var writer = new StringWriter();
        Console.SetOut(writer);

        // Act
        CollateModule.CollateManyEvenly(root);

        // Assert
        writer.ToString().Should().Contain("Collation: Start");
    }
}