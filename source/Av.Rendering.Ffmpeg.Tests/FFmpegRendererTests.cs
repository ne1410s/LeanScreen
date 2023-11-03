// <copyright file="FFmpegRendererTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering.Ffmpeg.Tests;

using System.Globalization;
using System.Reflection;
using Av.Abstractions.Shared;
using Av.Rendering.Ffmpeg.Decoding;
using Crypt.Encoding;
using Crypt.Hashing;

/// <summary>
/// Tests for the <see cref="FfmpegRenderer"/>.
/// </summary>
public class FFmpegRendererTests
{
    public enum DecodeMode
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
        var sut = new FfmpegRenderer();
        sut.SetSource(fi.FullName, null, new Size2D(3, 3));
        var ts = sut.Media.Duration * position;

        // Act
        var md5Hex = sut.RenderAt(ts).Rgb24Bytes.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(1, 1, "2e7f411e0497d28729d257b46d9a29c1")]
    [InlineData(500, 100, "f3b76d9a332737b4e594c8750eb4dd16")]
    [InlineData(100, 500, "d0b76d42f1c7de683b271011f0ac898b")]
    [InlineData(0, 72, "79f4d1aad4e61218528e6c6c8e5a3818")]
    public void RenderAt_VaryingSize_ReturnsExpected(int width, int height, string expectedMd5Hex)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mkv"));
        var sut = new FfmpegRenderer();
        sut.SetSource(fi.FullName, null, new Size2D { Width = width, Height = height });
        var ts = sut.Media.Duration / 7;

        // Act
        var result = sut.RenderAt(ts);
        var md5Hex = result.Rgb24Bytes.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.Should().Be(expectedMd5Hex);
    }

    [Theory]
    [InlineData("sample.mkv", 146.222, "505a6dd01934f9be1208237ee4c5802a")]
    public void RenderAt_VaryingPosition_ReturnsExpected(string file, double frameNo, string expectedMd5Hex)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", file));
        var sut = new FfmpegRenderer();
        sut.SetSource(fi.FullName, null);
        var ts = sut.Media.Duration * frameNo / sut.Media.TotalFrames;

        // Act
        var frame = sut.RenderAt(ts);
        var md5Hex = frame.Rgb24Bytes.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.Should().Be(expectedMd5Hex);
    }

    [Fact]
    public void RenderAt_ForFile_ProducesFileApproxFrame()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
        var sut = new FfmpegRenderer();
        sut.SetSource(fi.FullName, null);
        const double relative = 0.5;
        var ts = sut.Media.Duration * relative;
        var expectedFrame = (int)(sut.Media.TotalFrames * relative);

        // Act
        var result = sut.RenderAt(ts).FrameNumber;

        // Assert
        result.Should().BeCloseTo(expectedFrame, 10);
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
        var sut = new FfmpegRenderer();
        sut.SetSource(fi.FullName, null);
        var expectedFrames = frameCsv.Split(',').Select(n => long.Parse(n, CultureInfo.InvariantCulture));

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
        sut.Media.TotalFrames.Should().Be(expectedTotalFrames);
        actualFrames.Should().BeEquivalentTo(expectedFrames);
    }

    [Theory]
    [InlineData("sample.mp4", true)]
    [InlineData("4a3a54004ec9482cb7225c2574b0f889291e8270b1c4d61dbc1ab8d9fef4c9e0.mp4", false)]
    public void Ctor_VaryingFileSecurity_AffectsDecoder(string fileName, bool expectPhysicalDecoder)
    {
        // Arrange
        var filePath = Path.Combine("Samples", fileName);
        var expectedType = expectPhysicalDecoder
            ? typeof(PhysicalFfmpegDecoding)
            : typeof(StreamFfmpegDecoding);

        // Act
        var sut = new FfmpegRenderer();
        sut.SetSource(filePath, new byte[] { 9, 0, 2, 1, 0 });
        var decoderInfo = sut.GetType().GetField("decoder", BindingFlags.Instance | BindingFlags.NonPublic);
        var decoder = (IFfmpegDecodingSession)decoderInfo!.GetValue(sut)!;

        // Assert
        decoder.Should().BeOfType(expectedType);
    }
}