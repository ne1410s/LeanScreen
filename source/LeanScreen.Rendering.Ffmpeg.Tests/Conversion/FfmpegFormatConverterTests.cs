// <copyright file="FfmpegFormatConverterTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Tests.Conversion;

using CryptoStream.Streams;
using LeanScreen.Rendering.Ffmpeg.Conversion;

/// <summary>
/// Tests for <see cref="FfmpegFormatConverter_002_Str2File"/> class.
/// </summary>
public class FfmpegFormatConverterTests
{
    [Theory]
    [InlineData(TargetExts.Asf)]
    [InlineData(TargetExts.Flv)]
    [InlineData(TargetExts.Mkv)]
    [InlineData(TargetExts.Mov)]
    [InlineData(TargetExts.Mp4)]
    [InlineData(TargetExts.Ts)]
    [InlineData(TargetExts.Vob)]
    public void RemuxF2F_WhenCalled_ProducesExpected(string ext)
    {
        // Arrange
        const string path = "C:\\temp\\~vids\\1.avi";

        // Act
        var result = FfmpegFormatConverter_001_File2File.Remux(path, ext);

        // Assert
        result.Should().Be(0);
    }

    [Theory]
    [InlineData(TargetExts.Asf)]
    [InlineData(TargetExts.Flv)]
    [InlineData(TargetExts.Mkv)]
    [InlineData(TargetExts.Mov)]
    [InlineData(TargetExts.Mp4)]
    [InlineData(TargetExts.Ts)]
    [InlineData(TargetExts.Vob)]
    public void RemuxS2F_WhenCalled_ProducesExpected(string ext)
    {
        // Arrange
        var fi = new FileInfo("C:\\temp\\~vids\\1.avi");
        using var srs = new BlockReadStream(fi);

        // Act
        var result = new FfmpegFormatConverter_002_Str2File().Remux(srs, ext);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(TargetExts.Asf)]
    [InlineData(TargetExts.Flv)]
    [InlineData(TargetExts.Mkv)]
    [InlineData(TargetExts.Mov)]
    [InlineData(TargetExts.Mp4)]
    [InlineData(TargetExts.Ts)]
    [InlineData(TargetExts.Vob)]
    public void RemuxS2S_WhenCalled_ProducesExpected(string ext)
    {
        // Arrange
        using var fsRead = File.OpenRead("C:\\temp\\~vids\\1.avi");
        using var fsWrite = File.Open("C:\\temp\\~vids\\out\\1.avi" + "_333" + ext, FileMode.Create);

        // Act
        var result = new FfmpegFormatConverter_003_Str2Str().Remux(fsRead, fsWrite, ext, [], []);

        // Assert
        result.Should().NotBeNull();
    }
}

public static class TargetExts
{
    public const string Asf = ".asf";

    public const string Flv = ".flv";

    public const string Mkv = ".mkv";

    public const string Mov = ".mov";

    public const string Mp4 = ".mp4";

    public const string Ts = ".ts";

    public const string Vob = ".vob";
}
