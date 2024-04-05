// <copyright file="StreamFfmpegDecodingTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering.Ffmpeg.Tests.Decoding;

using System.Reflection;
using Av.Rendering.Ffmpeg.Decoding;
using Crypt.Streams;
using FFmpeg.AutoGen;

/// <summary>
/// Tests for the <see cref="StreamFfmpegDecoding"/>.
/// </summary>
public class StreamFfmpegDecodingTests
{
    public StreamFfmpegDecodingTests()
    {
        FfmpegUtils.SetupBinaries();
    }

    [Fact]
    public void Ctor_WhenCalled_SetsBinariesPath()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
        ffmpeg.RootPath = null;

        // Act
        using var str = new SimpleFileStream(fi);
        var x = new StreamFfmpegDecoding(str);

        // Assert
        ffmpeg.RootPath.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Ctor_WhenCalled_UriIsEmpty()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));

        // Act
        using var str = new SimpleFileStream(fi);
        var sut = new StreamFfmpegDecoding(str);

        // Assert
        sut.Url.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_FractionalFrameRate_ConvertsToDouble()
    {
        // Arrange
        const string fileName = "4a3a54004ec9482cb7225c2574b0f889291e8270b1c4d61dbc1ab8d9fef4c9e0.mp4";
        var fi = new FileInfo(Path.Combine("Samples", fileName));
        var key = new byte[] { 9, 0, 2, 1, 0 };

        // Act
        using var str = new CryptoBlockReadStream(fi, key);
        var sut = new StreamFfmpegDecoding(str);

        // Assert
        sut.FrameRate.Should().BeApproximately(23.962, 0.001);
    }

    [Fact]
    public void Ctor_WhenCalled_PopulatesDuration()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
        ffmpeg.RootPath = null;

        // Act
        using var str = new SimpleFileStream(fi);
        var decoding = new StreamFfmpegDecoding(str);

        // Assert
        decoding.Duration.Ticks.Should().Be(100000000);
    }

    [Fact]
    public void Dispose_WhenCalled_SetsNegativeStreamIndex()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
        using var str = new SimpleFileStream(fi);
        var sut = new StreamFfmpegDecoding(str);
        var indexInfo = sut.GetType().GetProperty("StreamIndex", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        sut.Dispose();
        var index = (int?)indexInfo!.GetValue(sut);

        // Assert
        index.Should().Be(-1);
    }

    [Fact]
    public void Dispose_WhenCalled_NullsTheStreamReadFunction()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
        using var str = new SimpleFileStream(fi);
        var sut = new StreamFfmpegDecoding(str);
        const BindingFlags fieldFlags = BindingFlags.NonPublic | BindingFlags.Instance;

        // Act
        sut.Dispose();

        // Assert
        var fnInfo = sut.GetType().GetField("readFn", fieldFlags);
        var fnValue = fnInfo!.GetValue(sut);
        fnValue.Should().BeNull();
    }

    [Fact]
    public void GetContextInfo_WhenCalled_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
        using var str = new SimpleFileStream(fi);
        var sut = new StreamFfmpegDecoding(str);

        // Act
        var info = sut.GetContextInfo();

        // Assert
        info.Should().NotBeEmpty();
    }

    [Fact]
    public void TryDecodeNextFrame_AtStart_ReturnsValidPosition()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
        var sut = new TestDecoding(fi.FullName);

        // Act
        var result = sut.Seek(default);

        // Assert
        result.pkt_pos.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void TryDecodeNextFrame_AfterEnd_ReturnsInvalidPosition()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
        var sut = new TestDecoding(fi.FullName);

        // Act
        var result = sut.Seek(TimeSpan.FromDays(21));

        // Assert
        result.pkt_pos.Should().BeLessThan(0);
    }

    private unsafe sealed class TestDecoding : FfmpegDecodingSessionBase
    {
        public TestDecoding(string url)
            : base(url)
        {
            this.OpenInputContext();
            var pFormatContext = this.PtrFormatContext;
            pFormatContext->seek2any = 1;
        }
    }
}
