using System;
using System.IO;
using Av.Abstractions.Rendering;
using Av.Abstractions.Shared;
using Av.Rendering.Ffmpeg.Decoding;
using Crypt.IO;
using Crypt.Streams;

namespace Av.Rendering.Ffmpeg
{
    /// <summary>
    /// Ffmpeg frame renderer.
    /// </summary>
    public class FfmpegRenderer : IRenderingService, IDisposable
    {
        private readonly IFfmpegDecodingSession decoder;
        private readonly FfmpegConverter converter;

        /// <summary>
        /// Initialises a new <see cref="FfmpegRenderer"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="key">The key (for cryptographic sources).</param>
        /// <param name="frameSize">The target frame size.</param>
        public FfmpegRenderer(string source, byte[] key = null, Dimensions2D? frameSize = null)
            : this(GetDecoder(source, key), frameSize)
        { }

        /// <summary>
        /// Initialises a new <see cref="FfmpegRenderer"/>.
        /// </summary>
        /// <param name="decoder">The decoder.</param>
        /// <param name="frameSize">The target frame size.</param>
        public FfmpegRenderer(IFfmpegDecodingSession decoder, Dimensions2D? frameSize = null)
        {
            this.decoder = decoder ?? throw new ArgumentException("Required parameter is missing.", nameof(decoder));
            FrameSize = frameSize ?? decoder.Dimensions;
            Duration = decoder.Duration;
            TotalFrames = decoder.TotalFrames;
            converter = new FfmpegConverter(decoder.Dimensions, decoder.PixelFormat, FrameSize);
        }

        /// <inheritdoc/>
        public TimeSpan Duration { get; }

        /// <inheritdoc/>
        public Dimensions2D FrameSize { get; }

        /// <inheritdoc/>
        public long TotalFrames { get; }

        /// <inheritdoc/>
        public RenderedFrame RenderAt(TimeSpan position)
        {
            decoder.Seek(position.Clamp(decoder.Duration));
            decoder.TryDecodeNextFrame(out var frame);
            var rawFrame = converter.RenderRawFrame(frame);
            var actualPosition = rawFrame.PresentationTime.ToTimeSpan(decoder.TimeBase);
            var inferredFrame = TotalFrames * (actualPosition.TotalSeconds / Duration.TotalSeconds);

            return new RenderedFrame
            {
                Rgb24Bytes = rawFrame.Rgb24Bytes,
                Dimensions = FrameSize,
                Position = actualPosition,
                FrameNumber = (long)Math.Round(inferredFrame),
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            converter.Dispose();
            decoder.Dispose();
        }

        private static IFfmpegDecodingSession GetDecoder(string source, byte[] key = null)
        {
            var fi = new FileInfo(source);
            return fi.IsSecure()
                ? new StreamFfmpegDecoding(new CryptoBlockReadStream(fi, key))
                : (IFfmpegDecodingSession)new PhysicalFfmpegDecoding(source);
        }
    }
}
