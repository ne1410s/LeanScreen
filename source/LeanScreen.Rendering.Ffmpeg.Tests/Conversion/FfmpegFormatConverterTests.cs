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
        const string path = "C:\\temp\\~vids\\3.avi";

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
        var fi = new FileInfo("C:\\temp\\~vids\\3.avi");
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
        using var fsRead = File.OpenRead("C:\\temp\\~vids\\3.avi");
        using var fsWrite = File.Open("C:\\temp\\~vids\\out\\3.avi" + "_333" + ext, FileMode.Create);

        // Act
        var result = new FfmpegFormatConverter_003_Str2Str().Remux(fsRead, fsWrite, ext, [], []);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(TargetExts.Asf)]
    [InlineData(TargetExts.Flv)]
    [InlineData(TargetExts.Mkv)]
    [InlineData(TargetExts.Mov)]
    [InlineData(TargetExts.Mp4)]
    [InlineData(TargetExts.Ts)]
    [InlineData(TargetExts.Vob)]
    public void RemuxV2VPlain_WhenCalled_ProducesExpected(string ext)
    {
        // Arrange
        var source = new FileInfo("C:\\temp\\~vids\\3.avi");

        // Act
        var result = FfmpegFormatConverter.Remux(source, ext, []);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    //[InlineData(TargetExts.Asf)]
    [InlineData(TargetExts.Mkv)]
    //[InlineData(TargetExts.Mov)]
    //[InlineData(TargetExts.Mp4)]
    //[InlineData(TargetExts.Ts)]
    //[InlineData(TargetExts.Vob)]
    public void HashTest(string ext)
    {
        // Arrange
        var controlRefFi = new FileInfo($"C:\\temp\\~vids\\out\\F2F_CONTROL{ext}");
        var testingRefFi = new FileInfo($"C:\\temp\\~vids\\out\\B2B_TEST{ext}");

        var controlHash = controlRefFi.Hash(HashType.Md5).Encode(Codec.ByteHex);
        var testingHash = testingRefFi.Hash(HashType.Md5).Encode(Codec.ByteHex);

        ////testingHash.Should().Be(controlHash);

        using var controlFi1 = controlRefFi.OpenRead();
        using var testFi = testingRefFi.OpenRead();

        var matchingBlocks = 0;
        var buffer = new byte[32768];
        var testBlocks = (int)Math.Ceiling((double)testingRefFi.Length / buffer.Length);
        var badBoys = new List<int>();
        for (var block = 0; block < testBlocks; block++)
        {
            Array.Clear(buffer);
            controlFi1.Read(buffer, 0, buffer.Length);
            var ctrl1 = buffer.Hash(HashType.Md5).Encode(Codec.ByteHex);

            Array.Clear(buffer);
            testFi.Read(buffer, 0, buffer.Length);
            var test = buffer.Hash(HashType.Md5).Encode(Codec.ByteHex);

            if (ctrl1 == test)
            {
                matchingBlocks++;
            }
            else
            {
                badBoys.Add(block);
            }
        }

        matchingBlocks.Should().Be(testBlocks);
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
