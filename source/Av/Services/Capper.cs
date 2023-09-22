// <copyright file="Capper.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Services;

using System;
using System.IO;
using System.Linq;
using Av.Abstractions.Imaging;
using Av.Abstractions.Rendering;
using Av.Abstractions.Shared;

/// <summary>
/// Produces video captures.
/// </summary>
public sealed class Capper : IDisposable
{
    private readonly IRenderingService renderer;
    private readonly IImagingService imager;

    /// <summary>
    /// Initializes a new instance of the <see cref="Capper"/> class.
    /// </summary>
    /// <param name="renderer">The renderer.</param>
    /// <param name="imager">The imager.</param>
    public Capper(IRenderingService renderer, IImagingService imager)
    {
        this.renderer = renderer;
        this.imager = imager;
    }

    /// <summary>
    /// Gets the media.
    /// </summary>
    public MediaInfo Media { get; private set; }

    /// <summary>
    /// Sets the source.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="key">The key.</param>
    /// <param name="thumbSize">The thumb size. If omitted, the natural media size is used.</param>
    public void SetSource(string filePath, byte[] key, Size2D? thumbSize = null)
    {
        this.renderer.SetSource(filePath, key, thumbSize);
        this.Media = this.renderer.Media;
    }

    /// <summary>
    /// Gets a frame at the specified relative position.
    /// </summary>
    /// <param name="relative">The relative position.</param>
    /// <returns>An image frame.</returns>
    public CapturedFrame Snap(double relative)
        => this.Snap(this.ToAbsolute(relative));

    /// <summary>
    /// Gets a frame at the specified absolute time.
    /// </summary>
    /// <param name="absolute">The absolute time.</param>
    /// <returns>An image frame.</returns>
    public CapturedFrame Snap(TimeSpan absolute)
    {
        var frame = this.renderer.RenderAt(absolute);
        return new CapturedFrame
        {
            FrameNumber = frame.FrameNumber,
            Image = this.imager.Encode(frame.Rgb24Bytes, frame.Dimensions),
            Position = frame.Position,
            SequenceNumber = 1,
        };
    }

    /// <summary>
    /// Collates a set of captures.
    /// </summary>
    /// <param name="opts">The collation options.</param>
    /// <param name="total">The total.</param>
    /// <returns>A sequence of frames.</returns>
    public MemoryStream Collate(CollationOptions opts = null, int total = 24)
        => this.Collate(opts, this.Distribute(total));

    /// <summary>
    /// Collates a set of captures.
    /// </summary>
    /// <param name="opts">The collation options.</param>
    /// <param name="times">The times.</param>
    /// <returns>A sequence of frames.</returns>
    public MemoryStream Collate(CollationOptions opts = null, params TimeSpan[] times)
    {
        var frames = times.Select(this.renderer.RenderAt);
        opts ??= new();
        opts.ItemSize ??= this.renderer.ThumbSize;
        return this.imager.Collate(frames, opts);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.renderer?.Dispose();
        GC.SuppressFinalize(this);
    }

    private TimeSpan ToAbsolute(double position)
    {
        var milliseconds = this.renderer.Media.Duration.TotalMilliseconds;
        return TimeSpan.FromMilliseconds(milliseconds * position);
    }

    private TimeSpan[] Distribute(int total)
        => this.renderer.Media.Duration.DistributeEvenly(total);
}
