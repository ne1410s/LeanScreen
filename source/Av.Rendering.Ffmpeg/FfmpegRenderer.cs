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
    public sealed class FfmpegRenderer : IRenderingService
    {
        private IFfmpegDecodingSession decoder;
        private FfmpegConverter converter;

        /// <inheritdoc/>
        public MediaInfo Media { get; private set; }

        /// <inheritdoc/>
        public Size2D ThumbSize { get; private set; }

        /// <inheritdoc/>
        public void SetSource(string filePath, byte[] key, Size2D? thumbSize = null)
        {
            var codec = GetDecoder(filePath, key);
            this.decoder = codec;
            this.ThumbSize = thumbSize == null ? codec.Dimensions : codec.Dimensions.ResizeTo(thumbSize.Value);
            this.Media = new(codec.Duration, codec.Dimensions, codec.TotalFrames, codec.FrameRate);
            this.converter = new(codec.Dimensions, codec.PixelFormat, this.ThumbSize);
        }

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
                Dimensions = this.ThumbSize,
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
