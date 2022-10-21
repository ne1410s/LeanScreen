// <copyright file="FFmpegRendererTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
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

        [Fact]
        public void RenderAt_WhenCalled_CallsSeek()
        {
            // Arrange
            FfmpegUtils.SetupBinaries();
            var mockDecoder = new Mock<IFfmpegDecodingSession>();
            mockDecoder.Setup(m => m.Dimensions).Returns(new Size2D { Width = 1, Height = 1 });
            mockDecoder.Setup(m => m.Duration).Returns(TimeSpan.FromSeconds(100));
            var sut = new FfmpegRenderer(mockDecoder.Object);
            var ts = TimeSpan.FromSeconds(1.34);

            // Act
            sut.RenderAt(ts);

            // Assert
            mockDecoder.Verify(m => m.Seek(ts), Times.Once());
        }

        [Theory]
        [InlineData("sample.avi", DecodeMode.PhysicalFm, 32768, 0)]
        [InlineData("sample.avi", DecodeMode.SimpleFile, 32768, 0)]
        [InlineData("sample.avi", DecodeMode.BlockReads, 32768, 0)]
        [InlineData("sample.avi", DecodeMode.PhysicalFm, 32768, 6 / 24d)]
        [InlineData("sample.avi", DecodeMode.SimpleFile, 32768, 6 / 24d)]
        [InlineData("sample.avi", DecodeMode.BlockReads, 32768, 6 / 24d)]
        public void RenderAt_VaryingDecoder_ReturnsExpected(
            string sampleFileName,
            DecodeMode decodeMode,
            int bufferLength,
            double position)
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", sampleFileName));
            var decoder = Get(decodeMode, fi, bufferLength);
            var sut = new FfmpegRenderer(decoder, new Size2D(3, 3));
            var ts = decoder.Duration * position;

            // Act
            var md5Hex = sut.RenderAt(ts).Rgb24Bytes.Hash(HashType.Md5).Encode(Codec.ByteHex);

            // Assert
            md5Hex.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(1, 1, "d46e041b07eea036492b33315f9bbe1d")]
        [InlineData(500, 100, "3b1b21c9cf626ae54c2840c32a5b3ad6")]
        [InlineData(100, 500, "126d8039f3d4ac0282fa0ec9e9e24a4b")]
        [InlineData(0, 72, "2dd40cdc29f03f09523dfcd2185d396d")]
        public void RenderAt_VaryingSize_ReturnsExpected(int width, int height, string expectedMd5Hex)
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", "sample.mkv"));
            var decoder = Get(DecodeMode.BlockReads, fi);
            var sut = new FfmpegRenderer(decoder, new Size2D { Width = width, Height = height });
            var ts = decoder.Duration / 7;

            // Act
            var result = sut.RenderAt(ts);
            var md5Hex = result.Rgb24Bytes.Hash(HashType.Md5).Encode(Codec.ByteHex);

            // Assert
            md5Hex.Should().Be(expectedMd5Hex);
        }

        [Theory]
        [InlineData("sample.mkv", 146.222, "be5b4daac7a633cf5f9ea7c9575273ad")]
        public void RenderAt_VaryingPosition_ReturnsExpected(string file, double frameNo, string expectedMd5Hex)
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", file));
            var decoder = Get(DecodeMode.PhysicalFm, fi);
            var sut = new FfmpegRenderer(decoder);
            var ts = decoder.Duration * frameNo / decoder.TotalFrames;

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
            var decoder = Get(DecodeMode.BlockReads, fi);
            var sut = new FfmpegRenderer(decoder);
            const double relative = 0.5;
            var ts = decoder.Duration * relative;
            var expectedFrame = (int)(decoder.TotalFrames * relative);

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
        /// <param name="decodeMode">The decode mode.</param>
        /// <param name="frameCount">The count.</param>
        /// <param name="expectedTotalFrames">Expected total frames.</param>
        /// <param name="frameCsv">Expected frames.</param>
        [Theory]
        [InlineData("sample.avi", DecodeMode.PhysicalFm, 10, 345, "2,35,74,112,150,189,226,265,304,342")]
        [InlineData("sample.mkv", DecodeMode.PhysicalFm, 10, 658, "153,153,153,216,289,362,436,509,582,655")]
        [InlineData("sample.mp4", DecodeMode.PhysicalFm, 10, 973, "0,106,215,323,431,539,647,755,864,972")]
        public void RenderAt_FrameSweep_YieldsGoodFrameStats(
            string sampleFile,
            DecodeMode decodeMode,
            int frameCount,
            long expectedTotalFrames,
            string frameCsv)
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", sampleFile));
            var decoder = Get(decodeMode, fi);
            var sut = new FfmpegRenderer(decoder);
            var expectedFrames = frameCsv.Split(',').Select(n => long.Parse(n, CultureInfo.InvariantCulture));

            // Act
            var frameData = decoder.Duration.DistributeEvenly(frameCount)
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
            decoder.TotalFrames.Should().Be(expectedTotalFrames);
            actualFrames.Should().BeEquivalentTo(expectedFrames);
        }

        [Fact]
        public void Dispose_WhenCalled_DisposesDecoder()
        {
            // Arrange
            FfmpegUtils.SetupBinaries();
            var mockDecoder = new Mock<IFfmpegDecodingSession>();
            mockDecoder.Setup(m => m.Dimensions).Returns(new Size2D { Width = 1, Height = 1 });
            var sut = new FfmpegRenderer(mockDecoder.Object);

            // Act
            sut.Dispose();

            // Assert
            mockDecoder.Verify(m => m.Dispose(), Times.Once());
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
        public void Ctor_Resized_SetsExpectedFrameSize()
        {
            // Arrange
            var mockDecoder = new Mock<IFfmpegDecodingSession>();
            mockDecoder.Setup(m => m.Dimensions).Returns(new Size2D(7, 11));
            var resizeTo = new Size2D { Width = 70 };
            var expected = new Size2D(70, 110);

            // Act
            var sut = new FfmpegRenderer(mockDecoder.Object, resizeTo);

            // Assert
            sut.Session.FrameSize.Should().Be(expected);
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
            var sut = new FfmpegRenderer(filePath, new byte[] { 9, 0, 2, 1, 0 });
            var decoderInfo = sut.GetType().GetField("decoder", BindingFlags.Instance | BindingFlags.NonPublic);
            var decoder = (IFfmpegDecodingSession)decoderInfo!.GetValue(sut)!;

            // Assert
            decoder.Should().BeOfType(expectedType);
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