// <copyright file="FileExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Tests;

using System.Text.RegularExpressions;
using Av.Common;
using Av.Tests.Samples;
using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.IO;

/// <summary>
/// Tests for the <see cref="FileExtensions"/>.
/// </summary>
public class FileExtensionsTests
{
    [Fact]
    public void EnumerateMedia_NullDirectory_ThrowsExpected()
    {
        // Arrange
        var fi = (DirectoryInfo)null!;

        // Act
        var act = () => fi.EnumerateMedia(MediaTypes.AnyMedia);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(MediaTypes.Audio, 0)]
    [InlineData(MediaTypes.Image, 2)]
    [InlineData(MediaTypes.Video, 1)]
    [InlineData(MediaTypes.AnyMedia, 3)]
    [InlineData(MediaTypes.NonMedia, 1)]
    public void EnumerateMedia_VaryingMediaType_CountExpected(MediaTypes types, int expectedCount)
    {
        // Arrange
        var di = TestHelper.CopySamples();

        // Act
        var media = di.EnumerateMedia(types, recurse: true);

        // Assert
        media.Count().Should().Be(expectedCount);
        di.Delete(true);
    }

    [Theory]
    [InlineData(null, 3)]
    [InlineData(false, 2)]
    [InlineData(true, 1)]
    public void EnumerateMedia_VaryingSecureFlag_CountExpected(bool? secure, int expectedCount)
    {
        // Arrange
        var di = TestHelper.CopySamples();

        // Act
        var media = di.EnumerateMedia(MediaTypes.AnyMedia, secure, true);

        // Assert
        media.Count().Should().Be(expectedCount);
        di.Delete(true);
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 3)]
    public void EnumerateMedia_VaryingRecurseFlag_CountExpected(bool recurse, int expectedCount)
    {
        // Arrange
        var di = TestHelper.CopySamples();

        // Act
        var media = di.EnumerateMedia(MediaTypes.AnyMedia, recurse: recurse);

        // Assert
        media.Count().Should().Be(expectedCount);
        di.Delete(true);
    }

    [Fact]
    public void MakeKey_BadKeyFolder_ThrowsException()
    {
        // Arrange
        var fakeDir = new DirectoryInfo(Guid.NewGuid().ToString());

        // Act
        var act = () => fakeDir.MakeKey(null, [], out _);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Directory not found*");
    }

    [Theory]
    [InlineData(null, "T3H8ysQ8c1Rd3ZzXcvN1mA==")]
    [InlineData("", "T3H8ysQ8c1Rd3ZzXcvN1mA==")]
    [InlineData("one,two,three", "cvN2WzfLE+6qnH8w8HyaoA==")]
    public void MakeKey_VaryingEntropy_OutputsExpectedChecksum(string? entropyCsv, string expectedSum)
    {
        // Arrange
        var entropy = entropyCsv?.Split(',');

        // Act
        Av.FileExtensions.MakeKey(null, null, entropy!, out var checkSum);

        // Assert
        checkSum.Should().Be(expectedSum);
    }

    [Theory]
    [InlineData("ogg", "f652b8fd0e9c447c9cb0cae3044a927e")]
    [InlineData(null, "e68a0f5696c34381362ccc1da69beb2f")]
    public void MakeKey_VaryingKeyRegex_ReturnsExpectedBytes(string? keyRegex, string expectedHash)
    {
        // Arrange
        var regex = keyRegex == null ? null : new Regex(keyRegex);
        var di = new DirectoryInfo("Samples/Subfolder");

        // Act
        var md5Hex = di.MakeKey(regex, [], out _).ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.Should().Be(expectedHash);
    }

    [Theory]
    [InlineData("Samples/blue-pixel.png", null, "Samples")]
    [InlineData("Samples/blue-pixel.png", "samplez22", "samplez22")]
    [InlineData("Samples/does-not-exist.png", null, null)]
    public void QualifyDestination_VaryingParameters_ReturnsExpected(string file, string? dest, string? expectedDir)
    {
        // Arrange
        expectedDir ??= new DirectoryInfo(Directory.GetCurrentDirectory()).Name;
        var fi = new FileInfo(file);

        // Act
        var actualDir = fi.QualifyDestination(dest).Name;

        // Assert
        actualDir.Should().Be(expectedDir);
    }
}
