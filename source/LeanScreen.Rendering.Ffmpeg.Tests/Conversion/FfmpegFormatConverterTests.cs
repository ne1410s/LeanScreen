// <copyright file="FfmpegFormatConverterTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Tests.Conversion;

using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.IO;
using CryptoStream.Streams;
using CryptoStream.Transform;
using LeanScreen.Rendering.Ffmpeg.Conversion;

/// <summary>
/// Tests for the <see cref="FfmpegFormatConverter"/> class.
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
    ////[InlineData(TargetExts.Asf)]
    ////[InlineData(TargetExts.Flv)]
    ////[InlineData(TargetExts.Mkv)]
    ////[InlineData(TargetExts.Mov)]
    [InlineData(TargetExts.Mp4)]
    ////[InlineData(TargetExts.Ts)]
    ////[InlineData(TargetExts.Vob)]
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
    ////[InlineData(TargetExts.Asf)]
    ////[InlineData(TargetExts.Flv)]
    ////[InlineData(TargetExts.Mkv)]
    ////[InlineData(TargetExts.Mov)]
    [InlineData(TargetExts.Mp4)]
    ////[InlineData(TargetExts.Ts)]
    ////[InlineData(TargetExts.Vob)]
    public void RemuxCB2CB_WhenCalled_ProducesExpected(string ext)
    {
        // Arrange
        var key = new byte[] { 9, 0, 2, 1, 0 };
        var source = new FileInfo(
            "C:\\temp\\~vids\\fc0526019655151aadeece71ae623a9f7d445498f0a88c6873dc3710a0e91ef8.29366df843");

        // Act
        var result = FfmpegFormatConverter.Remux(source, ext, key);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void DecTest()
    {
        var key = new byte[] { 9, 0, 2, 1, 0 };
        var src = new FileInfo(
            "C:\\temp\\~vids\\out\\fc0526019655151aadeece71ae623a9f7d445498f0a88c6873dc3710a0e91ef8.293661fe1e");
        src.DecryptHere(key);
    }

    [Theory]
    //[InlineData(TargetExts.Asf)]
    //[InlineData(TargetExts.Mkv)]
    //[InlineData(TargetExts.Mov)]
    [InlineData(TargetExts.Mp4)]
    //[InlineData(TargetExts.Ts)]
    //[InlineData(TargetExts.Vob)]
    public void HashTest(string ext)
    {
        // Arrange
        var controlRefFi = new FileInfo($"C:\\temp\\~vids\\out\\CONTROL{ext}");
        var testingRefFi = new FileInfo($"C:\\temp\\~vids\\out\\TEST{ext}");

        var controlHash = controlRefFi.Hash(HashType.Md5).Encode(Codec.ByteHex);
        var testingHash = testingRefFi.Hash(HashType.Md5).Encode(Codec.ByteHex);

        ////testingHash.Should().Be(controlHash);

        using var controlFi1 = controlRefFi.OpenRead();
        using var testFi = testingRefFi.OpenRead();

        var matchingBlocks = 0;
        var buffer = new byte[32768];
        var testBlocks = (int)Math.Ceiling((double)testingRefFi.Length / buffer.Length);
        var badBoys = new List<int>();
        var b1ControlBytes = "";
        var b1TestingBytes = "";

        for (var blockNo = 1; blockNo <= testBlocks; blockNo++)
        {
            Array.Clear(buffer);
            controlFi1.Read(buffer, 0, buffer.Length);
            var ctrl1 = buffer.Hash(HashType.Md5).Encode(Codec.ByteHex);
            if (blockNo == 1) b1ControlBytes = string.Join("\r\n", buffer);

            Array.Clear(buffer);
            testFi.Read(buffer, 0, buffer.Length);
            var test = buffer.Hash(HashType.Md5).Encode(Codec.ByteHex);
            if (blockNo == 1) b1TestingBytes = string.Join("\r\n", buffer);

            if (ctrl1 == test)
            {
                matchingBlocks++;
            }
            else
            {
                badBoys.Add(blockNo);
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
