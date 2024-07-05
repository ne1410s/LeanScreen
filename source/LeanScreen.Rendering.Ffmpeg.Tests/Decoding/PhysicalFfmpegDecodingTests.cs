// <copyright file="PhysicalFfmpegDecodingTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Tests.Decoding;

using FFmpeg.AutoGen;
using LeanScreen.Rendering.Ffmpeg.Decoding;

/// <summary>
/// Tests for the <see cref="PhysicalFfmpegDecoding"/>.
/// </summary>
public class PhysicalFfmpegDecodingTests
{
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
        using var x = new PhysicalFfmpegDecoding(source);

        // Assert
        writer.ToString().Should().Contain("Starting connection attempt");

        // Reset
        FfmpegUtils.Logger = null;
        FfmpegUtils.LogLevel = ffmpeg.AV_LOG_WARNING;
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
        using var x = new PhysicalFfmpegDecoding(source);

        // Assert
        writer.ToString().Should().NotContain("Starting connection attempt");

        // Reset
        FfmpegUtils.Logger = null;
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
        using var x = new PhysicalFfmpegDecoding(source);

        // Assert
        writer.ToString().Should().NotContain("Starting connection attempt");

        // Reset
        FfmpegUtils.LogLevel = ffmpeg.AV_LOG_WARNING;
    }
}
