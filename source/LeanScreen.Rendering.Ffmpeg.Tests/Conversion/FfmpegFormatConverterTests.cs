// <copyright file="FfmpegFormatConverterTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Tests.Conversion;

using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.IO;
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
            "C:\\temp\\~vids\\e6d787f97a7b620cd438e75e3e5bdba384fe7df43605fad51eabb758fb347de5.1e99a40d6e");
            ////"C:\\temp\\~vids\\edffa6fa556940cf3441b771c2cf62619b7207ada90ab54ff154598ad5705464.dc80231635");

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
            "C:\\temp\\~vids\\out\\e6d787f97a7b620cd438e75e3e5bdba384fe7df43605fad51eabb758fb347de5.1e99a80b33");
            ////"C:\\temp\\~vids\\out\\edffa6fa556940cf3441b771c2cf62619b7207ada90ab54ff154598ad5705464.dc802f1068");
        src.DecryptHere(key);
    }

    [Theory]
    [InlineData("1.avi.mp4")]
    [InlineData("2.avi.mp4")]
    public void HashTest(string suffix)
    {
        // Arrange
        var controlRefFi = new FileInfo($"C:\\temp\\~vids\\out\\CONTROL__{suffix}");
        var testingRefFi = new FileInfo($"C:\\temp\\~vids\\out\\TESTING__{suffix}");

        var controlHash = controlRefFi.Hash(HashType.Md5).Encode(Codec.ByteHex);
        var testingHash = testingRefFi.Hash(HashType.Md5).Encode(Codec.ByteHex);

        ////testingHash.Should().Be(controlHash);

        using var controlFi = controlRefFi.OpenRead();
        using var testingFi = testingRefFi.OpenRead();

        var matchingBlocks = 0;
        var buffer = new byte[32768];
        var testBlocks = (int)Math.Ceiling((double)testingRefFi.Length / buffer.Length);
        var badBoys = new List<int>();
        var b1ControlBytes = "";
        var b1TestingBytes = "";

        for (var blockNo = 1; blockNo <= testBlocks; blockNo++)
        {
            Array.Clear(buffer);
            var read = controlFi.Read(buffer, 0, buffer.Length);
            var ctrl1 = buffer.AsSpan(0, read).ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex);
            if (blockNo == testBlocks) b1ControlBytes = string.Join("\r\n", buffer);

            Array.Clear(buffer);
            read = testingFi.Read(buffer, 0, buffer.Length);
            var test = buffer.AsSpan(0, read).ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex);
            if (blockNo == testBlocks) b1TestingBytes = string.Join("\r\n", buffer);

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
