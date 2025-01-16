// <copyright file="StreamFfmpegDecodingTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Tests.Decoding;

using System.Reflection;
using CryptoStream.Streams;
using FFmpeg.AutoGen;
using LeanScreen.Rendering.Ffmpeg.Decoding;

/// <summary>
/// Tests for the <see cref="StreamFfmpegDecoding"/>.
/// </summary>
public class StreamFfmpegDecodingTests
{
    [Fact]
    public void Ctor_WhenCalled_SetsBinariesPath()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));

        // Act
        using var str = fi.OpenBlockRead();
        using var x = new StreamFfmpegDecoding(str);

        // Assert
        ffmpeg.RootPath.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Ctor_WhenCalled_UriIsEmpty()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));

        // Act
        using var str = fi.OpenBlockRead();
        using var sut = new StreamFfmpegDecoding(str);

        // Assert
        sut.Url.ShouldBeEmpty();
    }

    [Fact]
    public void Ctor_FractionalFrameRate_ConvertsToDouble()
    {
        // Arrange
        const string fileName = "60f02e749eb0898e69dafbf2fdc8eb9f7e5d71c85d010526e4514532f5b7aece.d1116ac481";
        var fi = new FileInfo(Path.Combine("Samples", fileName));
        var key = new byte[] { 9, 0, 2, 1, 0 };

        // Act
        using var str = fi.OpenCryptoRead(key);
        using var sut = new StreamFfmpegDecoding(str);

        // Assert
        sut.FrameRate.ShouldBeInRange(23.961, 23.963);
    }

    [Fact]
    public void Ctor_WhenCalled_PopulatesDuration()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));

        // Act
        using var str = fi.OpenBlockRead();
        using var decoding = new StreamFfmpegDecoding(str);

        // Assert
        decoding.Duration.Ticks.ShouldBe(100000000);
    }

    [Fact]
    public void Dispose_WhenCalled_SetsNegativeStreamIndex()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
        using var str = fi.OpenBlockRead();
        var sut = new StreamFfmpegDecoding(str);
        var indexInfo = sut.GetType().GetProperty("StreamIndex", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        sut.Dispose();
        var index = (int?)indexInfo!.GetValue(sut);

        // Assert
        index.ShouldBe(-1);
    }

    [Fact]
    public void Dispose_WhenCalled_NullsTheStreamReadFunction()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
        using var str = fi.OpenBlockRead();
        var sut = new StreamFfmpegDecoding(str);
        const BindingFlags fieldFlags = BindingFlags.NonPublic | BindingFlags.Instance;

        // Act
        sut.Dispose();

        // Assert
        var fnInfo = sut.GetType().GetField("readFn", fieldFlags);
        var fnValue = fnInfo!.GetValue(sut);
        fnValue.ShouldBeNull();
    }

    [Fact]
    public void GetContextInfo_WhenCalled_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
        using var str = fi.OpenBlockRead();
        using var sut = new StreamFfmpegDecoding(str);

        // Act
        var info = sut.GetContextInfo();

        // Assert
        info.ShouldNotBeEmpty();
    }

    [Fact]
    public void TryDecodeNextFrame_AtStart_ReturnsValidPosition()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
        using var sut = new TestDecoding(fi.FullName);

        // Act
        var result = sut.Seek(default);

        // Assert
        result.best_effort_timestamp.ShouldBe(0);
    }

    [Fact]
    public void TryDecodeNextFrame_AfterEnd_ReturnsInvalidPosition()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
        using var sut = new TestDecoding(fi.FullName);

        // Act
        var result = sut.Seek(TimeSpan.FromDays(21));

        // Assert
        result.best_effort_timestamp.ShouldBeLessThan(0);
    }

    private sealed unsafe class TestDecoding : FfmpegDecodingSessionBase
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
