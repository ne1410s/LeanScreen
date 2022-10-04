using Av.Rendering.Ffmpeg.Decoding;
using Crypt.Streams;
using FFmpeg.AutoGen;

namespace Av.Rendering.Ffmpeg.Tests.Decoding
{
    /// <summary>
    /// Tests for <see cref="UStream"/>.
    /// </summary>
    public class UStreamTests
    {
        [Fact]
        public void Dispose_WithNullStream_DoesNotError()
        {
            // Arrange
            var sut = new UStream(null!);

            // Act
            var act = () => sut.Dispose();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public unsafe void Seek_Oversize_ReturnsEOF()
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", "sample.flv"));
            var sut = new UStream(new SimpleFileStream(fi));
            var expected = ffmpeg.AVERROR_EOF;

            // Act
            var result = sut.SeekUnsafe(default, long.MinValue, 17);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public unsafe void Ctor_WithStream_CanSeek()
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", "sample.flv"));

            // Act
            var sut = new UStream(new SimpleFileStream(fi));

            // Assert
            sut.CanSeek.Should().BeTrue();
        }
    }
}
