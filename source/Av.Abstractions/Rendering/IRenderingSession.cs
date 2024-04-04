// <copyright file="IRenderingSession.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering;

using System;
using Av.Common;

/// <summary>
/// Rendering session.
/// </summary>
public interface IRenderingSession : IDisposable
{
    /// <summary>
    /// Gets the media info.
    /// </summary>
    public MediaInfo Media { get; }

    /// <summary>
    /// Gets the target thumb size.
    /// </summary>
    public Size2D ThumbSize { get; }

    /// <summary>
    /// Renders a frame at the specified position.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <returns>A rendered frame.</returns>
    public RenderedFrame RenderAt(TimeSpan position);
}
