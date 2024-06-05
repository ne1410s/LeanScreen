// <copyright file="MediaInfo.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering;

using System;
using LeanScreen.Common;

/// <summary>
/// Original media info.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MediaInfo"/> class.
/// </remarks>
/// <param name="duration">The duration.</param>
/// <param name="dimensions">The dimensions.</param>
/// <param name="totalFrames">The total number of frames.</param>
/// <param name="frameRate">The average frame rate.</param>
public class MediaInfo(TimeSpan duration, Size2D dimensions, long totalFrames, double frameRate)
{
    /// <summary>
    /// Gets the duration.
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// Gets the frame size.
    /// </summary>
    public Size2D Dimensions { get; } = dimensions;

    /// <summary>
    /// Gets the total number of frames.
    /// </summary>
    public long TotalFrames { get; } = totalFrames;

    /// <summary>
    /// Gets the average frame rate.
    /// </summary>
    public double FrameRate { get; } = Math.Round(frameRate, 4);
}
