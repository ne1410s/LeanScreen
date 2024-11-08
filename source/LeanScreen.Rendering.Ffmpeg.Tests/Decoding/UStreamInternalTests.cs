// <copyright file="UStreamInternalTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Tests.Decoding;

using CryptoStream.Streams;
using FFmpeg.AutoGen;
using LeanScreen.Rendering.Ffmpeg.Decoding;
using LeanScreen.Rendering.Ffmpeg.IO;

/// <summary>
/// Tests for <see cref="UStreamInternal"/>.
/// </summary>
public class UStreamInternalTests
{
    ////[Fact]
    ////public void Dispose_WithStream_DisposesInner()
    ////{
    ////    // Arrange
    ////    var mockInner = new Mock<ISimpleReadStream>();
    ////    var sut = new UStreamInternal(mockInner.Object);

    ////    // Act
    ////    sut.Dispose();

    ////    // Assert
    ////    mockInner.Verify(m => m.Dispose(), Times.Once());
    ////}

    [Fact]
    public unsafe void ReadUnsafe_AtEnd_DoesNotCopy()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.flv"));
        var mockCopier = new Mock<IByteArrayCopier>();
        using var str = fi.OpenBlockRead();
        using var sut = new UStreamInternal(str, 333, mockCopier.Object);
        sut.SeekUnsafe(default, fi.Length, 0);

        // Act
        var result = sut.ReadUnsafe(default, default, 1);

        // Assert
        result.Should().BeLessThan(1);
        mockCopier.Verify(
            m => m.Copy(It.IsAny<byte[]>(), It.IsAny<IntPtr>(), It.IsAny<int>()),
            Times.Never());
    }

    [Fact]
    public unsafe void ReadUnsafe_WithCopier_CallsCopy()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.flv"));
        var mockCopier = new Mock<IByteArrayCopier>();
        using var str = fi.OpenBlockRead();
        using var sut = new UStreamInternal(str, 333, mockCopier.Object);

        // Act
        _ = sut.ReadUnsafe(default, default, 1);

        // Assert
        mockCopier.Verify(
            m => m.Copy(It.IsAny<byte[]>(), It.IsAny<IntPtr>(), It.IsAny<int>()));
    }

    [Fact]
    public unsafe void SeekUnsafe_Oversize_ReturnsEOF()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.flv"));
        using var str = fi.OpenBlockRead();
        using var sut = new UStreamInternal(str);
        var expected = ffmpeg.AVERROR_EOF;

        // Act
        var result = sut.SeekUnsafe(default, long.MinValue, 17);

        // Assert
        result.Should().Be(expected);
    }

    ////[Theory]
    ////[InlineData(0, 1)]
    ////[InlineData(ffmpeg.AVSEEK_SIZE, 0)]
    ////public unsafe void SeekUnsafe_VaryingWhence_CallsInnerSeekExpectedTimes(int whence, int expectedCalls)
    ////{
    ////    // Arrange
    ////    var innerMock = new Mock<ISimpleReadStream>();
    ////    using var sut = new UStreamInternal(innerMock.Object);
    ////    const long position = 12;

    ////    // Act
    ////    sut.SeekUnsafe(default, position, whence);

    ////    // Assert
    ////    innerMock.Verify(m => m.Seek(position), Times.Exactly(expectedCalls));
    ////}
}
