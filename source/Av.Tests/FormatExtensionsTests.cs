// <copyright file="FormatExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using System.Globalization;
using Av.Models;

namespace Av.Tests;

/// <summary>
/// Tests for the <see cref="FormatExtensions"/>.
/// </summary>
public class FormatExtensionsTests
{
    [Theory]
    [InlineData("1:0:0:0", "1.00:00:00")]
    [InlineData("1:1:1:1.2", "1.01:01:01")]
    [InlineData("1:0:0", "1:00:00")]
    [InlineData("1:1:1.2", "1:01:01")]
    [InlineData("0:1:0", "1:00")]
    [InlineData("0:1:1.2", "1:01")]
    [InlineData("0:0:1", "1.0s")]
    [InlineData("0:0:1.2", "1.2s")]
    public void FormatConcise_VaryingInput_ReturnsExpected(string input, string expected)
    {
        // Arrange
        var timeSpan = TimeSpan.Parse(input, CultureInfo.InvariantCulture);

        // Act
        var actual = timeSpan.FormatConcise();

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(1L, "1b")]
    [InlineData(1L * 1024, "1kb")]
    [InlineData(1L * 1024 * 1024, "1mb")]
    [InlineData(1L * 1024 * 1024 * 1024, "1gb")]
    [InlineData(1L * 1024 * 1024 * 1024 * 1024, "1tb")]
    public void FormatSize_VaryingSize_ReturnsExpected(long bytes, string expected)
    {
        // Arrange & Act
        var result = bytes.FormatSize();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(".7z", MediaTypes.Archive, "application/x-7z-compressed")]
    [InlineData(".gz", MediaTypes.Archive, "application/gzip")]
    [InlineData(".jar", MediaTypes.Archive, "application/java-archive")]
    [InlineData(".rar", MediaTypes.Archive, "application/vnd.rar")]
    [InlineData(".tar", MediaTypes.Archive, "application/x-tar")]
    [InlineData(".zip", MediaTypes.Archive, "application/zip")]
    [InlineData(".bmp", MediaTypes.Image, "image/bmp")]
    [InlineData(".gif", MediaTypes.Image, "image/gif")]
    [InlineData(".jpe", MediaTypes.Image, "image/jpeg")]
    [InlineData(".jpeg", MediaTypes.Image, "image/jpeg")]
    [InlineData(".jpg", MediaTypes.Image, "image/jpeg")]
    [InlineData(".png", MediaTypes.Image, "image/png")]
    [InlineData(".tif", MediaTypes.Image, "image/tiff")]
    [InlineData(".tiff", MediaTypes.Image, "image/tiff")]
    [InlineData(".webp", MediaTypes.Image, "image/webp")]
    [InlineData(".aac", MediaTypes.Audio, "audio/aac")]
    [InlineData(".m4a", MediaTypes.Audio, "audio/m4a")]
    [InlineData(".mp3", MediaTypes.Audio, "audio/mpeg")]
    [InlineData(".oga", MediaTypes.Audio, "audio/ogg")]
    [InlineData(".wav", MediaTypes.Audio, "audio/wav")]
    [InlineData(".weba", MediaTypes.Audio, "audio/webm")]
    [InlineData(".3g2", MediaTypes.Video, "video/3gpp2")]
    [InlineData(".3gp", MediaTypes.Video, "video/3gpp")]
    [InlineData(".avi", MediaTypes.Video, "video/x-msvideo")]
    [InlineData(".flv", MediaTypes.Video, "video/x-flv")]
    [InlineData(".m2v", MediaTypes.Video, "video/mpeg")]
    [InlineData(".m4v", MediaTypes.Video, "video/x-m4v")]
    [InlineData(".mkv", MediaTypes.Video, "video/x-matroska")]
    [InlineData(".mov", MediaTypes.Video, "video/quicktime")]
    [InlineData(".mp4", MediaTypes.Video, "video/mp4")]
    [InlineData(".mpeg", MediaTypes.Video, "video/mpeg")]
    [InlineData(".mpg", MediaTypes.Video, "video/mpeg")]
    [InlineData(".mts", MediaTypes.Video, "video/mp2t")]
    [InlineData(".swf", MediaTypes.Video, "application/x-shockwave-flash")]
    [InlineData(".ts", MediaTypes.Video, "video/mp2t")]
    [InlineData(".ogg", MediaTypes.Video, "video/ogg")]
    [InlineData(".ogv", MediaTypes.Video, "video/ogg")]
    [InlineData(".vob", MediaTypes.Video, "video/x-ms-vob")]
    [InlineData(".webm", MediaTypes.Video, "video/webm")]
    [InlineData(".wmv", MediaTypes.Video, "video/x-ms-wmv")]
    public void GetMediaTypeInfo_ValidExtension_ReturnsInfo(
        string extension, MediaTypes expectedType, string expectedMime)
    {
        // Arrange
        var expected = new MediaTypeInfo(expectedType, expectedMime);

        // Act
        var result = extension.GetMediaTypeInfo();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(".zip")]
    [InlineData("ZIp")]
    public void GetMediaTypeInfo_VarietyOfFormats_ReturnsInfo(string extension)
    {
        // Arrange & Act
        var result = extension.GetMediaTypeInfo();

        // Assert
        result.MediaType.Should().Be(MediaTypes.Archive);
        result.MimeType.Should().Be("application/zip");
    }

    [Theory]
    [InlineData(".")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("fakerzz")]
    [InlineData("zip.")]
    public void GetMediaTypeInfo_NotFound_ReturnsNull(string extension)
    {
        // Arrange
        var result = extension.GetMediaTypeInfo();

        // Assert
        result.MimeType.Should().Be(null);
    }

    [Theory]
    [InlineData(MediaTypes.NonMedia, 0)]
    [InlineData(MediaTypes.Archive, 6)]
    [InlineData(MediaTypes.Image, 9)]
    [InlineData(MediaTypes.Audio, 6)]
    [InlineData(MediaTypes.Video, 19)]
    [InlineData(MediaTypes.Visible, 28)]
    [InlineData(MediaTypes.AnyMedia, 34)]
    [InlineData(MediaTypes.Anything, 0)]
    public void GetExtensions_VaryingType_GetsExpectedCount(MediaTypes mediaType, int expectedCount)
    {
        // Arrange & Act
        var extensions = mediaType.GetExtensions();

        // Assert
        extensions.Count.Should().Be(expectedCount);
    }

    [Fact]
    public void GetUpperBoundFormat_WithValue_ReturnsExpected()
    {
        // Arrange
        const int input = 100000;

        // Act
        var result = input.GetUpperBoundFormat();

        // Assert
        result.Should().Be("D6");
    }
}
