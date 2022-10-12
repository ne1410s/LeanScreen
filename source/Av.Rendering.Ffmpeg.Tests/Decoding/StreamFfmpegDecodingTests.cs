// <copyright file="StreamFfmpegDecodingTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using System.Reflection;
using Av.Rendering.Ffmpeg.Decoding;
using Crypt.Streams;
using FFmpeg.AutoGen;

namespace Av.Rendering.Ffmpeg.Tests.Decoding
{
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
            _ = new StreamFfmpegDecoding(new SimpleFileStream(fi));

            // Assert
            ffmpeg.RootPath.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Ctor_WhenCalled_UriIsEmpty()
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));

            // Act
            var sut = new StreamFfmpegDecoding(new SimpleFileStream(fi));

            // Assert
            sut.Url.Should().BeEmpty();
        }

        [Fact]
        public void Dispose_WhenCalled_SetsNegativeStreamIndex()
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
            var sut = new StreamFfmpegDecoding(new SimpleFileStream(fi));
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
            var sut = new StreamFfmpegDecoding(new SimpleFileStream(fi));
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
            var sut = new StreamFfmpegDecoding(new SimpleFileStream(fi));

            // Act
            var info = sut.GetContextInfo();

            // Assert
            info.Should().NotBeEmpty();
        }

        [Fact]
        public void TryDecodeNextFrame_AtStart_ReturnsTrue()
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
            var sut = new TestDecoding(fi.FullName);

            // Act
            var result = sut.TryDecodeNextFrame(out _);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void TryDecodeNextFrame_AtEnd_ReturnsFalse()
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
            var sut = new TestDecoding(fi.FullName);
            sut.Seek(TimeSpan.FromDays(21));

            // Act
            var result = sut.TryDecodeNextFrame(out _);

            // Assert
            result.Should().BeFalse();
        }

        private unsafe class TestDecoding : FfmpegDecodingSessionBase
        {
            public TestDecoding(string url)
                : base(url)
            {
                this.OpenInputContext();
                var pFormatContext = this.PtrFormatContext;
                pFormatContext->seek2any = 1;
            }

            public override void Seek(TimeSpan position)
            {
                const long max_ts = long.MaxValue;
                var ts = position.ToLong(this.TimeBase);
                ffmpeg.avformat_seek_file(this.PtrFormatContext, this.StreamIndex, long.MinValue, ts, max_ts, 0);
            }
        }
    }
}
