// <copyright file="FFmpegRendererTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Tests;

using System.Globalization;
using System.Reflection;
using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.IO;
using LeanScreen.Rendering.Ffmpeg.Decoding;

/// <summary>
/// Tests for the <see cref="FfmpegRenderer"/>.
/// </summary>
public class FfmpegRendererTests
{
    internal enum DecodeMode
    {
        /// <summary>Cryto mode.</summary>
        Crypto,

        /// <summary>Physical source mode.</summary>
        PhysicalFm,

        /// <summary>Simple file mode.</summary>
        SimpleFile,

        /// <summary>Block read mode.</summary>
        BlockReads,
    }

    [Theory]
    [InlineData("sample.avi", 0)]
    [InlineData("sample.avi", 6 / 24d)]
    public void RenderAt_VaryingDecoder_ReturnsExpected(
        string sampleFileName,
        double position)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", sampleFileName));
        using var str = fi.OpenRead();
        using var sut = new FfmpegRenderer(str, [], [], 3);
        var ts = sut.Media.Duration * position;

        // Act
        var md5Hex = sut.RenderAt(ts).Rgb24Bytes.ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.ShouldNotBeEmpty();
    }

    [Theory]
    [InlineData(1, "3c820c3da3c2338ebfa6ab8f123eb9d4", 53)]
    [InlineData(100, "69186884f769afea14402e065e55fceb", 53)]
    [InlineData(500, "2252faedce7b6e2568ca21b8eb55b9db", 53)]
    [InlineData(72, "79f4d1aad4e61218528e6c6c8e5a3818", 53)]
    public void RenderAt_VaryingSize_ReturnsExpected(int height, string expectedMd5Hex, long expectedFrame)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mkv"));
        using var str = fi.OpenRead();
        using var sut = new FfmpegRenderer(str, [], [], height);
        var ts = sut.Media.Duration / 7;

        // Act
        var result = sut.RenderAt(ts);
        var md5Hex = result.Rgb24Bytes.ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.ShouldBe(expectedMd5Hex);
        result.FrameNumber.ShouldBe(expectedFrame);
    }

    [Theory]
    [InlineData("sample.mkv", 146.222, "505a6dd01934f9be1208237ee4c5802a")]
    public void RenderAt_VaryingPosition_ReturnsExpected(string file, double frameNo, string expectedMd5Hex)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", file));
        using var str = fi.OpenRead();
        using var sut = new FfmpegRenderer(str, [], [], null);
        var ts = sut.Media.Duration * frameNo / sut.Media.TotalFrames;

        // Act
        var frame = sut.RenderAt(ts);
        var md5Hex = frame.Rgb24Bytes.ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.ShouldBe(expectedMd5Hex);
    }

    [Fact]
    public void RenderAt_ForFile_ProducesFileApproxFrame()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
        using var str = fi.OpenRead();
        using var sut = new FfmpegRenderer(str, [], [], null);
        const double relative = 0.5;
        var ts = sut.Media.Duration * relative;
        var expectedFrame = (int)(sut.Media.TotalFrames * relative);

        // Act
        var result = sut.RenderAt(ts).FrameNumber;

        // Assert
        result.ShouldBeInRange(expectedFrame - 5, expectedFrame + 5);
    }

    /// <summary>
    /// Requests a seeks to EVERY SINGLE FRAME in the media and asserts that
    /// every resulting frame is within a tolerance.
    /// </summary>
    /// <param name="sampleFile">The sample file name.</param>
    /// <param name="frameCount">The count.</param>
    /// <param name="expectedTotalFrames">Expected total frames.</param>
    /// <param name="frameCsv">Expected frames.</param>
    [Theory]
    [InlineData("sample.avi", 10, 901, "2, 96, 196, 296, 396, 496, 596, 697, 797, 0")]
    [InlineData("sample.mkv", 10, 400, "0, 40, 85, 129, 174, 218, 263, 307, 352, 396")]
    [InlineData("sample.mp4", 10, 300, "0, 29, 63, 96, 129, 163, 196, 229, 263, 296")]
    public void RenderAt_FrameSweep_YieldsGoodFrameStats(
        string sampleFile,
        int frameCount,
        long expectedTotalFrames,
        string frameCsv)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", sampleFile));
        using var str = fi.OpenRead();
        using var sut = new FfmpegRenderer(str, [], [], null);
        var expectedFrames = frameCsv.NotNull().Split(',').Select(n => long.Parse(n, CultureInfo.InvariantCulture));

        // Act
        var frameData = sut.Media.Duration.DistributeEvenly(frameCount)
            .Select(req => new { req, rend = sut.RenderAt(req) })
            .Select(tup => new
            {
                tup.req,
                tup.rend,
                delta = Math.Abs((tup.req - tup.rend.Position).TotalMilliseconds),
            }).ToList();
        ////var worst = frameData.OrderByDescending(tup => tup.delta).First();
        ////var mean = frameData.Average(tup => tup.delta);
        var actualFrames = frameData.ConvertAll(tup => tup.rend.FrameNumber);

        // Assert
        ////mean.Should().BeLessThanOrEqualTo(75);
        ////worst.delta.Should().BeLessThanOrEqualTo(100);
        sut.Media.TotalFrames.ShouldBe(expectedTotalFrames);
        actualFrames.ShouldBeEquivalentTo(expectedFrames.ToList());
    }

    [Theory]
    [InlineData("sample.mp4")]
    [InlineData("60f02e749eb0898e69dafbf2fdc8eb9f7e5d71c85d010526e4514532f5b7aece.d1116ac481")]
    public void Ctor_VaryingFileSecurity_AffectsDecoder(string fileName)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", fileName));
        var expectedType = typeof(StreamFfmpegDecoding);

        // Act
        using var str = fi.OpenRead();
        var salt = fi.IsSecure() ? fi.ToSalt() : [];
        using var sut = new FfmpegRenderer(str, salt, [9, 0, 2, 1, 0], null);
        var decoderInfo = sut.GetType().GetField("decoder", BindingFlags.Instance | BindingFlags.NonPublic);
        var decoder = (IFfmpegDecodingSession)decoderInfo!.GetValue(sut)!;

        // Assert
        decoder.ShouldBeOfType(expectedType);
    }
}