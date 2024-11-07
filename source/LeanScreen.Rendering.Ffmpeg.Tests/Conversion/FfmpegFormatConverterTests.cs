// <copyright file="FfmpegFormatConverterTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Tests.Conversion;

using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.IO;
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
        using var srs = fi.OpenBlockRead();

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
        using var fsRead = File.OpenRead("C:\\temp\\~vids\\2.avi");
        using var fsWrite = File.Open("C:\\temp\\~vids\\out\\2.avi" + "_333" + ext, FileMode.Create);

        // Act
        var result = new FfmpegFormatConverter_003_Str2Str().Remux(fsRead, fsWrite, ext, [], []);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void RemuxS2S_Diagnose()
    {
        // Arrange
        var controlRefFi = new FileInfo("C:\\temp\\~vids\\out\\111_demo.mp4");
        var testRefFi = new FileInfo("C:\\temp\\~vids\\out\\1.avi_333.mp4");

        using var controlFi = controlRefFi.OpenRead();
        using var testFi = testRefFi.OpenRead();

        var matchingBlocks = 0;
        var buffer = new byte[32768];
        var testBlocks = (int)Math.Ceiling((double)testRefFi.Length / buffer.Length);
        for (var block = 0; block < testBlocks; block++)
        {
            Array.Clear(buffer);
            controlFi.Read(buffer, 0, buffer.Length);
            var ctrl = buffer.Hash(HashType.Md5).Encode(Codec.ByteHex);

            Array.Clear(buffer);
            testFi.Read(buffer, 0, buffer.Length);
            var test = buffer.Hash(HashType.Md5).Encode(Codec.ByteHex);

            if (ctrl == test)
            {
                matchingBlocks++;
            }
        }
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
