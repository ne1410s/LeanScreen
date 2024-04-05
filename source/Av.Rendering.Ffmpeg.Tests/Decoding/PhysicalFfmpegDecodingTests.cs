// <copyright file="PhysicalFfmpegDecodingTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering.Ffmpeg.Tests.Decoding;

using Av.Rendering.Ffmpeg.Decoding;
using FFmpeg.AutoGen;

/// <summary>
/// Tests for the <see cref="PhysicalFfmpegDecoding"/>.
/// </summary>
public class PhysicalFfmpegDecodingTests
{
    [Fact]
    public void Ctor_WithNetworkStream_SetsBinariesPath()
    {
        // Arrange
        const string source = "https://download.samplelib.com/mp4/sample-5s.mp4";
        ffmpeg.RootPath = null;

        // Act
        var x = new PhysicalFfmpegDecoding(source);

        // Assert
        ffmpeg.RootPath.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Ctor_WithNetworkStreamAndVerboseLevel_ReceivesVerboseMessage()
    {
        // Arrange
        const string source = "https://download.samplelib.com/mp4/sample-5s.mp4";
        using StringWriter writer = new();
        Console.SetOut(writer);

        // Act
        FfmpegUtils.LogLevel = ffmpeg.AV_LOG_VERBOSE;
        FfmpegUtils.Logger = (_, msg) => Console.WriteLine(msg);
        var x = new PhysicalFfmpegDecoding(source);

        // Assert
        writer.ToString().Should().Contain("Starting connection attempt");
    }

    [Fact]
    public void Ctor_WithNetworkStreamAndWarningLevel_DoesNotReceiveVerboseMessage()
    {
        // Arrange
        const string source = "https://download.samplelib.com/mp4/sample-5s.mp4";
        using StringWriter writer = new();
        Console.SetOut(writer);

        // Act
        FfmpegUtils.LogLevel = ffmpeg.AV_LOG_WARNING;
        FfmpegUtils.Logger = (_, msg) => Console.WriteLine(msg);
        var x = new PhysicalFfmpegDecoding(source);

        // Assert
        writer.ToString().Should().NotContain("Starting connection attempt");
    }

    [Fact]
    public void Ctor_WithNetworkStreamAndVerboseLevelButNullLogger_DoesNotReceiveVerboseMessage()
    {
        // Arrange
        const string source = "https://download.samplelib.com/mp4/sample-5s.mp4";
        using StringWriter writer = new();
        Console.SetOut(writer);

        // Act
        FfmpegUtils.LogLevel = ffmpeg.AV_LOG_VERBOSE;
        FfmpegUtils.Logger = null;
        var x = new PhysicalFfmpegDecoding(source);

        // Assert
        writer.ToString().Should().NotContain("Starting connection attempt");
    }
}
