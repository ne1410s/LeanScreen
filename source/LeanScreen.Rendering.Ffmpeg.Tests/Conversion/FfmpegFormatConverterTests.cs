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
    //[InlineData(TargetExts.Asf)]
    //[InlineData(TargetExts.Flv)]
    //[InlineData(TargetExts.Mkv)]
    //[InlineData(TargetExts.Mov)]
    [InlineData(TargetExts.Mp4)]
    //[InlineData(TargetExts.Ts)]
    //[InlineData(TargetExts.Vob)]
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
        using var fsRead = File.OpenRead("C:\\temp\\~vids\\1.avi");
        using var fsWrite = File.Open("C:\\temp\\~vids\\out\\1.avi" + "_333" + ext, FileMode.Create);

        // Act
        var result = new FfmpegFormatConverter_003_Str2Str().Remux(fsRead, fsWrite, ext, [], []);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    ////[InlineData(TargetExts.Asf, false, false)]
    ////[InlineData(TargetExts.Asf, false, true)]
    ////[InlineData(TargetExts.Flv, false, false)]
    ////[InlineData(TargetExts.Flv, false, true)]
    ////[InlineData(TargetExts.Mkv, false, false)]
    ////[InlineData(TargetExts.Mkv, false, true)]
    ////[InlineData(TargetExts.Mov, false, false)]
    ////[InlineData(TargetExts.Mov, false, true)]
    [InlineData(TargetExts.Mp4, false, false)]
    [InlineData(TargetExts.Mp4, false, true)]
    [InlineData(TargetExts.Mp4, true, false)]
    [InlineData(TargetExts.Mp4, true, true)]
    ////[InlineData(TargetExts.Ts, false, false)]
    ////[InlineData(TargetExts.Ts, false, true)]
    ////[InlineData(TargetExts.Vob, false, false)]
    ////[InlineData(TargetExts.Vob, false, true)]
    public void RemuxV2VPlain_WhenCalled_ProducesExpected(string ext, bool fsIn, bool fsOut)
    {
        // Arrange
        var source = new FileInfo("C:\\temp\\~vids\\fffff1.avi");

        // Act
        var result = new FfmpegFormatConverter().Remux(source, ext, [], fsIn, fsOut);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void RemuxS2S_Diagnose()
    {
        // Arrange
        var controlRefFi1 = new FileInfo("C:\\temp\\~vids\\out\\1.avi__TOTALFF2fs.mp4");
        var controlRefFi2 = new FileInfo("C:\\temp\\~vids\\out\\1.avi__TOTALFF2bs.mp4");
        var testRefFi = new FileInfo("hi");

        var cHash1 = controlRefFi1.Hash(HashType.Md5).Encode(Codec.ByteHex);
        var cHash2 = controlRefFi2.Hash(HashType.Md5).Encode(Codec.ByteHex);
        //var tHash = testRefFi.Hash(HashType.Md5).Encode(Codec.ByteHex);

        using var controlFi1 = controlRefFi1.OpenRead();
        using var testFi = testRefFi.OpenRead();

        var matchingBlocks = 0;
        var buffer = new byte[36];
        var testBlocks = (int)Math.Ceiling((double)testRefFi.Length / buffer.Length);
        for (var block = 0; block < 2; block++)
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
