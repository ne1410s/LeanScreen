// <copyright file="FfmpegRenderer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering.Ffmpeg
{
    using System;
    using System.IO;
    using Av.Abstractions.Rendering;
    using Av.Abstractions.Shared;
    using Av.Rendering.Ffmpeg.Decoding;
    using Crypt.IO;
    using Crypt.Streams;

    /// <summary>
    /// Ffmpeg frame renderer.
    /// </summary>
    public sealed class FfmpegRenderer : IRenderingService, IDisposable
    {
        private readonly IFfmpegDecodingSession decoder;
        private readonly FfmpegConverter converter;

        /// <summary>
        /// Initialises a new instance of the <see cref="FfmpegRenderer"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="key">The key (for cryptographic sources).</param>
        /// <param name="frameSize">The target frame size.</param>
        public FfmpegRenderer(string source, byte[] key = null, Dimensions2D? frameSize = null)
            : this(GetDecoder(source, key), frameSize)
        { }

        /// <summary>
        /// Initialises a new instance of the <see cref="FfmpegRenderer"/> class.
        /// </summary>
        /// <param name="decoder">The decoder.</param>
        /// <param name="frameSize">The target frame size.</param>
        public FfmpegRenderer(IFfmpegDecodingSession decoder, Dimensions2D? frameSize = null)
        {
            this.decoder = decoder ?? throw new ArgumentException("Required parameter is missing.", nameof(decoder));
            var finalFrameSize = frameSize == null ? decoder.Dimensions : decoder.Dimensions.ResizeTo(frameSize.Value);
            this.converter = new FfmpegConverter(decoder.Dimensions, decoder.PixelFormat, finalFrameSize);
            this.Media = new(decoder.Duration, decoder.Dimensions, decoder.TotalFrames, decoder.FrameRate);
            this.Session = new(finalFrameSize);
        }

        /// <inheritdoc/>
        public MediaInfo Media { get; }

        /// <inheritdoc/>
        public RenderSessionInfo Session { get; }

        /// <inheritdoc/>
        public RenderedFrame RenderAt(TimeSpan position)
        {
            var frame = this.decoder.Seek(position.Clamp(this.decoder.Duration));
            var rawFrame = this.converter.RenderRawFrame(frame);
            var actualPosition = ((double)rawFrame.PresentationTime).ToTimeSpan(this.decoder.TimeBase);
            var inferredFrame = this.Media.TotalFrames
                * (actualPosition.TotalSeconds / this.Media.Duration.TotalSeconds);

            return new RenderedFrame
            {
                Rgb24Bytes = rawFrame.Rgb24Bytes,
                Dimensions = this.Session.FrameSize,
                Position = actualPosition,
                FrameNumber = (long)Math.Round(inferredFrame),
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.converter.Dispose();
            this.decoder.Dispose();
        }

        private static IFfmpegDecodingSession GetDecoder(string source, byte[] key = null)
        {
            var fi = new FileInfo(source);
            return fi.IsSecure()
                ? new StreamFfmpegDecoding(new CryptoBlockReadStream(fi, key))
                : new PhysicalFfmpegDecoding(source);
        }
    }
}
