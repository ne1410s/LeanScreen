// <copyright file="FfmpegFormatConverterTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Tests.Conversion;

using CryptoStream;
using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.IO;
using LeanScreen.Rendering.Ffmpeg.Conversion;

/// <summary>
/// Tests for the <see cref="FfmpegFormatConverter"/> class.
/// </summary>
public class FfmpegFormatConverterTests
{
    [Fact]
    public void Remux_NullSource_ThrowsException()
    {
        // Arrange
        var source = (FileInfo)null!;

        // Act
        var act = () => FfmpegFormatConverter.Remux(source, ".avi", []);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(source));
    }

    [Fact]
    public void Remux_DirectModeOnSecure_ThrowsException()
    {
        // Arrange
        const string secureName = "2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881";
        const string secureExt = ".0123456789";
        var secureFi = new FileInfo(secureName + secureExt);

        // Act
        var act = () => FfmpegFormatConverter.Remux(secureFi, ".avi", [], directFile: true);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Direct file mode not supported for secure sources*")
            .WithParameterName("directFile");
    }

    [Fact]
    public void Remux_PlainFile_ProducesExpected()
    {
        // Arrange
        var source = new FileInfo(Path.Combine("Samples", "sample.flv"));

        // Act
        var target = FfmpegFormatConverter.Remux(source, ".mov", []);
        var resultHash = target.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        resultHash.Should().Be("018c976a75c5fd24d38ff7cec60c394a");
    }

    [Fact]
    public void Remux_NoAudio_ProducesExpected()
    {
        // Arrange
        var source = new FileInfo(Path.Combine("Samples", "no-audio.mp4"));

        // Act
        var target = FfmpegFormatConverter.Remux(source, ".mov", []);
        var resultHash = target.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        resultHash.Should().Be("bca83d7903c7eaf44cf405c8f5518724");
    }

    [Fact]
    public void Remux_BadTargetExt_ThrowsExpected()
    {
        // Arrange
        var source = new FileInfo(Path.Combine("Samples", "sample.mkv"));

        // Act
        var act = () => FfmpegFormatConverter.Remux(source, ".ogg", []);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("sample.avi", TargetExts.Asf)]
    [InlineData("sample.avi", TargetExts.Mov)]
    [InlineData("sample.avi", TargetExts.Mp4)]
    [InlineData("sample.flv", TargetExts.Asf)]
    [InlineData("sample.flv", TargetExts.Mov)]
    [InlineData("sample.flv", TargetExts.Ts)]
    [InlineData("sample.flv", TargetExts.Vob)]
    [InlineData("sample.mkv", TargetExts.Asf)]
    [InlineData("sample.mkv", TargetExts.Flv)]
    [InlineData("sample.mkv", TargetExts.Mov)]
    [InlineData("sample.mkv", TargetExts.Mp4)]
    [InlineData("sample.mkv", TargetExts.Ts)]
    [InlineData("sample.mkv", TargetExts.Vob)]
    [InlineData("sample.mp4", TargetExts.Asf)]
    [InlineData("sample.mp4", TargetExts.Flv)]
    [InlineData("sample.mp4", TargetExts.Mov)]
    [InlineData("sample.mp4", TargetExts.Ts)]
    [InlineData("sample.mp4", TargetExts.Vob)]
    public void RemuxCryptoE2E_VaryingFile_MatchesDirect(string fileName, string ext)
    {
        // Obtain a control conversion by remuxing plain source via "direct mode"
        var di = Directory.CreateDirectory($"{new FileInfo(fileName).Extension}2{ext}--{Guid.NewGuid()}");
        var srcFi = new FileInfo(Path.Combine(di.FullName, fileName));
        File.Copy(Path.Combine("Samples", fileName), srcFi.FullName);
        var controlFi = FfmpegFormatConverter.Remux(srcFi, ext, [], true);

        // Churn the plain source before obtaining a gcm conversion
        var key = new byte[] { 9, 0, 2, 1, 0 };
        srcFi.EncryptInSitu(key);
        var cryptFi = FfmpegFormatConverter.Remux(srcFi, ext, key);
        var testingFi = cryptFi.DecryptHere(key);
        testingFi.MoveTo(Path.Combine(di.FullName, $"{fileName}__B2B-rt{ext}"));
        srcFi.Delete();
        cryptFi.Delete();

        // Assert
        var controlHash = controlFi.Hash(HashType.Md5).Encode(Codec.ByteHex);
        var testingHash = testingFi.Hash(HashType.Md5).Encode(Codec.ByteHex);

        testingHash.Should().Be(controlHash);
        di.Delete(true);

        ////using var controlFs = controlFi.OpenRead();
        ////using var testingFs = testingFi.OpenRead();

        ////var matchingBlocks = 0;
        ////var buffer = new byte[32768];
        ////var testBlocks = (int)Math.Ceiling((double)testingFi.Length / buffer.Length);
        ////var badBoys = new List<int>();
        ////var b1ControlBytes = "";
        ////var b1TestingBytes = "";

        ////for (var blockNo = 1; blockNo <= testBlocks; blockNo++)
        ////{
        ////    Array.Clear(buffer);
        ////    var read = controlFs.Read(buffer, 0, buffer.Length);
        ////    var ctrl1 = buffer.AsSpan(0, read).ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex);
        ////    if (blockNo == 1) b1ControlBytes = string.Join("\r\n", buffer);

        ////    Array.Clear(buffer);
        ////    read = testingFs.Read(buffer, 0, buffer.Length);
        ////    var test = buffer.AsSpan(0, read).ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex);
        ////    if (blockNo == 1) b1TestingBytes = string.Join("\r\n", buffer);

        ////    if (ctrl1 == test)
        ////    {
        ////        matchingBlocks++;
        ////    }
        ////    else
        ////    {
        ////        badBoys.Add(blockNo);
        ////    }
        ////}

        ////matchingBlocks.Should().Be(testBlocks);
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
