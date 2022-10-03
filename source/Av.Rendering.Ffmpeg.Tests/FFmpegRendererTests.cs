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
        [Theory]
        [InlineData("sample.avi", DecodeMode.PhysicalFm, 32768, 0, "3c99996ed49f0c7891aec69f48d93329")]
        [InlineData("sample.avi", DecodeMode.SimpleFile, 32768, 0, "3c99996ed49f0c7891aec69f48d93329")]
        [InlineData("sample.avi", DecodeMode.BlockReads, 32768, 0, "3c99996ed49f0c7891aec69f48d93329")]
        [InlineData("sample.avi", DecodeMode.PhysicalFm, 32768, 6 / 24d, "7c198dc342fd432cf30606c5f7281c6d")]
        [InlineData("sample.avi", DecodeMode.SimpleFile, 32768, 6 / 24d, "7c198dc342fd432cf30606c5f7281c6d")]
        [InlineData("sample.avi", DecodeMode.BlockReads, 32768, 6 / 24d, "7c198dc342fd432cf30606c5f7281c6d")]
        public void RenderAt_VaryingDecoder(
            string sampleFileName,
            DecodeMode decodeMode,
            int bufferLength,
            double position,
            string expectedMd5Hex)
        {
            // Arrange
            var fi = new FileInfo(Path.Combine("Samples", sampleFileName));
            var decoder = Get(decodeMode, fi, null, bufferLength);
            var sut = new FfmpegRenderer(decoder);
            var ts = decoder.Duration * position;

            // Act
            var md5Hex = sut.RenderAt(ts).Rgb24Bytes.Hash(HashType.Md5).Encode(Codec.ByteHex);

            // Assert
            md5Hex.Should().Be(expectedMd5Hex);
        }

        private static IFfmpegDecodingSession Get(DecodeMode mode, FileInfo fi, byte[]? key = null, int bufferLength = 32768) => mode switch
        {
            DecodeMode.Crypto => new StreamFfmpegDecoding(new CryptoBlockReadStream(fi, key, bufferLength)),
            DecodeMode.PhysicalFm => new PhysicalFfmpegDecoding(fi.FullName),
            DecodeMode.SimpleFile => new StreamFfmpegDecoding(new SimpleFileStream(fi, bufferLength)),
            DecodeMode.BlockReads => new StreamFfmpegDecoding(new BlockReadStream(fi, 32768)),
            _ => throw new ArgumentException("Decode mode not recognised"),
        };

        public enum DecodeMode
        {
            Crypto,
            PhysicalFm,
            SimpleFile,
            BlockReads
        }
    }
}