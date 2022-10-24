// <copyright file="CollateModuleTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.IO;

namespace AvCtl.Tests;

/// <summary>
/// Tests for the <see cref="CollateModule"/>.
/// </summary>
public class CollateModuleTests
{
    [Fact]
    public void CollateEvenly_ForCryptFile_ProducesResult()
    {
        // Arrange
        var source = Path.Combine("Samples", "4a3a54004ec9482cb7225c2574b0f889291e8270b1c4d61dbc1ab8d9fef4c9e0.mp4");
        const int total = 10;
        const string keyCsv = "9,0,2,1,0";
        var destInfo = Directory.CreateDirectory(Guid.NewGuid().ToString());

        // Act
        var returnPath = (string)TestHelper.Route(
            $"collate evenly -s {source} -d {destInfo.Name} -t {total} -k {keyCsv}")!;

        // Assert
        returnPath.Should().Match($"*_collation_x{total}.jpg");
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
}