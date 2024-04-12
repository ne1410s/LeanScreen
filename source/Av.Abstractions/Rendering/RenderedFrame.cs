// <copyright file="RenderedFrame.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering;

using System;
using Av.Common;

/// <summary>
/// A rendered frame.
/// </summary>
public class RenderedFrame
{
    /// <summary>
    /// Gets or sets the rendered bytes, in RGB 24-byte format.
    /// </summary>
    public ReadOnlyMemory<byte> Rgb24Bytes { get; set; }

    /// <summary>
    /// Gets or sets the frame dimensions.
    /// </summary>
    public Size2D Dimensions { get; set; }

    /// <summary>
    /// Gets or sets the chronological frame number.
    /// </summary>
    public long FrameNumber { get; set; }

    /// <summary>
    /// Gets or sets the presentation time of the frame.
    /// </summary>
    public TimeSpan Position { get; set; }
}
