// <copyright file="PhysicalFfmpegDecodingTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using Av.Rendering.Ffmpeg.Decoding;
using FFmpeg.AutoGen;

namespace Av.Rendering.Ffmpeg.Tests.Decoding;

/// <summary>
/// Tests for the <see cref="PhysicalFfmpegDecoding"/>.
/// </summary>
public class PhysicalFfmpegDecodingTests
{
    [Fact]
    public void Ctor_WithNetworkStream_SetsBinariesPath()
    {
        // Arrange
        const string source = "http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4";

        // Act
        _ = new PhysicalFfmpegDecoding(source);

        // Assert
        ffmpeg.RootPath.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Ctor_WithNetworkStream_CallsLogger()
    {
        // Arrange
        const string source = "http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4";
        StringWriter writer = new();
        Console.SetOut(writer);

        // Act
        _ = new PhysicalFfmpegDecoding(source);

        // Assert
        writer.ToString().Should().Contain("Starting connection attempt");
    }
}
