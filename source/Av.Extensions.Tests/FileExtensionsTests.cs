// <copyright file="FileExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Extensions.Tests;

using Av.Rendering;
using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.IO;

/// <summary>
/// Tests for the <see cref="FileExtensions"/> class.
/// </summary>
public class FileExtensionsTests
{
    [Theory]
    [InlineData("sample.flv")]
    [InlineData("1bcedf85fab4eae955a6444ee7b2d70be3b5fe02bdebaecd433828f9731630da.flv")]
    public void GetMediaInfo_WhenCalled_ReturnsExpected(string sourceName)
    {
        // Arrange
        var expected = new MediaInfo(TimeSpan.FromSeconds(13.346), new(640, 360), 400, 29.9697);
        var fi = new FileInfo(Path.Combine("Samples", sourceName));

        // Act
        var info = fi.GetMediaInfo([9, 0, 2, 1, 0]);

        // Assert
        info.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("sample.flv", "efd6f4e5213878701053d32b3273214e")]
    public void SnapHere_WhenCalled_ProducesExpected(string sourceName, string expectedMd5)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", sourceName));

        // Act
        var path = fi.SnapHere([9, 0, 2, 1, 0]);
        var snapFi = new FileInfo(path);
        var md5Hex = snapFi.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.Should().Be(expectedMd5);
    }

    [Theory]
    [InlineData("sample.flv", "sample.flv_snap_p055_h0024.jpg")]
    [InlineData(
        "1bcedf85fab4eae955a6444ee7b2d70be3b5fe02bdebaecd433828f9731630da.flv",
        "1bcedf85fab4.1eb62785a1fdd615b52619ff6e957840a3a0e88ef9624e3210a5cf7f20fe8e25.jpg")]
    public void SnapHere_WhenCalled_ReturnsExpectedPath(string sourceName, string expectedName)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", sourceName));

        // Act
        var actualName = new FileInfo(fi.SnapHere([9, 0, 2, 1, 0], .55, 24)).Name;

        // Assert
        actualName.Should().Be(expectedName);
    }

    [Theory]
    [InlineData("sample.flv", "821c5e80ab5d1e2c9d0e83bbb242bfcd")]
    public void CollateHere_WhenCalled_ProducesExpected(string sourceName, string expectedMd5)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", sourceName));

        // Act
        var path = fi.CollateHere([9, 0, 2, 1, 0]);
        var snapFi = new FileInfo(path);
        var md5Hex = snapFi.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.Should().Be(expectedMd5);
    }

    [Theory]
    [InlineData("sample.flv", "sample.flv_collate_t006_c02_h0200.jpg")]
    [InlineData(
        "1bcedf85fab4eae955a6444ee7b2d70be3b5fe02bdebaecd433828f9731630da.flv",
        "1bcedf85fab4.87e1da6af4b93bb2b8f9c8033b48922ca11ef6384abd614f7c0a1bdbfc0104d0.jpg")]
    public void CollateHere_WhenCalled_ReturnsExpectedPath(string sourceName, string expectedName)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", sourceName));

        // Act
        var actualName = new FileInfo(fi.CollateHere([9, 0, 2, 1, 0], 6, 2, 200)).Name;

        // Assert
        actualName.Should().Be(expectedName);
    }
}