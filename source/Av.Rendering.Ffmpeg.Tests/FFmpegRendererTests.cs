// <copyright file="FFmpegRendererTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using Av.Abstractions.Shared;
using Av.Rendering.Ffmpeg.Decoding;
using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.Streams;

namespace Av.Rendering.Ffmpeg.Tests
{
    /// <summary>
    /// Tests for the <see cref="FfmpegRenderer"/>.
    /// </summary>
    public class FFmpegRendererTests
    {
        private enum DecodeMode
        {
            Crypto,
            PhysicalFm,
            SimpleFile,
            BlockReads,
        }

        ////[Theory]
        ////[InlineData("sample.avi", DecodeMode.PhysicalFm, 32768, 0, "3c99996ed49f0c7891aec69f48d93329")]
        ////[InlineData("sample.avi", DecodeMode.SimpleFile, 32768, 0, "3c99996ed49f0c7891aec69f48d93329")]
        ////[InlineData("sample.avi", DecodeMode.BlockReads, 32768, 0, "3c99996ed49f0c7891aec69f48d93329")]
        ////[InlineData("sample.avi", DecodeMode.PhysicalFm, 32768, 6 / 24d, "7c198dc342fd432cf30606c5f7281c6d")]
        ////[InlineData("sample.avi", DecodeMode.SimpleFile, 32768, 6 / 24d, "7c198dc342fd432cf30606c5f7281c6d")]
        ////[InlineData("sample.avi", DecodeMode.BlockReads, 32768, 6 / 24d, "7c198dc342fd432cf30606c5f7281c6d")]
        ////public void RenderAt_VaryingDecoder_ReturnsExpected(
        ////    string sampleFileName,
        ////    DecodeMode decodeMode,
        ////    int bufferLength,
        ////    double position,
        ////    string expectedMd5Hex)
        ////{
        ////    // Arrange
        ////    var fi = new FileInfo(Path.Combine("Samples", sampleFileName));
        ////    var decoder = Get(decodeMode, fi, bufferLength);
        ////    var sut = new FfmpegRenderer(decoder);
        ////    var ts = decoder.Duration * position;

        ////    // Act
        ////    var md5Hex = sut.RenderAt(ts).Rgb24Bytes.Hash(HashType.Md5).Encode(Codec.ByteHex);

        ////    // Assert
        ////    md5Hex.Should().Be(expectedMd5Hex);
        ////}

        [Theory]
        [InlineData(1, 1, "d46e041b07eea036492b33315f9bbe1d")]
        [InlineData(500, 100, "3b1b21c9cf626ae54c2840c32a5b3ad6")]
        [InlineData(100, 500, "126d8039f3d4ac0282fa0ec9e9e24a4b")]
        public void RenderAt_VaryingSize_ReturnsExpected(int width, int height, string expectedMd5Hex)
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", "sample.mkv"));
            var decoder = Get(DecodeMode.BlockReads, fi);
            var sut = new FfmpegRenderer(decoder, new Dimensions2D { Width = width, Height = height });
            var ts = decoder.Duration / 7;

            // Act
            var md5Hex = sut.RenderAt(ts).Rgb24Bytes.Hash(HashType.Md5).Encode(Codec.ByteHex);

            // Assert
            md5Hex.Should().Be(expectedMd5Hex);
        }

        [Fact]
        public void Dispose_WhenCalled_DoesNotError()
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", "sample.avi"));
            var sut = new FfmpegRenderer(fi.FullName);

            // Act
            var act = () => sut.Dispose();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Ctor_NullDecoder_ThrowsException()
        {
            // Arrange & Act
            var act = () => new FfmpegRenderer((IFfmpegDecodingSession)null!);

            // Assert
            act.Should().ThrowExactly<ArgumentException>()
                .WithMessage("Required parameter is missing. (Parameter 'decoder')");
        }

        [Fact]
        public void InternalGet_UnrecognisedDecodeMode_ThrowsException()
        {
            // Arrange
            const DecodeMode decodeMode = (DecodeMode)999;

            // Act
            var act = () => Get(decodeMode, default!);

            // Assert
            act.Should().ThrowExactly<ArgumentException>()
                .WithMessage("Decode mode not recognised");
        }

        private static IFfmpegDecodingSession Get(DecodeMode mode, FileInfo fi, int bufferLength = 32768) => mode switch
        {
            DecodeMode.PhysicalFm => new PhysicalFfmpegDecoding(fi.FullName),
            DecodeMode.SimpleFile => new StreamFfmpegDecoding(new SimpleFileStream(fi, bufferLength)),
            DecodeMode.BlockReads => new StreamFfmpegDecoding(new BlockReadStream(fi, 32768)),
            _ => throw new ArgumentException("Decode mode not recognised"),
        };
    }
}