// <copyright file="FfmpegFormatConverterTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Tests.Conversion;

using LeanScreen.Rendering.Ffmpeg.Conversion;

/// <summary>
/// Tests for <see cref="FfmpegFormatConverter"/> class.
/// </summary>
public class FfmpegFormatConverterTests
{
    [Fact]
    public void Remux_WhenCalled_ProducesExpected()
    {
        // Arrange
        const string path = "C:\\temp\\~vids\\2.mp4";

        // Act
        var result = FfmpegFormatConverter_Demo.Remux(path);

        // Assert
        result.Should().Be(0);
    }
}
