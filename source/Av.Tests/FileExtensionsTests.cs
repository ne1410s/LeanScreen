// <copyright file="FileExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using Av.Models;

namespace Av.Tests;

/// <summary>
/// Tests for the <see cref="FileExtensions"/>.
/// </summary>
public class FileExtensionsTests
{
    [Fact]
    public void GetMediaTypeInfo_WithJpg_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo("test.jpg");
        var expected = new MediaTypeInfo(MediaTypes.Image, "image/jpeg");

        // Act
        var result = fi.GetMediaTypeInfo();

        // Assert
        result.Should().Be(expected);
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
        var di = new DirectoryInfo("Samples");

        // Act
        var media = di.EnumerateMedia(types);

        // Assert
        media.Count().Should().Be(expectedCount);
    }

    [Theory]
    [InlineData(null, 3)]
    [InlineData(false, 2)]
    [InlineData(true, 1)]
    public void EnumerateMedia_VaryingSecureFlag_CountExpected(bool? secure, int expectedCount)
    {
        // Arrange
        var di = new DirectoryInfo("Samples");

        // Act
        var media = di.EnumerateMedia(MediaTypes.AnyMedia, secure);

        // Assert
        media.Count().Should().Be(expectedCount);
    }
}
