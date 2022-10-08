// <copyright file="StreamFfmpegDecodingTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

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
        public void Dispose_WhenCalled_DoesNotError()
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", "sample.mp4"));
            var sut = new StreamFfmpegDecoding(new SimpleFileStream(fi));

            // Act
            var act = () => sut.Dispose();

            // Assert
            act.Should().NotThrow();
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
