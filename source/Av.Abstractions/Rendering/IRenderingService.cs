// <copyright file="IRenderingService.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Abstractions.Rendering;

using System;
using Av.Abstractions.Shared;

/// <summary>
/// Obtains image stills from a video.
/// </summary>
public interface IRenderingService : IDisposable
{
    /// <summary>
    /// Gets source media info.
    /// </summary>
    MediaInfo Media { get; }

    /// <summary>
    /// Gets the target thumb size.
    /// </summary>
    public Size2D ThumbSize { get; }

    /// <summary>
    /// Sets the source.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="key">The key.</param>
    /// <param name="thumbSize">The thumb size. If omitted, the natural media size is used.</param>
    public void SetSource(string filePath, byte[] key, Size2D? thumbSize = null);

    /// <summary>
    /// Renders a frame at the position specified.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <returns>The rendered frame.</returns>
    RenderedFrame RenderAt(TimeSpan position);
}
