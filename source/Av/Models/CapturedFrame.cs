// <copyright file="CapturedFrame.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Models;

using System;
using System.IO;

/// <summary>
/// A captured frame, converted to an image stream.
/// </summary>
public sealed class CapturedFrame : IDisposable
{
    /// <summary>
    /// Gets or sets the image stream.
    /// </summary>
    public MemoryStream Image { get; set; }

    /// <summary>
    /// Gets or sets the sequence number.
    /// </summary>
    public int SequenceNumber { get; set; }

    /// <summary>
    /// Gets or sets the chronological frame number.
    /// </summary>
    public long FrameNumber { get; set; }

    /// <summary>
    /// Gets or sets the presentation time of the frame.
    /// </summary>
    public TimeSpan Position { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Image.Dispose();
        GC.SuppressFinalize(this);
    }
}
