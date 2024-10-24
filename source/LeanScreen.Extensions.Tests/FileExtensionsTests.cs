// <copyright file="FileExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Extensions.Tests;

using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.IO;
using LeanScreen.Common;
using LeanScreen.Rendering;

/// <summary>
/// Tests for the <see cref="LeanScreen.Extensions.FileExtensions"/> class.
/// </summary>
public class FileExtensionsTests
{
    [Fact]
    public void GetMediaInfo_NullInfo_ThrowsExpected()
    {
        // Arrange
        var fi = (FileInfo)null!;

        // Act
        var act = () => fi.GetMediaInfo([]);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("sample.flv")]
    [InlineData("c12a3419943d6ceb89c41ce7cd4fe1ff75b991cc3cb01a31a13b08693c5dc63d.e4e4742e63")]
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
    [InlineData("5e84bf533440.fb62352ee50d77e90b9d4c59f92263b576756148e1cee33b8ad338741b2af7b4.63e74026ac", 1650, 1980)]
    [InlineData("blue-pixel.png", 1, 1)]
    public async Task GetSize_WithImage_ReturnsExpected(string path, int expectedWidth, int expectedHeight)
    {
        // Arrange
        var fi = new FileInfo($"Samples/{path}");
        var expectedSize = new Size2D(expectedWidth, expectedHeight);

        // Act
        var actualSize = await fi.GetImageSize([9, 0, 2, 1, 0]);

        // Assert
        actualSize.Should().Be(expectedSize);
    }

    [Fact]
    public async Task ResizeImage_NotImage_ThrowsException()
    {
        // Arrange
        var fakeVid = new FileInfo("hi.wmv");

        // Act
        var act = () => fakeVid.ResizeImage([]);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Media type must be: Image*");
    }

    [Theory]
    [InlineData("blue-pixel.png", "4591d2fd73a570ff757fb3c9e55186a7")]
    [InlineData(
        "5e84bf533440.fb62352ee50d77e90b9d4c59f92263b576756148e1cee33b8ad338741b2af7b4.63e74026ac",
        "a338f1f9914186c1730f51dde4d09fd6")]
    public async Task ResizeImage_IsImage_ProducesExpectedHash(string name, string expectedHash)
    {
        // Arrange
        var imageSource = new FileInfo($"Samples/{name}");

        // Act
        await using var stream = await imageSource.ResizeImage([9, 0, 2, 1, 0]);
        var resultingPosition = stream.Position;
        var actual = stream.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        resultingPosition.Should().Be(0);
        actual.Should().Be(expectedHash);
    }

    [Theory]
    [InlineData("sample.flv", "efd6f4e5213878701053d32b3273214e")]
    public void SnapHere_WhenCalled_ProducesExpected(string sourceName, string expectedMd5)
    {
        // Arrange
        var source = new FileInfo(Path.Combine("Samples", sourceName));
        var fi = new FileInfo($"{source.DirectoryName}/{Guid.NewGuid()}{source.Extension}");
        source.CopyTo(fi.FullName);

        // Act
        var path = fi.SnapHere([9, 0, 2, 1, 0], out _);
        var snapFi = new FileInfo(path);
        var md5Hex = snapFi.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.Should().Be(expectedMd5);
    }

    [Theory]
    [InlineData("sample.flv", "sample.flv_snap_p055_h0024.jpg")]
    [InlineData(
        "c12a3419943d6ceb89c41ce7cd4fe1ff75b991cc3cb01a31a13b08693c5dc63d.e4e4742e63",
        "c12a3419943d.eafc926af0e776a633eb7f5bf3cba71db5664c1a598fa35638411d19cc8a92a6.e4e4783272")]
    public void SnapHere_WhenCalled_ReturnsExpectedPath(string sourceName, string expectedName)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", sourceName));

        // Act
        var actualName = new FileInfo(fi.SnapHere([9, 0, 2, 1, 0], out _, .55, 24)).Name;

        // Assert
        actualName.Should().Be(expectedName);
    }

    [Theory]
    [InlineData("sample.flv", "821c5e80ab5d1e2c9d0e83bbb242bfcd")]
    public void CollateHere_WhenCalled_ProducesExpected(string sourceName, string expectedMd5)
    {
        // Arrange
        var source = new FileInfo(Path.Combine("Samples", sourceName));
        var fi = new FileInfo($"{source.DirectoryName}/{Guid.NewGuid()}{source.Extension}");
        source.CopyTo(fi.FullName);

        // Act
        var path = fi.CollateHere([9, 0, 2, 1, 0], out _);
        var snapFi = new FileInfo(path);
        var md5Hex = snapFi.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.Should().Be(expectedMd5);
    }

    [Theory]
    [InlineData("sample.flv", "sample.flv_collate_t006_c02_h0200.jpg")]
    [InlineData("1.mkv", "1.mkv_collate_t006_c02_h0200.jpg")]
    [InlineData(
        "c12a3419943d6ceb89c41ce7cd4fe1ff75b991cc3cb01a31a13b08693c5dc63d.e4e4742e63",
        "c12a3419943d.e13f663d61480831c5269fc07825a9c418574828fba4ae66b0c6c87829fa3e7c.e4e4783272")]
    public void CollateHere_WhenCalled_ReturnsExpectedPath(string sourceName, string expectedName)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", sourceName));

        // Act
        var actualName = new FileInfo(fi.CollateHere([9, 0, 2, 1, 0], out _, 6, 2, 200)).Name;

        // Assert
        actualName.Should().Be(expectedName);
    }
}