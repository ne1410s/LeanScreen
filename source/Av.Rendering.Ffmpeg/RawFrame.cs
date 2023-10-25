﻿// <copyright file="RawFrame.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering.Ffmpeg;

/// <summary>
/// Raw frame data.
/// </summary>
public class RawFrame
{
    /// <summary>
    /// Gets or sets the rendered bytes, in RGB 24-byte format.
    /// </summary>
    public byte[] Rgb24Bytes { get; set; }

    /// <summary>
    /// Gets or sets the presentation time of the frame.
    /// </summary>
    public long PresentationTime { get; set; }
}
