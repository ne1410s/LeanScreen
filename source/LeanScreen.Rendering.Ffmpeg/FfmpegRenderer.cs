// <copyright file="FfmpegRenderer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg;

using System;
using System.IO;
using CryptoStream.Streams;
using LeanScreen.Common;
using LeanScreen.Rendering;
using LeanScreen.Rendering.Ffmpeg.Decoding;

/// <summary>
/// Ffmpeg frame renderer.
/// </summary>
public sealed class FfmpegRenderer : IRenderingSession
{
    private readonly StreamFfmpegDecoding decoder;
    private readonly FfmpegConverter converter;

    /// <summary>
    /// Initializes a new instance of the <see cref="FfmpegRenderer"/> class.
    /// </summary>
    /// <param name="stream">The source stream.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="key">The key.</param>
    /// <param name="height">The height.</param>
    public FfmpegRenderer(Stream stream, byte[] salt, byte[] key, int? height)
    {
        var readStream = salt?.Length > 0
            ? new CryptoBlockReadStream(stream, salt, key, true)
            : new BlockReadStream(stream);
        var codec = new StreamFfmpegDecoding(readStream);
        this.decoder = codec;
        this.ThumbSize = height == null ? codec.Dimensions : codec.Dimensions.ResizeTo(new() { Height = height.Value });
        this.Media = new(codec.Duration, codec.Dimensions, codec.TotalFrames, codec.FrameRate);
        this.converter = new(codec.Dimensions, codec.PixelFormat, this.ThumbSize);
    }

    /// <inheritdoc/>
    public MediaInfo Media { get; }

    /// <inheritdoc/>
    public Size2D ThumbSize { get; }

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
}
