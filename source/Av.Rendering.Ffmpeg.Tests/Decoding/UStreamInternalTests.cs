// <copyright file="UStreamInternalTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using Av.Rendering.Ffmpeg.Decoding;
using Crypt.Streams;
using FFmpeg.AutoGen;

namespace Av.Rendering.Ffmpeg.Tests.Decoding
{
    /// <summary>
    /// Tests for <see cref="UStreamInternal"/>.
    /// </summary>
    public class UStreamInternalTests
    {
        [Fact]
        public void Dispose_WithNullStream_DoesNotError()
        {
            // Arrange
            var sut = new UStreamInternal(null!);

            // Act
            var act = () => sut.Dispose();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Dispose_WithStream_DisposesInner()
        {
            // Arrange
            var mockInner = new Mock<ISimpleReadStream>();
            var sut = new UStreamInternal(mockInner.Object);

            // Act
            sut.Dispose();

            // Assert
            mockInner.Verify(m => m.Dispose(), Times.Once());
        }

        [Fact]
        public unsafe void ReadUnsafe_AtEnd_DoesNotCopy()
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", "sample.flv"));
            var mockCopier = new Mock<IByteArrayCopier>();
            var sut = new UStreamInternal(new SimpleFileStream(fi), mockCopier.Object);
            sut.SeekUnsafe(default, fi.Length, 0);

            // Act
            var result = sut.ReadUnsafe(default, default, 1);

            // Assert
            result.Should().Be(0);
            mockCopier.Verify(
                m => m.Copy(It.IsAny<byte[]>(), It.IsAny<IntPtr>(), It.IsAny<int>()),
                Times.Never());
        }

        [Fact]
        public unsafe void SeekUnsafe_Oversize_ReturnsEOF()
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", "sample.flv"));
            var sut = new UStreamInternal(new SimpleFileStream(fi));
            var expected = ffmpeg.AVERROR_EOF;

            // Act
            var result = sut.SeekUnsafe(default, long.MinValue, 17);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(ffmpeg.AVSEEK_SIZE, 0)]
        public unsafe void SeekUnsafe_VaryingWhence_CallsInnerSeekExpectedTimes(int whence, int expectedCalls)
        {
            // Arrange
            var innerMock = new Mock<ISimpleReadStream>();
            var sut = new UStreamInternal(innerMock.Object);
            const long position = 12;

            // Act
            sut.SeekUnsafe(default, position, whence);

            // Assert
            innerMock.Verify(m => m.Seek(position), Times.Exactly(expectedCalls));
        }

        [Fact]
        public unsafe void Ctor_WithStream_CanSeek()
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", "sample.flv"));

            // Act
            var sut = new UStreamInternal(new SimpleFileStream(fi));

            // Assert
            sut.CanSeek.Should().BeTrue();
        }
    }
}
